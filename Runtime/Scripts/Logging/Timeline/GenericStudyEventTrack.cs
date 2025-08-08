using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DistractorTask.Logging.Timeline
{
    [TrackClipType(typeof(GenericStudyEventAsset))]
    public class GenericStudyEventTrack : TrackAsset, ILayerable
    {

        /*public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var playable = ScriptPlayable<MyTrackMixerBehaviour>.Create(graph, inputCount);

            foreach (var subTrack in GetChildTracks())
            {
                // Create and connect subtrack mixers here if needed
            }

            return playable;
        }*/

        [ContextMenu("Test")]
        public void Test()
        {
            var t = TimelineUtility.CreateSubTrack<GenericStudyEventTrack>(this.timelineAsset, this);
            
        }


        public Playable CreateLayerMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var playable = ScriptPlayable<CustomLayerMixer>.Create(graph);
            
            
            return playable;
        }
    }

    public class CustomLayerMixer : PlayableBehaviour
    {
        
    }
}