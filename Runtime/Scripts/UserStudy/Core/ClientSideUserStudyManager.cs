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
            NetworkManager.Instance.RegisterCallback<IpAddressData>(OnIpAddressDataReceived);
            NetworkManager.Instance.StartListening(NetworkHelper.GetLocalIpListeningEndpoint(), null, ConnectionType.Multicast);
            
            NetworkManager.Instance.DebugAction += OnDebugActionOfNetworkManager;
            
            return base.Start();
        }

        private void OnDebugActionOfNetworkManager(string obj)
        {
            if (debugText)
            {
                debugText.text = obj;
            }
            
        }

        private void OnIpAddressDataReceived(IpAddressData obj, int callerId)
        {
            if (callerId == GetInstanceID())
            {
                return;
            }
            OnDebugActionOfNetworkManager($"Trying to connect to {obj.Endpoint}");
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
                Manager.UnregisterToConnectionStateChange(NetworkHelper.DefaultPort, OnConnectionEstablished);
                base.OnStudyBeginRequest(obj, callerId);
            }
            
        }
    }
}