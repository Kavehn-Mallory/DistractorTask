using System;
using UnityEngine.Timeline;

namespace DistractorTask.Logging.Timeline
{
    [TrackClipType(typeof(LogFileControlAsset))]
    public class LogFileControlTrack : TrackAsset
    {
        private void OnEnable()
        {
            this.m_Clips.Clear();
            var test = this.CreateClip<LogFileControlAsset>();

            test.start = 5f;
            test.duration = 0.5;
            test.displayName = "Automatically created clip";
        }
        
    }
}