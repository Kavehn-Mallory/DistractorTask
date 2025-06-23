using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DistractorTask.Core;
using DistractorTask.Transport;
using DistractorTask.UserStudy.Core;
using DistractorTask.UserStudy.DataDrivenSetup;
using MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
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
            PreprocessFilePaths();

            videoPlayer.source = VideoSource.Url;
            videoPlayer.isLooping = true;
            audioSource.loop = true;

        }

        private static async Task<AudioClip> LoadClip(string path)
        {
            AudioClip clip = null;
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
            {
                uwr.SendWebRequest();

                // wrap tasks in try/catch, otherwise it'll fail silently
                try
                {
                    while (!uwr.isDone) await Task.Delay(5);

                    if (uwr.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.Log($"{uwr.error}");
                    }
                    else
                    {
                        clip = DownloadHandlerAudioClip.GetContent(uwr);
                    }
                }
                catch (Exception err)
                {
                    Debug.Log($"{err.Message}, {err.StackTrace}");
                }
            }

            return clip;
        }
        

        private async void PreprocessFilePaths()
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

                var files = Directory.GetFiles(path);
                var videoClips = new List<string>();
                var audioClips = new List<AudioClip>();

                for (int j = 0; j < files.Length; j++)
                {
                    if (files[i].EndsWith(".wav", StringComparison.CurrentCultureIgnoreCase))
                    {
                        audioClips.Add(await LoadClip(files[i]));
                        continue;
                    }
                    videoClips.Add(files[i]);
                }

                videoClipGroup.videoClips = videoClips.ToArray();
                videoClipGroup.audioClips = audioClips.ToArray();
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
            videoPlayer.time = 0;
            audioSource.time = 0;
            videoPlayer.Play();
            audioSource.Play();
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
                    SwitchVideoClip(videoClipGroup.videoClips.RandomElement(), videoClipGroup.audioClips.RandomElement());
                    return;
                }
            }
            
            Debug.LogWarning("Did not find a fitting clip. Playing first group as fallback", this);
            SwitchVideoClip(videoClipGroups[0].videoClips.RandomElement(), videoClipGroups[0].audioClips.RandomElement());
            
        }

        private void SwitchVideoClip(string videoUrl, AudioClip audioClip)
        {
            videoPlayer.url = videoUrl;
            audioSource.clip = audioClip;
            audioSource.Play();
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
            
            [HideInInspector]
            public AudioClip[] audioClips;
        }
        

    }
}
