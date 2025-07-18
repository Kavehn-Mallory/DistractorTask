﻿using System.Threading;
using System.Threading.Tasks;
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

        [SerializeField] private float maxReactionTime = 5f;

        private Transform _mainCameraTransform;

        private CancellationTokenSource _cancellationTokenSource;

        private bool _taskIsActive;

        private float _timestampLastAudioTask = -1;


        private void Start()
        {
            audioClipTarget.clip = audioClip;
            _mainCameraTransform = Camera.main.transform;
            
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
                _timestampLastAudioTask = Time.time;
                PlayAudioTask();
                var audioTask = Task.Delay((int)(maxReactionTime * 1000), _cancellationTokenSource.Token);
                await audioTask;
                _taskIsActive = false;
            }
            
        }

        private void OnButtonPressReceived()
        {
            if (_timestampLastAudioTask < 0 || !_taskIsActive)
            {
                return;
            }

            if (Time.time - _timestampLastAudioTask <= maxReactionTime)
            {
                //success 
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
            var positionOffset = Random.onUnitSphere * Random.Range(distanceOfAudioSource.x, distanceOfAudioSource.y);
            
            audioClipTarget.transform.SetPositionAndRotation(_mainCameraTransform.position + positionOffset, Quaternion.identity);
            audioClipTarget.Play();

        }
    }
}