using System;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
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
        private int debugIndex;

        [SerializeField] private VideoClip[] videoClips = Array.Empty<VideoClip>();
        
        // Start is called before the first frame update
        void Start()
        {
            Assert.IsNotNull(videoPlayer, "The video player was not set.");
            NetworkManager.Instance.RegisterCallback<VideoClipChangeData>(SwitchVideoClip, NetworkExtensions.DefaultPort);
        }

        private void SwitchVideoClip(VideoClipChangeData changeData, int instanceId)
        {
            if (changeData.videoClipIndex < 0 && changeData.videoClipIndex >= videoClips.Length)
            {
                return;
            }

            videoPlayer.clip = videoClips[changeData.videoClipIndex];
            videoPlayer.Play();
        }


        [ContextMenu("Play video clip")]
        public void DebugTest()
        {
            SwitchVideoClip(new VideoClipChangeData
            {
                videoClipIndex = debugIndex
            }, 0);
        }

    }
}
