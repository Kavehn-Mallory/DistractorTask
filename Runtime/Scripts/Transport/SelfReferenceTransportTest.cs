using System;
using System.Collections;
using DistractorTask.Transport.DataContainer;
using Unity.Networking.Transport;
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
            NetworkManager.Instance.StartListening(serverSettings.NetworkEndpoint, OnConnectionEstablished, ConnectionType.Multicast);
            NetworkManager.Instance.RegisterCallback<LogfileData>(OnLogFileDataReceived);
            yield return new WaitForSeconds(5f);
            

            //NetworkManager.Instance.OnConnectionEstablished += OnConnectionEstablished;
            //we dont want to subscribe twice 
            Debug.Log("Trying to connect", this);
            NetworkManager.Instance.Connect(clientToConnectToAddress.NetworkEndpoint, null, ConnectionType.Multicast);
        }
        
        private void OnConnectionEstablished(ConnectionState state)
        {
            
            if (state == ConnectionState.Connected)
            {
                NetworkManager.Instance.Multicast(new LogfileData
                {
                    Message = "Hello",
                    NetworkEndpoint = clientToConnectToAddress.NetworkEndpoint,
                    Time = new TimeSpan(10, 1, 1, 1),
                    LogCategory = LogCategory.Network
                }, serverSettings.NetworkEndpoint);
                NetworkManager.Instance.UnregisterToConnectionStateChange(serverSettings.NetworkEndpoint, OnConnectionEstablished);
                return;
            }
            Debug.Log("Connection is invalid");
            
        }

        private void OnLogFileDataReceived(LogfileData obj)
        {
            Debug.Log(obj.Message);
        }
        
    }
}