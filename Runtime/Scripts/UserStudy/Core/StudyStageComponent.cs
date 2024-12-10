using System;
using DistractorTask.Transport;
using UnityEngine;

namespace DistractorTask.UserStudy.Core
{
    public abstract class StudyStageComponent : MonoBehaviour
    {

        public INetworkManager Manager;
        public event Action OnStudyStart = delegate { };

        public event Action OnStudyEnd = delegate { };

        protected void TriggerStudyStartEvent() => OnStudyStart.Invoke();
        
        protected void TriggerStudyEndEvent() => OnStudyEnd.Invoke();
        
    }
}