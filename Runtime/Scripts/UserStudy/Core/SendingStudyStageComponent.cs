using DistractorTask.Core;
using DistractorTask.Transport;

namespace DistractorTask.UserStudy.Core
{
    public class SendingStudyStageComponent<TStudyEvent> : SendingStudyStageComponent where TStudyEvent : unmanaged, IStudyStageEvent
    {


        public override void StartStudy(INetworkManager manager)
        {
            manager.BroadcastMessage(new TStudyEvent
            {
                IsStartEvent = true
            }, GetInstanceID());
            TriggerStudyStartEvent();
        }

        public async void StartStudy<TAwaitableStudyEvent, TResponse>(INetworkManager manager, TAwaitableStudyEvent studyEventData)
            where TAwaitableStudyEvent : IStudyStageEvent, IRespondingSerializer<TResponse>, new() where TResponse : IResponseIdentifier, new()
        {
            await manager.BroadcastMessageAndAwaitResponse<TAwaitableStudyEvent, TResponse>(studyEventData, GetInstanceID(),
                studyEventData.MessageId);
        }

        public override void EndStudy(INetworkManager manager)
        {
            manager.BroadcastMessage(new TStudyEvent
            {
                IsStartEvent = false
            }, GetInstanceID());
            TriggerStudyEndEvent();
        }
    }
    
    public abstract class SendingStudyStageComponent : StudyStageComponent
    {
        public abstract void StartStudy(INetworkManager manager);

        public abstract void EndStudy(INetworkManager manager);
    }
}