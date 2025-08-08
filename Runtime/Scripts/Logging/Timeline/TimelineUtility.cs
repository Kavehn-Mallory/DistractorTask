using UnityEngine.Timeline;

namespace DistractorTask.Logging.Timeline
{
    public static class TimelineUtility
    {
        public static T CreateSubTrack<T>(TimelineAsset timeline, TrackAsset parentTrack, string name = "SubTrack") where T : TrackAsset, new()
        {
            var subTrack = timeline.CreateTrack<T>(parentTrack, name);
            return subTrack;
        }
    }
}