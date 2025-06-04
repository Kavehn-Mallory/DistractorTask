using System;
using System.Threading.Tasks;
using DistractorTask.Core;
using DistractorTask.Transport;
using UnityEngine;

namespace DistractorTask.UserStudy.Core.Async
{
    public abstract class AsyncStudyStageComponent<TStudyData, TStudyResponseData> : StudyStageComponent where TStudyData : IRespondingSerializer<TStudyResponseData>, IStudyStageEvent, new() where TStudyResponseData : IResponseIdentifier, new()
    {


        [SerializeField]
        private StudyPart studyPart;

        [SerializeField]
        private int messageId;
        
        //todo this does not really work. One component should be able to register all their needs first I guess?


        public void InitializeStudy()
        {
            if(studyPart == StudyPart.Receiver)
                Manager.AwaitBroadcastMessageAndRespond<TStudyData, TStudyResponseData>(null, GetInstanceID());
        }

        public async void PerformStudy(INetworkManager manager)
        {
            switch (studyPart)
            {
                case StudyPart.Sender:
                    await PerformSendStart();
                    await PerformSendingStudy();
                    break;
                case StudyPart.Receiver:
                    await PerformReceivingStudy();
                    break;
            }
            
        }
        

        private async Task PerformSendStart()
        {
            TriggerStudyStartEvent();
            await Manager.BroadcastMessageAndAwaitResponse<TStudyData, TStudyResponseData>(new TStudyData
                {
                    IsStartEvent = true
                },
                GetInstanceID(), messageId);

        }

        private async Task PerformSendEnd()
        {
            await Manager.BroadcastMessageAndAwaitResponse<TStudyData, TStudyResponseData>(new TStudyData
                {
                    IsStartEvent = false
                },
                GetInstanceID(), messageId);
            
            TriggerStudyEndEvent();
        }
        

        public abstract Task PerformSendingStudy();
        public abstract Task PerformReceivingStudy();
    }

    [Serializable]
    public enum StudyPart
    {
        Sender,
        Receiver
    }
}