using System;
using System.Threading;
using System.Threading.Tasks;
using DistractorTask.Debugging;
using DistractorTask.Logging;
using DistractorTask.Transport;
using DistractorTask.UserStudy.DataDrivenSetup;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DistractorTask.UserStudy.AudioTask
{
    public class AudioTaskComponent : MonoBehaviour
    {
        [SerializeField]
        private AudioClip audioClip;

        [SerializeField] private AudioSource audioClipTarget;

        [SerializeField] private Vector2 distanceOfAudioSource = new Vector2(1f, 1f);

        [SerializeField, Tooltip("Min and max delay between repetitions in seconds")] private Vector2 taskFrequency = new Vector2(15, 25);

        [SerializeField] private float maxReactionTime = 2f;

        [SerializeField]
        private DebuggingScriptableObject debugObject;

        private Transform _mainCameraTransform;

        private CancellationTokenSource _cancellationTokenSource;

        private bool _taskIsActive;
        
        private long _timestampLastAudioTask = -1;
        


        private void Start()
        {
            audioClipTarget.clip = audioClip;
            _mainCameraTransform = Camera.main.transform;
            InputHandler.InputHandler.Instance.OnTriggerButtonPressed += OnButtonPressReceived;
        }
        


        public async void BeginAudioTask()
        {
            audioClipTarget.enabled = true;
            _taskIsActive = true;
            _cancellationTokenSource = new CancellationTokenSource();
            

            while (_taskIsActive)
            {
                var delay = (int)(Random.Range(taskFrequency.x, taskFrequency.y) * 1000);
                var delayTask = Task.Delay(delay, _cancellationTokenSource.Token);
                await delayTask;
                if (delayTask.IsCanceled)
                {
                    _taskIsActive = false;
                    return;
                }

                _taskIsActive = true;
                _timestampLastAudioTask = LogData.GetCurrentTimestamp();
                PlayAudioTask();
                //we wait a bit longer than the acceptable time frame to make sure that the result without button press is invalid
                var audioTask = Task.Delay((int)(maxReactionTime * 1000 + 20), _cancellationTokenSource.Token);
                await audioTask;
                if (_taskIsActive)
                {
                    //we did not get it
                    LoggingComponent.Log(LogData.CreateAudioTaskConfirmationLogData(_timestampLastAudioTask, LogData.GetCurrentTimestamp()));
                }
                _taskIsActive = false;
            }
            
        }

        private void OnButtonPressReceived()
        {
            if (_timestampLastAudioTask < 0 || !_taskIsActive)
            {
                return;
            }

            var dif = (new TimeSpan(LogData.GetCurrentTimestamp()) - new TimeSpan(_timestampLastAudioTask)).TotalSeconds;
            LoggingComponent.Log(LogData.CreateAudioTaskConfirmationLogData(_timestampLastAudioTask, LogData.GetCurrentTimestamp()));
            
            if (dif <= maxReactionTime)
            {
                //success 
                debugObject.AddDebugText("Audio Task Success");
            }
            else
            {
                debugObject.AddDebugText("Audio Task Failure");
                //too late
            }

            _taskIsActive = false;
        }

        public void EndAudioTask()
        {
            _cancellationTokenSource.Cancel();
            audioClipTarget.Stop();
            audioClipTarget.enabled = false;
            _taskIsActive = false;
        }
        
        private void PlayAudioTask()
        {
            audioClipTarget.enabled = true;
            var positionOffset = Random.onUnitSphere * Random.Range(distanceOfAudioSource.x, distanceOfAudioSource.y);
            
            audioClipTarget.transform.SetPositionAndRotation(_mainCameraTransform.position + positionOffset, Quaternion.identity);
            audioClipTarget.Play();

        }
    }
}