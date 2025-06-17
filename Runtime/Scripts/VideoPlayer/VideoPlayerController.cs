using System;
using DistractorTask.Core;
using DistractorTask.Transport;
using DistractorTask.UserStudy.Core;
using DistractorTask.UserStudy.DataDrivenSetup;
using MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Video;
using Random = UnityEngine.Random;

namespace DistractorTask.VideoPlayer
{
    public class VideoPlayerController : MonoBehaviour
    {

        [SerializeField]
        private UnityEngine.Video.VideoPlayer videoPlayer;

        [SerializeField]
        private int debugIndex;

        [SerializeField] private VideoClipGroup[] videoClipGroups = Array.Empty<VideoClipGroup>();

        private Action _unregisterVideoClipChangeEvent;

        private Action _unregisterVideoClipResetEvent;
        
        
        
        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(videoPlayer, "The video player was not set.");
            //NetworkManager.Instance.RegisterCallback<VideoClipChangeData>(SwitchVideoClip, NetworkExtensions.DisplayWallControlPort);
            
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
                    videoPlayer.clip = videoClipGroup.videoClips.RandomElement();
                    videoPlayer.Play();
                    return;
                }
            }
            
            Debug.LogWarning("Did not find a fitting clip. Playing first group as fallback", this);
            videoPlayer.clip = videoClipGroups[0].videoClips.RandomElement();
            videoPlayer.Play();
        }
        
        

        [Serializable]
        public struct VideoClipGroup
        {
            [EnumFlags]
            public NoiseLevel noiseLevel;
            public VideoClip[] videoClips;
        }

    }
}
