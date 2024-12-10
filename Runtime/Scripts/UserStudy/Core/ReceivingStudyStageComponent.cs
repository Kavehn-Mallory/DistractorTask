using DistractorTask.Core;
using DistractorTask.Transport;

namespace DistractorTask.UserStudy.Core
{
    public abstract class ReceivingStudyStageComponent<TStudyEvent> : ReceivingStudyStageComponent where TStudyEvent : unmanaged, IStudyStageEvent
    {
        
        public override void RegisterStudyComponent(INetworkManager manager)
        {
            manager.RegisterCallback<TStudyEvent>(OnStudyStageEventReceived);
        }

        public override void UnregisterStudyComponent(INetworkManager manager)
        {
            manager.UnregisterCallback<TStudyEvent>(OnStudyStageEventReceived);
        }
        
        private void OnStudyStageEventReceived(TStudyEvent studyEvent)
        {
            if (studyEvent.IsStartEvent)
            {
                OnStudyStageStart(studyEvent);
                TriggerStudyStartEvent();
                return;
            }
            OnStudyStageEnd(studyEvent);
            TriggerStudyEndEvent();
        }

        protected abstract void OnStudyStageStart(TStudyEvent studyEvent);

        protected abstract void OnStudyStageEnd(TStudyEvent studyEvent);
    }

    public abstract class ReceivingStudyStageComponent : StudyStageComponent
    {
        public abstract void RegisterStudyComponent(INetworkManager manager);

        public abstract void UnregisterStudyComponent(INetworkManager manager);
    }


}