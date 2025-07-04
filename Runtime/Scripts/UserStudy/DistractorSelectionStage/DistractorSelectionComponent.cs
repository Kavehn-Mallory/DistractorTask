using System;
using System.Threading;
using System.Threading.Tasks;
using DistractorTask.Debugging;
using DistractorTask.Logging;
using DistractorTask.Transport;
using DistractorTask.UserStudy.AudioTask;
using DistractorTask.UserStudy.Core;
using DistractorTask.UserStudy.DataDrivenSetup;
using DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents;
using UnityEngine;
using TrialCompletedData = DistractorTask.UserStudy.DataDrivenSetup.TrialCompletedData;

namespace DistractorTask.UserStudy.DistractorSelectionStage
{
    public class DistractorSelectionComponent : MonoBehaviour
    {

        public DebuggingScriptableObject debugText; 
        public DistractorAnchorPointAsset distractorAnchorPointAsset;
        public DistractorTaskComponent distractorTaskComponent;
        public AudioTaskComponent audioTaskComponent;
        
        private TrialEnumerator _trialEnumerator;

        private TaskCompletionSource<int> _inputTask;

        private CancellationTokenSource _cancellationTokenForStudy;
        
        private bool _acceptingInput;

        public Action<DistractorTaskComponent.DistractorSelectionResult> OnDistractorSelection = delegate { };

        private Action _unregisterPersistentConditionDataResponse;

        private void OnEnable()
        {
            //NetworkManager.Instance.RegisterCallback<ConditionData>(StartStudyCondition);
            _unregisterPersistentConditionDataResponse = NetworkManager.Instance.RegisterPersistentMulticastResponse<ConditionData, OnConditionCompleted>(
                StartStudyCondition, NetworkExtensions.DefaultPort, GetInstanceID());
            
            InputHandler.InputHandler.Instance.OnSelectionButtonPressed += OnReceiveInput;
            
            

        }

        private void OnDisable()
        {
            _unregisterPersistentConditionDataResponse.Invoke();
            InputHandler.InputHandler.Instance.OnSelectionButtonPressed -= OnReceiveInput;
        }
        
        
        private void OnReceiveInput()
        {
            if (!_acceptingInput)
            {
                return;
            }
            var currentRepetition = _trialEnumerator.Current?.CurrentRepetition ?? -1;
            var result = distractorTaskComponent.CheckInput();
            
            LoggingComponent.Log(LogData.CreateTrialConfirmationLogData(result.targetDistractor, result.selectedDistractor,
                result.symbolOrder, result.startTime, LogData.GetCurrentTimestamp(), _trialEnumerator.CurrentTrialIndex,
                currentRepetition, _trialEnumerator.CurrentTrialIndex %
                                   distractorAnchorPointAsset.Length));
            OnDistractorSelection.Invoke(result);
            _inputTask?.SetResult(1);
        }

        private async void StartStudyCondition(ConditionData condition, int instanceId)
        {
            //todo abort any active study. This means that every resource we use, needs to be disposable (mainly the awaitable stuff)
            //instead of just calling this method again, maybe have a task holder object, that has references to the underlying code and can allow us to just cancel the entire thing
            //maybe implement some form of cancellation token for the await response methods?
            //probably can use c# cancellation tokens. just allow for them to be passed into any awaitable method 
            
            
            //Todo get the points from the anchor point thing, I think 
            _trialEnumerator?.Dispose();
            distractorTaskComponent.EnableCanvas();
            //todo implement this 
            _cancellationTokenForStudy?.Cancel();
            _inputTask?.TrySetCanceled();
            _inputTask = new TaskCompletionSource<int>();
            
            //start new study 
            _trialEnumerator = new TrialEnumerator(condition.studyCondition);
            
            LoggingComponent.Log(LogData.CreateTrialBeginLogData(condition.studyCondition.noiseLevel, condition.studyCondition.loadLevel, condition.studyCondition.trialCount, condition.studyCondition.repetitionsPerTrial, condition.studyCondition.hasAudioTask ? 2 : -1));

            var loadLevel = condition.studyCondition.loadLevel == LoadLevel.Low ? 0 : 1;

            if (condition.studyCondition.hasAudioTask)
            {
                audioTaskComponent.BeginAudioTask();
            }
            
            debugText.AddDebugText("Starting study condition");
            
            while (_trialEnumerator.MoveNext())
            {
                
                //should never be null, but the squiggly lines annoyed me and better be safe than sorry 
                var repetitionEnumerator = _trialEnumerator.Current ?? new TrialRepetitionEnumerator(1);
                var anchor = distractorAnchorPointAsset.GetAnchorPoint(_trialEnumerator.CurrentTrialIndex %
                                                                    distractorAnchorPointAsset.Length);

                var placementPosition = anchor.GetPosition();

                if (condition.studyCondition.isInsideWall)
                {
                    placementPosition = anchor.GetPositionInsideWall();
                }
                
                distractorTaskComponent.RepositionCanvas(placementPosition);
                _acceptingInput = true;
                while (repetitionEnumerator.MoveNext())
                {
                    debugText.AddDebugText("Setting study visible");
                    _inputTask = new TaskCompletionSource<int>();
                    distractorTaskComponent.StartTrial(loadLevel);
                    await _inputTask.Task;
                    if (_inputTask.Task.IsCanceled)
                    {
                        _acceptingInput = true;
                        //todo check that this works, but I think this way we just get out of here 
                        return;
                    }
                    
                }
                _acceptingInput = false;
                debugText.AddDebugText("Sending trial end data");
                await NetworkManager.Instance
                    .MulticastMessageAndAwaitResponse<TrialCompletedData, TrialCompletedResponseData>(
                        new TrialCompletedData(), NetworkExtensions.DefaultPort, GetInstanceID(),
                        _trialEnumerator.CurrentTrialIndex);
                
            }
            
            if (condition.studyCondition.hasAudioTask)
            {
                audioTaskComponent.EndAudioTask();
            }

            LoggingComponent.Log(LogData.CreateTrialEndLogData());


            _trialEnumerator = null;
            _inputTask = null;
            _acceptingInput = false;
            distractorTaskComponent.DisableCanvas();

        }
        
        
        
    }
}