using System;
using DistractorTask.Transport;

namespace DistractorTask.UserStudy.Core
{
    public class ServerSideUserStudyManager : UserStudyManager
    {
        public override INetworkManager Manager => Server.Instance;

        private void Awake()
        {
            SecondsToWait = 3f;
            LogSystem = Logging.LogSystem.InitializeLogSystem(Server.Instance, Server.Instance.NetworkEndpoint).AsReceiver();
        }

        private void OnDisable()
        {
            LogSystem.SaveFiles();
        }

        public void EstablishConnection()
        {
            Server.Instance.TransmitIpAddress();
        }
    }
}