using System;
using UnityEngine;
using UnityEngine.Playables;

namespace DistractorTask.Logging.Timeline
{
    [Serializable]
    public class StudyEventAsset: GenericStudyEventAsset
    {

        public string studyName;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<StudyEventBehaviour>.Create(graph);

            var behaviour = playable.GetBehaviour();
            behaviour.testName = studyName;
            
            return playable;
        }
    }
}