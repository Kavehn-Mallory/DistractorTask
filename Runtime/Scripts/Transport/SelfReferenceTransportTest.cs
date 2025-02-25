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
            var anyIp = NetworkEndpoint.AnyIpv4.WithPort(serverSettings.port.Port);
            var loopback = NetworkEndpoint.LoopbackIpv4.WithPort(serverSettings.port.Port);
            Debug.Log(
                $"Are equal?: {AreEqual(anyIp, loopback)}, {AreEqual(anyIp, serverSettings.NetworkEndpoint)}, {AreEqual(loopback, serverSettings.NetworkEndpoint)}");
            
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
        
        private static bool AreEqual(NetworkEndpoint endpoint, NetworkEndpoint endpoint2)
        {

            if (endpoint2.Equals(endpoint))
            {
                return true;
            }
            //if(handler.Driver.)
            if (endpoint2.Port != endpoint.Port)
            {
                return false;
            }

            if ((endpoint2.IsLoopback || endpoint2 == NetworkEndpoint.AnyIpv4.WithPort(endpoint.Port)))
            {
                return true;
            }
            
            return false;
        }
    }
}