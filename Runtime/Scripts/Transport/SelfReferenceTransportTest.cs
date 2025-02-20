using System;
using System.Collections;
using DistractorTask.Transport.DataContainer;
using UnityEngine;
using UnityEngine.Serialization;

namespace DistractorTask.Transport
{
    public class SelfReferenceTransportTest : MonoBehaviour
    {
        
        public ConnectionDataSettings serverSettings = new()
        {
            port = new ConnectionPortProperty(7777),
            endpointSource = NetworkEndpointSetting.AnyIPv4
        };
        
        public ConnectionDataSettings clientToConnectToAddress = new()
        {
            port = new ConnectionPortProperty(7777),
            endpointSource = NetworkEndpointSetting.AnyIPv4
        };
        
        private IEnumerator Start()
        {
            NetworkConnectionManager.Instance.ListenForRequest(serverSettings.NetworkEndpoint);
            NetworkConnectionManager.Instance.RegisterCallback<LogfileData>(OnLogFileDataReceived);
            yield return new WaitForSeconds(5f);
            

            NetworkConnectionManager.Instance.OnConnectionEstablished += OnConnectionEstablished;
            NetworkConnectionManager.Instance.Connect(clientToConnectToAddress.NetworkEndpoint);
        }

        private void OnConnectionEstablished(bool success)
        {
            NetworkConnectionManager.Instance.TransmitNetworkMessage(new LogfileData
            {
                Message = "Hello",
                NetworkEndpoint = clientToConnectToAddress.NetworkEndpoint,
                Time = new TimeSpan(10, 1, 1, 1),
                LogCategory = LogCategory.Network
            });
        }

        private void OnLogFileDataReceived(LogfileData obj)
        {
            Debug.Log(obj.Message);
        }
    }
}