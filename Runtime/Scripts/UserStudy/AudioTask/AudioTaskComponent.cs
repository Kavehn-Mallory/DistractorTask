using System;
using System.Threading;
using System.Threading.Tasks;
using DistractorTask.Logging;
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
        
        private Transform _mainCameraTransform;

        private CancellationTokenSource _cancellationTokenSource;

        private bool _taskIsActive;
        
        private long _timestampAudioTaskStart = -1;
        private long _timestampAudioTaskEnd = -1;
        


        private void Start()
        {
            audioClipTarget.clip = audioClip;
            _mainCameraTransform = Camera.main.transform;
            InputHandler.InputHandler.Instance.OnTriggerButtonPressed += OnButtonPressReceived;
        }

        private void OnDisable()
        {
            _taskIsActive = false;
            _cancellationTokenSource?.Cancel();
        }


        [ContextMenu("Start Audio Task")]
        public async void BeginAudioTask()
        {
            Debug.Log("Audio task has started");
            audioClipTarget.enabled = true;
            _taskIsActive = true;
            _cancellationTokenSource = new CancellationTokenSource();
            

            while (_taskIsActive)
            {
                var delay = (int)(Random.Range(taskFrequency.x, taskFrequency.y) * 1000);
                var delayTask = Task.Delay(delay, _cancellationTokenSource.Token);
                await delayTask;
                Debug.Log("Delay is over");
                if (delayTask.IsCanceled)
                {
                    Debug.Log("Task was canceled");
                    _taskIsActive = false;
                    return;
                }
                
                _timestampAudioTaskStart = LogData.GetCurrentTimestamp();
                _timestampAudioTaskEnd = -1;
                PlayAudioTask();
                //we wait a bit longer than the acceptable time frame to make sure that the result without button press is invalid
                var audioTask = Task.Delay((int)(maxReactionTime * 1000 + 20), _cancellationTokenSource.Token);
                await audioTask;

                if (_timestampAudioTaskEnd == -1)
                {
                    Debug.Log("Did not hit the correct timing");
                    _timestampAudioTaskEnd = LogData.GetCurrentTimestamp();
                }
                LoggingComponent.Log(LogData.CreateAudioTaskConfirmationLogData(_timestampAudioTaskStart, _timestampAudioTaskEnd));
                _timestampAudioTaskStart = -1;
                audioClipTarget.Stop();
            }
            
        }

        [ContextMenu("Simulate button press")]
        private void OnButtonPressReceived()
        {
            if (_timestampAudioTaskStart < 0 || !_taskIsActive || _timestampAudioTaskEnd >= 0)
            {
                return;
            }
            Debug.Log("Hit the correct timing");
            _timestampAudioTaskEnd = LogData.GetCurrentTimestamp();
        }

        [ContextMenu("End Audio Task")]
        public void EndAudioTask()
        {
            _taskIsActive = false;
            audioClipTarget.Stop();
            audioClipTarget.enabled = false;
            _cancellationTokenSource.Cancel();
        }
        
        private void PlayAudioTask()
        {
            audioClipTarget.enabled = true;
            var positionOffset = Random.onUnitSphere * Random.Range(distanceOfAudioSource.x, distanceOfAudioSource.y);
            Debug.Log("Pling");
            audioClipTarget.transform.SetPositionAndRotation(_mainCameraTransform.position + positionOffset, Quaternion.identity);
            audioClipTarget.time = 0;
            audioClipTarget.Play();

        }
    }
}