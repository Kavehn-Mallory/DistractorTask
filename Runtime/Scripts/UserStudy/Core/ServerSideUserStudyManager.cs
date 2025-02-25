using System;
using DistractorTask.Transport;

namespace DistractorTask.UserStudy.Core
{
    public class ServerSideUserStudyManager : UserStudyManager
    {
        public override INetworkManager Manager => NetworkConnectionManager.Instance;

        private void Awake()
        {
            SecondsToWait = 3f;
        }
        
    }
}