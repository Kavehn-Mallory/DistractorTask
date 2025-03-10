using System;
using DistractorTask.Transport;
using DistractorTask.UI;

namespace DistractorTask.UserStudy.Core
{
    public class ServerSideUserStudyManager : UserStudyManager
    {
        public StandaloneMessageBroker broker;
        public override INetworkManager Manager => NetworkManager.Instance;

        private void Awake()
        {
            if (broker)
            {
                broker.OnStandaloneStudyStart += StartStudy;
            }
            SecondsToWait = 3f;
        }
        
        
        
    }
}