using System.Collections;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using UnityEngine;

namespace DistractorTask.UserStudy.Core
{
    public class ClientSideUserStudyManager : UserStudyManager
    {
        public override INetworkManager Manager => NetworkConnectionManager.Instance;

        private bool _hasConnection;
        private bool _receivedStudyStartRequest;

        protected override IEnumerator Start()
        {
            NetworkConnectionManager.Instance.OnConnectionEstablished += OnConnectionEstablished;
            return base.Start();
        }

        private void OnConnectionEstablished(bool connectionSuccessful)
        {
            _hasConnection = connectionSuccessful;
            if (!_hasConnection)
            {
                return;
            }
            Debug.Log("Connection established in study manager");
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
                base.OnStudyBeginRequest(obj);
            }
            
        }
    }
}