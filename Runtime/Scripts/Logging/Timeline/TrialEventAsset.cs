using System;
using UnityEngine;
using UnityEngine.Playables;

namespace DistractorTask.Logging.Timeline
{
    [Serializable]
    public class TrialEventAsset : GenericStudyEventAsset
    {

        public int trialCount;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<TrialEventBehaviour>.Create(graph);

            var behaviour = playable.GetBehaviour();
            behaviour.TrialCount = trialCount;
            
            return playable;
        }
    }
}