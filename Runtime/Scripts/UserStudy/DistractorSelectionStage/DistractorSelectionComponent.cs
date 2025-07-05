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
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using TrialCompletedData = DistractorTask.UserStudy.DataDrivenSetup.TrialCompletedData;

namespace DistractorTask.UserStudy.DistractorSelectionStage
{
    public class DistractorSelectionComponent : MonoBehaviour
    {
        
        public DistractorAnchorPointAsset distractorAnchorPointAsset;
        public DistractorTaskComponent distractorTaskComponent;
        public AudioTaskComponent audioTaskComponent;
        
        private TrialEnumerator _trialEnumerator;

        private TaskCompletionSource<int> _inputTask;
        
        private bool _acceptingInput;
        

        public TMP_Text debugText;

        private void OnEnable()
        {
            //NetworkManager.Instance.RegisterCallback<ConditionData>(StartStudyCondition);
            NetworkManager.Instance.RegisterCallback<ConditionData>(StartStudyCondition, NetworkExtensions.DefaultPort);
            
            InputHandler.InputHandler.Instance.OnSelectionButtonPressed += OnReceiveInput;
            
            

        }

        private void OnDisable()
        {
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
            _inputTask?.SetResult(1);
        }

        private async void StartStudyCondition(ConditionData condition, int instanceId)
        {

            
            //probably not needed
            await PerformStudyCondition(condition);

            NetworkManager.Instance.MulticastRespond(condition, NetworkExtensions.DefaultPort, GetInstanceID());
            
        }

        private async Task PerformStudyCondition(ConditionData condition)
        {
            //Todo get the points from the anchor point thing, I think 
            _trialEnumerator?.Dispose();
            distractorTaskComponent.EnableCanvas();
            //todo implement this 
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
            
            debugText.text = ("Starting study condition");
            
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
                    debugText.text = ("Setting study visible");
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
                debugText.text = ("Sending trial end data");
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