using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DistractorTask.Core;
using DistractorTask.Logging;
using DistractorTask.Transport;
using DistractorTask.UserStudy.Core;
using DistractorTask.UserStudy.DataDrivenSetup;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.Video;

namespace DistractorTask.VideoPlayer
{
    public class VideoPlayerController : MonoBehaviour
    {

        [SerializeField]
        private UnityEngine.Video.VideoPlayer videoPlayer;

        [SerializeField]
        private AudioSource audioSource;
        
        [SerializeField] private VideoClipGroup[] videoClipGroups = Array.Empty<VideoClipGroup>();

        private Action _unregisterVideoClipChangeEvent;

        private Action _unregisterVideoClipResetEvent;

        
        
        
        
        // Start is called before the first frame update
        private void Start()
        {
            Assert.IsNotNull(videoPlayer, "The video player was not set.");
            //NetworkManager.Instance.RegisterCallback<VideoClipChangeData>(SwitchVideoClip, NetworkExtensions.DisplayWallControlPort);

            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.isLooping = true;
            audioSource.loop = true;

        }
        
        private void OnEnable()
        {
            _unregisterVideoClipChangeEvent?.Invoke();
            _unregisterVideoClipResetEvent?.Invoke();
            _unregisterVideoClipChangeEvent = NetworkManager.Instance
                .RegisterPersistentMulticastResponse<StudyConditionVideoInfoData, OnVideoClipChangedData>(
                    SwitchVideoClip, NetworkExtensions.DisplayWallControlPort, GetInstanceID());
            
            _unregisterVideoClipResetEvent = NetworkManager.Instance
                .RegisterPersistentMulticastResponse<UpdateVideoClipData, OnVideoClipChangedData>(
                    ResetVideoClip, NetworkExtensions.DisplayWallControlPort, GetInstanceID());
            
            NetworkManager.Instance.RegisterCallbackAllPorts<StudyEndData>(OnStudyEndDataReceived);
        }

        private void OnStudyEndDataReceived(StudyEndData arg1, int arg2)
        {
            videoPlayer.Stop();
            audioSource.Stop();
        }

        private void OnDisable()
        {
            _unregisterVideoClipChangeEvent?.Invoke();
            _unregisterVideoClipResetEvent?.Invoke();
            _unregisterVideoClipChangeEvent = null;
            _unregisterVideoClipResetEvent = null;
            videoPlayer.Stop();
            audioSource.Stop();
        }

        private void ResetVideoClip(UpdateVideoClipData videoClipData, int instanceId)
        {
            videoPlayer.time = 0;
            audioSource.time = 0;
            videoPlayer.Play();
            audioSource.Play();
        }

        private void SwitchVideoClip(StudyConditionVideoInfoData studyConditionVideoInfo, int instanceId)
        {
            if (videoClipGroups == null || videoClipGroups.Length == 0)
            {
                Debug.LogError("No video clips specified", this);
                return;
            }
            var noiseLevel = studyConditionVideoInfo.studyCondition.noiseLevel;
            
            
            foreach (var videoClipGroup in videoClipGroups)
            {
                if ((videoClipGroup.noiseLevel & noiseLevel) == noiseLevel)
                {
                    VideoClip videoLink = null;
                    AudioClip audioClip = null;
                    if (videoClipGroup.videoClips.Length != 0)
                    {
                        videoLink = videoClipGroup.videoClips.RandomElement();
                    }

                    if (videoClipGroup.audioClips.Length != 0)
                    {
                        audioClip = videoClipGroup.audioClips.RandomElement();
                    }
                    //choose clip 
                    SwitchVideoClip(videoLink, audioClip, videoClipGroup.volume);
                    return;
                }
            }
            
            Debug.LogWarning("Did not find a fitting clip. Playing first group as fallback", this);
            SwitchVideoClip(videoClipGroups[0].videoClips.RandomElement(), videoClipGroups[0].audioClips.RandomElement(), videoClipGroups[0].volume);
            
        }

        private void SwitchVideoClip(VideoClip videoClip, AudioClip audioClip, float volume)
        {
            var audioClipName = audioClip ? audioClip.name : "No Audio Clip Found";
            var videoClipName = videoClip ? videoClip.name : "No Video Clip Found";
            LoggingComponent.Log(LogData.CreateVideoPlayerChangeLogData(videoClipName, audioClipName));
            videoPlayer.clip = videoClip;
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.Play();
            videoPlayer.Play();
        }

        [Serializable]
        public struct VideoClipGroup
        {
            
            [FormerlySerializedAs("relativePath")] public string groupName;
            public NoiseLevel noiseLevel;
            [Range(0, 1)]
            public float volume;
            
            public VideoClip[] videoClips;
            public AudioClip[] audioClips;
        }
        

    }
}
