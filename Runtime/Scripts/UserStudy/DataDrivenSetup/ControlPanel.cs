using System;
using System.Collections;
using System.Collections.Generic;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using DistractorTask.Transport.DataContainer.GenericClasses;
using DistractorTask.UserStudy.MarkerPointStage;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace DistractorTask.UserStudy.DataDrivenSetup
{
    public class ControlPanel : MonoBehaviour
    {

        [SerializeField]
        private MarkerPointVisualizationController markerPointController;
        
        public Study[] studies = Array.Empty<Study>();
        
        [SerializeField]
        private int markerPointCount = 6;
        
        [SerializeField, Tooltip("Determines the type of participant / order of conditions for the given study")]
        private int startingCondition = 0;

        private MarkerPointEnumerator _markerPointEnumerator;

        private StudyConditionsEnumerator _enumerator;
        public Action<string> OnStudyPhaseStart = delegate { };
        public Action<int, int> OnNextIteration = delegate { };

        #region MarkerPointRegion

        public async void StartMarkerPointCreation()
        {
            OnStudyPhaseStart.Invoke("Marker Point Creation Phase");
            _markerPointEnumerator?.Dispose();
            _markerPointEnumerator = new MarkerPointEnumerator(markerPointCount);
            NetworkManager.Instance.MulticastMessage(new MarkerPointCountData
            {
                markerCount = markerPointCount
            }, NetworkExtensions.DefaultPort, GetInstanceID());
            await markerPointController.InitializeMarkerPointSetup(markerPointCount);
            
            

            while (_markerPointEnumerator.MoveNext())
            {
                
                var markerPointIndex = _markerPointEnumerator.Current;
                OnNextIteration.Invoke(markerPointIndex, markerPointCount);
                await markerPointController.TriggerNextPoint(markerPointIndex);
                await NetworkManager.Instance
                    .MulticastMessageAndAwaitResponse<OnMarkerPointActivatedData, OnAnchorPointSelectionData>(
                        new OnMarkerPointActivatedData
                        {
                            MarkerPointIndex = markerPointIndex
                        }, NetworkExtensions.DefaultPort, GetInstanceID(), markerPointIndex);
                Debug.Log("Marker point done");
            }
            
            //todo make sure that the start / end thing works.
            //I think we can replace start with the actual number element,
            //but end is probably still necessary if we want to adjust the process to a specific marker / set the desired index

            await markerPointController.EndMarkerPointSetup();
            
            _markerPointEnumerator.Dispose();
            _markerPointEnumerator = null;
        }

        public void RegenerateMarkers()
        {
            if (_markerPointEnumerator == null)
            {
                Debug.LogError("Marker Point Setup hasn't been started yet.");
                return;
            }
            _markerPointEnumerator.Reset();
        }

        public void RepeatPreviousPoint()
        {
            if (_markerPointEnumerator == null)
            {
                Debug.LogError("Marker Point Setup hasn't been started yet.");
                return;
            }
            _markerPointEnumerator.MovePrevious();
        }
     
#endregion
        

        //todo look if we have to cancel tasks somehow


        public async void StartStudy()
        {
            OnStudyPhaseStart.Invoke("Study Phase");
            _enumerator = new StudyConditionsEnumerator(GetCurrentStudy(), startingCondition);

            var unregisterCallback = NetworkManager.Instance.RegisterPersistentMulticastResponse<TrialCompletedData, TrialCompletedResponseData>(
                OnTrialCompleted, NetworkExtensions.DefaultPort, GetInstanceID());
            
            while (_enumerator.MoveNext())
            {
                var studyCondition = _enumerator.Current;
                
                //todo set correct message id. Also maybe broadcast and make client understand that it has to wait for confirmation
                await NetworkManager.Instance
                    .MulticastMessageAndAwaitResponse<StudyConditionData, OnVideoClipChangedData>(
                        new StudyConditionData
                        {
                            studyCondition = studyCondition
                        }, NetworkExtensions.DisplayWallControlPort, GetInstanceID(),
                        0);
                
                //todo await trial complete -> send message to video player?
                await NetworkManager.Instance
                    .MulticastMessageAndAwaitResponse<ConditionData, OnConditionCompleted>(
                        new ConditionData
                        {
                            studyCondition = studyCondition
                        }, NetworkExtensions.DefaultPort, GetInstanceID(),
                        1);
                
                //todo await button presses?
                /*await NetworkManager.Instance.AwaitMessageAndRespond<OnTrialCompleted, ChangeVideoClipData>(TestAction,
                    ConnectionType.Multicast, NetworkExtensions.DefaultPort, ConnectionType.Multicast,
                    NetworkEndpoint.AnyIpv4.WithPort(NetworkExtensions.DisplayWallControlPort), GetInstanceID());*/
            }
            
            unregisterCallback.Invoke();
        }

        private async void OnTrialCompleted(TrialCompletedData arg1, int arg2)
        {
            await NetworkManager.Instance
                .MulticastMessageAndAwaitResponse<UpdateVideoClipData, OnVideoClipChangedData>(
                    new UpdateVideoClipData(), NetworkExtensions.DisplayWallControlPort, GetInstanceID(),
                    0);
            
            //todo do we even want to do anything in here? 
            //todo try awaitable thing in here and make sure that it delays the response 
            //todo try to adjust values on TrialCompletedData to make sure that those actually make their way across 
        }
        //todo register persistent callback for trial end data? this way we can send new data without interrupting the current study process?
        

        private Study GetCurrentStudy()
        {
            return studies[0];
        }
        
    }
    

    public class OnConditionCompleted : BaseResponseData
    {
    }

    public class TrialCompletedData : BaseRespondingData<TrialCompletedResponseData>
    {
        
    }

    public class TrialCompletedResponseData : BaseResponseData
    {
    }

    [Serializable]
    public class ConditionData : BaseRespondingData<OnConditionCompleted>
    {
        public StudyCondition studyCondition;

        public override void Serialize(ref DataStreamWriter writer)
        {
            base.Serialize(ref writer);
            writer.WriteStudyCondition(studyCondition);
        }

        public override void Deserialize(ref DataStreamReader reader)
        {
            base.Deserialize(ref reader);
            studyCondition = reader.ReadStudyCondition();
        }
    }

    public class UpdateVideoClipData : BaseRespondingData<OnVideoClipChangedData>
    {
        
    }

    public class OnVideoClipChangedData : BaseResponseData
    {
    }

    [Serializable]
    public class StudyConditionData : BaseRespondingData<OnVideoClipChangedData>
    {
        public StudyCondition studyCondition;
        
        public override void Serialize(ref DataStreamWriter writer)
        {
            base.Serialize(ref writer);
            writer.WriteStudyCondition(studyCondition);
        }

        public override void Deserialize(ref DataStreamReader reader)
        {
            base.Deserialize(ref reader);
            studyCondition = reader.ReadStudyCondition();
        }
    }

    internal class MarkerPointEnumerator : IEnumerator<int>
    {

        private readonly int _markerCount;
        private int _currentMarker = -1;

        internal MarkerPointEnumerator(int markerCount)
        {
            _markerCount = markerCount;
        }
        
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public bool MoveNext()
        {
            _currentMarker++;
            return _currentMarker < _markerCount;
        }

        public void Reset()
        {
            _currentMarker = -1;
        }

        public bool MovePrevious()
        {
            _currentMarker = math.max(_currentMarker--, -1);
            return _currentMarker < _markerCount && _currentMarker >= 0;
        }

        public void SetMarker(int nextMarkerPosition)
        {
            _currentMarker = nextMarkerPosition - 1;
        }
        
        public int Current => _currentMarker;

        object IEnumerator.Current => Current;
    }
    

}