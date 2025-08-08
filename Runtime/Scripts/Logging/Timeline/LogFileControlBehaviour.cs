using System;
using UnityEngine.Playables;

namespace DistractorTask.Logging.Timeline
{
    [Serializable]
    public class LogFileControlBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
        }
    }
}