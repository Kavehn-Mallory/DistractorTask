using System;
using System.Threading;
using System.Threading.Tasks;
using DistractorTask.Transport;
using DistractorTask.UserStudy.AudioTask;
using DistractorTask.UserStudy.Core;
using DistractorTask.UserStudy.DataDrivenSetup;
using DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents;
using TMPro;
using UnityEngine;
using TrialCompletedData = DistractorTask.UserStudy.DataDrivenSetup.TrialCompletedData;

namespace DistractorTask.UserStudy.DistractorSelectionStage
{
    public class DistractorSelectionComponent : MonoBehaviour
    {

        public TextMeshProUGUI debugText; 
        public DistractorAnchorPointAsset distractorAnchorPointAsset;
        public DistractorTaskComponent distractorTaskComponent;
        public AudioTaskComponent audioTaskComponent;
        
        private Action _unregisterStudyStartData;
        private ConditionEnumerator _conditionEnumerator;

        private TaskCompletionSource<int> _inputTask;

        private CancellationTokenSource _cancellationTokenForStudy;
        
        private bool _acceptingInput;

        public Action<DistractorTaskComponent.DistractorSelectionResult> OnDistractorSelection = delegate { };
        

        private void OnEnable()
        {
            NetworkManager.Instance.RegisterCallback<ConditionData>(StartStudyCondition);
            
            InputHandler.InputHandler.Instance.OnSelectionButtonPressed += OnReceiveInput;
            
            

        }

        private void OnDisable()
        {
            NetworkManager.Instance.UnregisterCallback<ConditionData>(StartStudyCondition);
            InputHandler.InputHandler.Instance.OnSelectionButtonPressed -= OnReceiveInput;
        }
        
        
        private void OnReceiveInput()
        {
            if (!_acceptingInput)
            {
                return;
            }

            var result = distractorTaskComponent.CheckInput();
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
            _conditionEnumerator?.Dispose();
            distractorTaskComponent.EnableCanvas();
            //todo implement this 
            _cancellationTokenForStudy?.Cancel();
            _inputTask?.TrySetCanceled();
            _inputTask = new TaskCompletionSource<int>();
            
            //start new study 
            _conditionEnumerator = new ConditionEnumerator(condition.studyCondition);

            var loadLevel = condition.studyCondition.loadLevel == LoadLevel.Low ? 0 : 1;

            if (condition.studyCondition.hasAudioTask)
            {
                audioTaskComponent.BeginAudioTask();
            }
            
            debugText.text = "Starting study condition";
            
            while (_conditionEnumerator.MoveNext())
            {
                //should never be null, but the squiggly lines annoyed me and better be safe than sorry 
                var repetitionEnumerator = _conditionEnumerator.Current ?? new TrialsEnumerator(1);
                var placementPosition =
                    distractorAnchorPointAsset.GetPosition(_conditionEnumerator.CurrentTrialIndex %
                                                           distractorAnchorPointAsset.Length);
                distractorTaskComponent.RepositionCanvas(placementPosition.position);
                _acceptingInput = true;
                while (repetitionEnumerator.MoveNext())
                {
                    debugText.text = "Setting study visible";
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
                debugText.text = "Sending trial end data";
                await NetworkManager.Instance
                    .MulticastMessageAndAwaitResponse<TrialCompletedData, TrialCompletedResponseData>(
                        new TrialCompletedData(), NetworkExtensions.DefaultPort, GetInstanceID(),
                        _conditionEnumerator.CurrentTrialIndex);
                
            }
            
            if (condition.studyCondition.hasAudioTask)
            {
                audioTaskComponent.EndAudioTask();
            }

            NetworkManager.Instance.MulticastMessage(new OnConditionCompleted(), NetworkExtensions.DefaultPort,
                GetInstanceID());


            _conditionEnumerator = null;
            _inputTask = null;
            _acceptingInput = false;
            distractorTaskComponent.DisableCanvas();

        }
        
        
        
    }
}