using System;
using System.Collections;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using TMPro;
using UnityEngine;

namespace DistractorTask.UserStudy.Core
{
    [Serializable]
    public class ClientSideUserStudyManager : UserStudyManager
    {
        public override INetworkManager Manager => NetworkManager.Instance;

        private bool _hasConnection;
        private bool _receivedStudyStartRequest;
        
        public TextMeshProUGUI debugText; 

        protected override IEnumerator Start()
        {
            NetworkManager.Instance.RegisterCallback<IpAddressData>(OnIpAddressDataReceived, NetworkExtensions.IpListeningPort);
            NetworkManager.Instance.StartListening(NetworkExtensions.IpListeningPort, null, ConnectionType.Multicast);
            
            
            return base.Start();
        }



        private void OnIpAddressDataReceived(IpAddressData obj, int callerId)
        {
            if (callerId == GetInstanceID())
            {
                return;
            }
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
                OnStudyBeginRequest(new RequestStudyBeginData(), GetInstanceID());
            }
        }

        public override void OnStudyBeginRequest(RequestStudyBeginData obj, int callerId)
        {
            if (callerId == GetInstanceID())
            {
                return;
            }
            Debug.Log("Study request received");
            _receivedStudyStartRequest = true;
            if (_hasConnection)
            {
                Debug.Log("Study request accepted");
                Manager.UnregisterToConnectionStateChange(NetworkExtensions.DefaultPort, OnConnectionEstablished);
                base.OnStudyBeginRequest(obj, callerId);
            }
            
        }
    }
}