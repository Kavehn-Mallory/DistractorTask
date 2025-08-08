using System;
using UnityEngine;
using UnityEngine.Playables;

namespace DistractorTask.Logging.Timeline
{
    [Serializable]
    public class LogFileControlAsset : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<LogFileControlBehaviour>.Create(graph);

            var behaviour = playable.GetBehaviour();
            
            return playable;
        }
    }
}