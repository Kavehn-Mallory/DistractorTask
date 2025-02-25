using System.Collections;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using UnityEngine;

namespace DistractorTask.UserStudy.Core
{
    public class ClientSideUserStudyManager : UserStudyManager
    {
        public override INetworkManager Manager => NetworkManager.Instance;

        private bool _hasConnection;
        private bool _receivedStudyStartRequest;

        protected override IEnumerator Start()
        {
            NetworkManager.Instance.RegisterCallback<IpAddressData>(OnIpAddressDataReceived);
            NetworkManager.Instance.StartListening(NetworkHelper.GetLocalIpListeningEndpoint(), null);
            return base.Start();
        }

        private void OnIpAddressDataReceived(IpAddressData obj)
        {
            Debug.Log($"Trying to connect to {obj.Endpoint}");
            NetworkManager.Instance.Connect(obj.Endpoint, OnConnectionEstablished);
        }
        


        private void OnConnectionEstablished(ConnectionState connectionState)
        {
            _hasConnection = connectionState == ConnectionState.Connected;
            if (!_hasConnection)
            {
                return;
            }
            Debug.Log("Connection established in study manager", this);
            if (_receivedStudyStartRequest)
            {
                OnStudyBeginRequest(new RequestStudyBeginData());
            }
        }

        public override void OnStudyBeginRequest(RequestStudyBeginData obj)
        {
            Debug.Log("Study request received");
            _receivedStudyStartRequest = true;
            if (_hasConnection)
            {
                Debug.Log("Study request accepted");
                Manager.UnregisterToConnectionStateChange(NetworkHelper.GetLocalEndpointWithDefaultPort(), OnConnectionEstablished);
                base.OnStudyBeginRequest(obj);
            }
            
        }
    }
}