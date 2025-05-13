using System;
using System.Threading.Tasks;
using DistractorTask.Core;
using DistractorTask.Transport;
using Unity.Networking.Transport;
using UnityEngine;

namespace DistractorTask.UserStudy.Core.Async
{
    public abstract class AsyncStudyStageComponent<TStudyData, TStudyResponseData> : StudyStageComponent where TStudyData : IRespondingSerializer<TStudyResponseData>, new() where TStudyResponseData : IResponseIdentifier, new()
    {
        [SerializeField]
        private StudyPart studyPart;

        [SerializeField]
        private int messageId;

        public async void PerformStudy(INetworkManager manager)
        {
            switch (studyPart)
            {
                case StudyPart.Sender:
                    await PerformSend();
                    break;
                case StudyPart.Receiver:
                    await PerformReceive();
                    break;
            }
            TriggerStudyEndEvent();
        }

        private async Task PerformReceive()
        {
            //in theory, this should allow for this method to pause until we get data
            await Manager.AwaitRespondableMessage<TStudyData, TStudyResponseData>(null, ConnectionType.Broadcast,
                NetworkExtensions.DefaultPort, ConnectionType.Broadcast, new NetworkEndpoint(), GetInstanceID(), true,
                false);
            

        }

        private async Task PerformSend()
        {
            TriggerStudyStartEvent();
            await Manager.BroadcastMessageAndAwaitResponse<TStudyData, TStudyResponseData>(new TStudyData(),
                GetInstanceID(), messageId);

        }

        public abstract Task PerformSendingStudy();
    }

    [Serializable]
    public enum StudyPart
    {
        Sender,
        Receiver
    }
}