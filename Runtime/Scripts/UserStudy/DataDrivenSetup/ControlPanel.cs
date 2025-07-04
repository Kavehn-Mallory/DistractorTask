using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DistractorTask.Logging;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using DistractorTask.Transport.DataContainer.GenericClasses;
using DistractorTask.UserStudy.MarkerPointStage;
using TMPro;
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
        
        [SerializeField]
        private TMP_Dropdown participantDropdown;

        private MarkerPointEnumerator _markerPointEnumerator;

        private StudyConditionsEnumerator _enumerator;
        public Action<string, int> OnStudyPhaseStart = delegate { };
        public Action<string> OnStudyPhaseEnd = delegate { };
        public Action OnStudyCompleted = delegate { };
        
        /// <summary>
        /// Triggers on each iteration. String represents the name of the study stage, first int is the current index, second int is the iteration count
        /// </summary>
        public Action<string, int, int> OnIterationCompleted = delegate { };
        private StudyEnumerator _studyEnumerator;

        private const string MarkerPointPhaseName = "Marker Point Creation Phase";
        private const string StudyPhaseName = "Study Phase";

        private NetworkManagerExtensions.ResponseCallback<OnVideoClipChangedData> _videoWallCommunicationTask;
        private NetworkManagerExtensions.ResponseCallback<OnConditionCompleted> _hmdCommunicationTask;

        private void Awake()
        {
            _studyEnumerator = new StudyEnumerator(studies);
            NetworkManager.Instance.RegisterToConnectionStateChange(NetworkExtensions.DisplayWallControlPort, OnDisplayWallConnectionStateChanged);
            NetworkManager.Instance.RegisterToConnectionStateChange(NetworkExtensions.DefaultPort, OnHmdConnectionStateChanged);

            foreach (var study in studies)
            {
                var permutationCount = PermutationGenerator.GeneratePermutations(study.conditions, 0).Length;
                Debug.Log($"Study {study.studyName} has {permutationCount} permutations");
                
                _enumerator = new StudyConditionsEnumerator(study);
            
                Debug.Log($"Study Condition {_enumerator.CurrentPermutationIndex} out of {_enumerator.PermutationCount}");
            }

            _enumerator = null;
        }

        private void OnHmdConnectionStateChanged(ConnectionState obj)
        {
            if (obj != ConnectionState.Connected && _hmdCommunicationTask != null)
            {
                _hmdCommunicationTask.CancelTask();
            }
        }

        private void OnDisplayWallConnectionStateChanged(ConnectionState obj)
        {
            if (obj != ConnectionState.Connected && _videoWallCommunicationTask != null)
            {
                _videoWallCommunicationTask.CancelTask();
            }
            
        }


        #region MarkerPointRegion

        public async void StartMarkerPointCreation()
        {
            OnStudyPhaseStart.Invoke(MarkerPointPhaseName, 0);
            _markerPointEnumerator?.Dispose();
            _markerPointEnumerator = new MarkerPointEnumerator(markerPointCount);
            NetworkManager.Instance.MulticastMessage(new MarkerPointCountData
            {
                markerCount = markerPointCount
            }, NetworkExtensions.DefaultPort, GetInstanceID());
            await markerPointController.InitializeMarkerPointSetup(markerPointCount);
            
            LoggingComponent.Log(LogData.CreateMarkerPointBeginLogData(markerPointCount));

            while (_markerPointEnumerator.MoveNext())
            {
                
                var markerPointIndex = _markerPointEnumerator.Current;
                OnIterationCompleted.Invoke("Marker Point", markerPointIndex, markerPointCount);
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
            LoggingComponent.Log(LogData.CreateMarkerPointEndLogData());
            Debug.Log("Marker points are over");
            OnStudyPhaseEnd.Invoke(MarkerPointPhaseName);
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

        public void AdvanceStudy()
        {
            //end any study that is still ongoing 
            NetworkManager.Instance.BroadcastMessage(new StudyEndData(), GetInstanceID());
            if (!_studyEnumerator.MoveNext())
            {
                OnStudyCompleted.Invoke();
                return;
            }
            
            var startingCondition = participantDropdown.value;
            OnStudyPhaseStart.Invoke($"{StudyPhaseName}", _studyEnumerator.CurrentStudyIndex);
            _enumerator = new StudyConditionsEnumerator(_studyEnumerator.Current, startingCondition);
            
            LoggingComponent.Log(LogData.CreateStudyBeginLogData(_studyEnumerator.Current.studyName, _studyEnumerator.CurrentStudyIndex, TransformCurrentConditionToLetter(startingCondition, _enumerator.PermutationCount).ToString()));

        }


        public void ResetCurrentStudy()
        {
            _enumerator?.Reset();
        }

        public void RestartCurrentCondition()
        {
            if (_enumerator != null)
            {
                _hmdCommunicationTask = null;
                _videoWallCommunicationTask = null;
                _enumerator.MovePrevious();
                StartStudy();
            }
        }
        

        public async void StartStudy()
        {

            if (_enumerator == null)
            {
                Debug.LogWarning($"Study has not been initialized. Call {nameof(AdvanceStudy)} before calling {nameof(StartStudy)}");
                return;
            }


            var unregisterCallback = NetworkManager.Instance.RegisterPersistentMulticastResponse<TrialCompletedData, TrialCompletedResponseData>(
                OnTrialCompleted, NetworkExtensions.DefaultPort, GetInstanceID());
            
            OnIterationCompleted.Invoke("Study Condition", -1, _enumerator.PermutationCount);
            
            while (_enumerator.MoveNext())
            {
                var studyCondition = _enumerator.Current;
                Debug.Log($"Study condition {_enumerator.CurrentPermutationIndex} out of {_enumerator.PermutationCount}");
                
                
                
                Debug.Log($"Current Study Load Level: {studyCondition.loadLevel.ToString()}; Study Noise Level: {studyCondition.noiseLevel.ToString()}");
                Debug.Log($"Awaiting response with sender id {GetInstanceID()} and message id {_enumerator.CurrentPermutationIndex}");
                _videoWallCommunicationTask = NetworkManager.Instance
                    .MulticastMessageAndAwaitResponseWithInterrupt<StudyConditionVideoInfoData, OnVideoClipChangedData>(
                        new StudyConditionVideoInfoData
                        {
                            studyCondition = studyCondition
                        }, NetworkExtensions.DisplayWallControlPort, GetInstanceID(),
                        _enumerator.CurrentPermutationIndex);

                await _videoWallCommunicationTask.AwaitMessage();

                if (!_videoWallCommunicationTask.IsCompletedSuccessfully)
                {
                    //todo do we just restart the enumerator at the specified index?
                    Debug.LogError("Error while trying to switch video clip");
                    _enumerator.MovePrevious();
                    return;
                }

                _videoWallCommunicationTask = null;
                Debug.Log("Video clip was selected. Sending data to HMD");
                _hmdCommunicationTask =  NetworkManager.Instance
                    .MulticastMessageAndAwaitResponseWithInterrupt<ConditionData, OnConditionCompleted>(
                        new ConditionData
                        {
                            studyCondition = studyCondition
                        }, NetworkExtensions.DefaultPort, GetInstanceID(),
                        _enumerator.CurrentPermutationIndex);
                
                await _hmdCommunicationTask.AwaitMessage();

                if (!_hmdCommunicationTask.IsCompletedSuccessfully)
                {
                    //todo do we just restart the enumerator at the specified index?
                    _enumerator.MovePrevious();
                    Debug.LogError("Error while trying to perform trial");
                    return;
                }
                _hmdCommunicationTask = null;
                
                OnIterationCompleted.Invoke("Study Condition", _enumerator.CurrentPermutationIndex, _enumerator.PermutationCount);

            }
            Debug.Log("Study Phase Ended");
            LoggingComponent.Log(LogData.CreateStudyEndLogData());
            _enumerator = null;
            NetworkManager.Instance.BroadcastMessage(new StudyEndData(), GetInstanceID());
            OnStudyPhaseEnd.Invoke(StudyPhaseName);
            unregisterCallback.Invoke();
        }

        private static char TransformCurrentConditionToLetter(int currentCondition, int permutationCount)
        {
            //assumes that the conditions start with 0 instead of 1
            currentCondition %= permutationCount;
            return (char)(currentCondition + 65);
        }

        private async void OnTrialCompleted(TrialCompletedData arg1, int arg2)
        {
            await NetworkManager.Instance
                .MulticastMessageAndAwaitResponse<UpdateVideoClipData, OnVideoClipChangedData>(
                    new UpdateVideoClipData(), NetworkExtensions.DisplayWallControlPort, GetInstanceID(),
                    0);
            Debug.Log("Received that trial was completed");
            //todo do we even want to do anything in here? 
            //todo try awaitable thing in here and make sure that it delays the response 
            //todo try to adjust values on TrialCompletedData to make sure that those actually make their way across 
        }
        //todo register persistent callback for trial end data? this way we can send new data without interrupting the current study process?
        
        
    }

    public class StudyEndData : GenericNoValueData
    {
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
    public class StudyConditionVideoInfoData : BaseRespondingData<OnVideoClipChangedData>
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

        public override string Serialize()
        {
            return base.Serialize() + $"{nameof(studyCondition)}: {studyCondition.ToString()}";
        }
        
    }

    internal class MarkerPointEnumerator : IEnumerator<int>
    {

        private readonly int _markerCount;
        private int _currentMarker;

        internal MarkerPointEnumerator(int markerCount)
        {
            _markerCount = markerCount;
            _currentMarker = -1;
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

        public void MovePrevious()
        {
            _currentMarker = math.max(_currentMarker--, -1);
        }

        public void SetMarker(int nextMarkerPosition)
        {
            _currentMarker = nextMarkerPosition - 1;
        }
        
        public int Current => _currentMarker;

        object IEnumerator.Current => Current;
    }
    

}