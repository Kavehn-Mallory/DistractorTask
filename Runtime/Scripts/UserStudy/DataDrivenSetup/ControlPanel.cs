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
        private int startingCondition;

        private MarkerPointEnumerator _markerPointEnumerator;

        private StudyConditionsEnumerator _enumerator;
        public Action<string> OnStudyPhaseStart = delegate { };
        public Action<string> OnStudyPhaseEnd = delegate { };
        public Action OnStudyCompleted = delegate { };
        public Action<LogCategory, string> OnStudyLog = delegate { };
        
        /// <summary>
        /// Triggers on each iteration. String represents the name of the study stage, first int is the current index, second int is the iteration count
        /// </summary>
        public Action<string, int, int> OnNextIteration = delegate { };
        private StudyEnumerator _studyEnumerator;

        private const string MarkerPointPhaseName = "Marker Point Creation Phase";
        private const string StudyPhaseName = "Study Phase";

        private void Awake()
        {
            _studyEnumerator = new StudyEnumerator(studies);

        }




        #region MarkerPointRegion

        public async void StartMarkerPointCreation()
        {
            OnStudyPhaseStart.Invoke(MarkerPointPhaseName);
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
                OnNextIteration.Invoke("Marker Point", markerPointIndex, markerPointCount);
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
            OnStudyLog.Invoke(LogCategory.UserStudy, "Regenerating Marker Points");
            _markerPointEnumerator.Reset();
        }

        public void RepeatPreviousPoint()
        {
            if (_markerPointEnumerator == null)
            {
                Debug.LogError("Marker Point Setup hasn't been started yet.");
                return;
            }
            OnStudyLog.Invoke(LogCategory.UserStudy, $"Repeating Marker Point {_markerPointEnumerator.Current}");
            _markerPointEnumerator.MovePrevious();
            
        }
     
#endregion
        

        //todo look if we have to cancel tasks somehow


        public async void StartStudy()
        {

            if (!_studyEnumerator.MoveNext())
            {
                OnStudyCompleted.Invoke();
                return;
            }
            OnStudyPhaseStart.Invoke($"{StudyPhaseName}: {_studyEnumerator.CurrentStudyIndex} Starting Condition: {TransformCurrentConditionToLetter(startingCondition)}");
            _enumerator = new StudyConditionsEnumerator(_studyEnumerator.Current, startingCondition);

            var unregisterCallback = NetworkManager.Instance.RegisterPersistentMulticastResponse<TrialCompletedData, TrialCompletedResponseData>(
                OnTrialCompleted, NetworkExtensions.DefaultPort, GetInstanceID());
            
            while (_enumerator.MoveNext())
            {
                var studyCondition = _enumerator.Current;
                OnNextIteration.Invoke("Study Condition", _enumerator.CurrentPermutationIndex, _enumerator.PermutationCount);
                OnStudyLog.Invoke(LogCategory.UserStudy, $"Study Load Level: {studyCondition.loadLevel.ToString()}; Study Noise Level: {studyCondition.noiseLevel.ToString()}");
                Debug.Log($"Current Study Load Level: {studyCondition.loadLevel.ToString()}; Study Noise Level: {studyCondition.noiseLevel.ToString()}");
                Debug.Log($"Awaiting response with sender id {GetInstanceID()} and message id {_enumerator.CurrentPermutationIndex}");
                await NetworkManager.Instance
                    .MulticastMessageAndAwaitResponse<StudyConditionVideoInfoData, OnVideoClipChangedData>(
                        new StudyConditionVideoInfoData
                        {
                            studyCondition = studyCondition
                        }, NetworkExtensions.DisplayWallControlPort, GetInstanceID(),
                        _enumerator.CurrentPermutationIndex);
                
                await NetworkManager.Instance
                    .MulticastMessageAndAwaitResponse<ConditionData, OnConditionCompleted>(
                        new ConditionData
                        {
                            studyCondition = studyCondition
                        }, NetworkExtensions.DefaultPort, GetInstanceID(),
                        _enumerator.CurrentPermutationIndex);
                
            }
            Debug.Log("Study Phase Ended");
            OnStudyPhaseEnd.Invoke(StudyPhaseName);
            unregisterCallback.Invoke();
        }

        private static char TransformCurrentConditionToLetter(int currentCondition)
        {
            //assumes that the conditions start with 0 instead of 1
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