using System;
using System.IO;
using DistractorTask.Core;
using DistractorTask.Transport;
using DistractorTask.UserStudy.Core;
using DistractorTask.UserStudy.DataDrivenSetup;
using MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.Assertions;
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
        void Start()
        {
            Assert.IsNotNull(videoPlayer, "The video player was not set.");
            //NetworkManager.Instance.RegisterCallback<VideoClipChangeData>(SwitchVideoClip, NetworkExtensions.DisplayWallControlPort);
            PreprocessFilePaths();

            videoPlayer.source = VideoSource.Url;
            videoPlayer.isLooping = true;
        }

        private void PreprocessFilePaths()
        {
            for (var i = 0; i < videoClipGroups.Length; i++)
            {
                var videoClipGroup = videoClipGroups[i];
                var path = Application.streamingAssetsPath + "/" + videoClipGroup.relativePath;
                if(!Directory.Exists(path))
                {
                    Debug.LogWarning($"Path {path} does not exist. Video clip group {i} will be ignored.");
                    continue;
                }
                videoClipGroup.videoClips = Directory.GetFiles(path);
                videoClipGroups[i] = videoClipGroup;
            }
        }

        private void OnEnable()
        {
            _unregisterVideoClipChangeEvent?.Invoke();
            _unregisterVideoClipResetEvent?.Invoke();
            _unregisterVideoClipChangeEvent = NetworkManager.Instance
                .RegisterPersistentMulticastResponse<StudyConditionData, OnVideoClipChangedData>(
                    SwitchVideoClip, NetworkExtensions.DisplayWallControlPort, GetInstanceID());
            
            _unregisterVideoClipResetEvent = NetworkManager.Instance
                .RegisterPersistentMulticastResponse<UpdateVideoClipData, OnVideoClipChangedData>(
                    ResetVideoClip, NetworkExtensions.DisplayWallControlPort, GetInstanceID());
            
        }

        private void OnDisable()
        {
            _unregisterVideoClipChangeEvent?.Invoke();
            _unregisterVideoClipResetEvent?.Invoke();
            _unregisterVideoClipChangeEvent = null;
            _unregisterVideoClipResetEvent = null;
        }

        private void ResetVideoClip(UpdateVideoClipData videoClipData, int instanceId)
        {
            //todo not sure if we do anything here 
        }

        private void SwitchVideoClip(StudyConditionData studyCondition, int instanceId)
        {
            if (videoClipGroups == null || videoClipGroups.Length == 0)
            {
                Debug.LogError("No video clips specified", this);
                return;
            }
            var noiseLevel = studyCondition.studyCondition.noiseLevel;
            

            foreach (var videoClipGroup in videoClipGroups)
            {
                if ((videoClipGroup.noiseLevel & noiseLevel) == noiseLevel)
                {
                    //choose clip 
                    videoPlayer.url = videoClipGroup.videoClips.RandomElement();
                    videoPlayer.Play();
                    return;
                }
            }
            
            Debug.LogWarning("Did not find a fitting clip. Playing first group as fallback", this);
            videoPlayer.url = videoClipGroups[0].videoClips.RandomElement();
            videoPlayer.Play();
            
        }
        

        [Serializable]
        public struct VideoClipGroup
        {
            
            public string relativePath;
            [EnumFlags]
            public NoiseLevel noiseLevel;
            
            [HideInInspector]
            public string[] videoClips;
        }
        

    }
}
