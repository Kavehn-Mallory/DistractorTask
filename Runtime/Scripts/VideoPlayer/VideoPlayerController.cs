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


        public Action OnVideoClipReset = delegate { };
        public readonly Action<string, string> OnVideoClipSelected = delegate { };
        
        
        
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

        private static async Task<AudioClip> LoadClip(string path, AudioType audioType)
        {
            AudioClip clip = null;
            using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
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
                    videoClipGroup.videoClips = Array.Empty<string>();
                    videoClipGroup.audioClips = Array.Empty<AudioClip>();
                    videoClipGroups[i] = videoClipGroup;
                    Debug.LogWarning($"Path {path} does not exist. Video clip group {i} will be empty.");
                    continue;
                }

                var files = Directory.GetFiles(path);
                var videoClips = new List<string>();
                var audioClips = new List<AudioClip>();

                foreach (var file in files)
                {
                    //todo maybe implement a list of acceptable file types 
                    if (file.EndsWith(".meta", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    if (file.EndsWith(".wav", StringComparison.CurrentCultureIgnoreCase))
                    {
                        audioClips.Add(await LoadClip(file, AudioType.WAV));
                        continue;
                    }

                    if (file.EndsWith(".mp3", StringComparison.CurrentCultureIgnoreCase))
                    {
                        audioClips.Add(await LoadClip(file, AudioType.MPEG));
                        continue;
                    }
                    videoClips.Add(file);
                }

                videoClipGroup.videoClips = videoClips.ToArray();
                videoClipGroup.audioClips = audioClips.ToArray();
                videoClipGroups[i] = videoClipGroup;
                
                Debug.Log($"Video Clip Count: {videoClipGroup.videoClips.Length}");
                Debug.Log($"Audio Clip Count: {videoClipGroup.audioClips.Length}");
            }
            
            
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
            OnVideoClipReset.Invoke();
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
                    var videoLink = "";
                    AudioClip audioClip = null;
                    if (videoClipGroup.videoClips.Length != 0)
                    {
                        videoClipGroup.videoClips.RandomElement();
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
            var element = videoClipGroups.First(v => v.audioClips.Length > 0);
            SwitchVideoClip(videoClipGroups.First(v => v.videoClips.Length > 0).videoClips.RandomElement(), element.audioClips.RandomElement(), element.volume);
            
        }

        private void SwitchVideoClip(string videoUrl, AudioClip audioClip, float volume)
        {
            var audioClipName = audioClip ? audioClip.name : "";
            LoggingComponent.Log(LogData.CreateVideoPlayerChangeLogData(videoUrl, audioClipName));
            OnVideoClipSelected.Invoke(videoUrl, audioClipName);
            videoPlayer.url = videoUrl;
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.Play();
            videoPlayer.Play();
        }

        [Serializable]
        public struct VideoClipGroup
        {
            
            public string relativePath;
            public NoiseLevel noiseLevel;
            [Range(0, 1)]
            public float volume;
            
            [HideInInspector]
            public string[] videoClips;
            
            [HideInInspector]
            public AudioClip[] audioClips;
        }
        

    }
}
