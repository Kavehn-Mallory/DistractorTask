using System;
using System.Collections;
using DistractorTask.Transport.DataContainer;
using UnityEngine;

namespace DistractorTask.Transport
{
    public class SelfReferenceTransportTest : MonoBehaviour
    {
        
        public ConnectionDataSettings serverSettings = new()
        {
            port = new ConnectionPortProperty(7777),
            endpointSource = NetworkEndpointSetting.AnyIPv4
        };
        
        public ConnectionDataSettings ipConnectionAddress = new()
        {
            port = new ConnectionPortProperty(7500),
            endpointSource = NetworkEndpointSetting.AnyIPv4
        };

        public ConnectionDataSettings videoTransportSettings = new ConnectionDataSettings
        {
            port = new ConnectionPortProperty(7600),
            endpointSource = NetworkEndpointSetting.AnyIPv4
        };
        
        private IEnumerator Start()
        {
            NetworkManager.Instance.StartListening(serverSettings.NetworkEndpoint.Port, OnConnectionEstablished);
            NetworkManager.Instance.StartListening(ipConnectionAddress.NetworkEndpoint.Port, OnIpConnectionEstablished);
            NetworkManager.Instance.StartListening(videoTransportSettings.port.Port, null);
            NetworkManager.Instance.RegisterCallbackAllPorts<LogfileDataOld>(OnLogFileDataReceived);
            NetworkManager.Instance.RegisterCallback<IpAddressData>(OnIpAddressDataReceived, ipConnectionAddress.port.Port);
            NetworkManager.Instance.RegisterCallbackAllPorts<IpAddressData>(OnAllDataReceived);
            yield return new WaitForSeconds(5f);
            

            //NetworkManager.Instance.OnConnectionEstablished += OnConnectionEstablished;
            //we dont want to subscribe twice 
            Debug.Log("Trying to connect", this);
            NetworkManager.Instance.Connect(ipConnectionAddress.NetworkEndpoint, null);
        }

        private void OnAllDataReceived(IpAddressData arg1, int arg2)
        {
            Debug.Log($"Received that data as well: Trying to connect to {arg1.Endpoint}");
        }

        private void OnIpAddressDataReceived(IpAddressData arg1, int arg2)
        {
            Debug.Log($"Connecting to {arg1.Endpoint}");
            //todo test MarkerPointVisualizationController 
            NetworkManager.Instance.Connect(arg1.Endpoint, OnVideoPortConnectionEstablished);
        }

        private void OnVideoPortConnectionEstablished(ConnectionState obj)
        {
            throw new NotImplementedException();
        }

        private void OnConnectionEstablished(ConnectionState state)
        {
            
            if (state == ConnectionState.Connected)
            {
                NetworkManager.Instance.MulticastMessage(new LogfileDataOld
                {
                    Message = "Hello",
                    NetworkEndpoint = ipConnectionAddress.NetworkEndpoint,
                    Time = new TimeSpan(10, 1, 1, 1),
                    LogCategoryOld = LogCategoryOld.Network
                }, serverSettings.NetworkEndpoint.Port, this.GetInstanceID());
                NetworkManager.Instance.UnregisterToConnectionStateChange(serverSettings.NetworkEndpoint.Port, OnConnectionEstablished);
                return;
            }
            Debug.Log("Connection is invalid");
            
        }

        private void OnIpConnectionEstablished(ConnectionState state)
        {
            Debug.Log("Ip connection established");
            if (state == ConnectionState.Connected)
            {
                NetworkManager.Instance.MulticastMessage(new IpAddressData
                {
                    Endpoint = videoTransportSettings.NetworkEndpoint
                }, ipConnectionAddress.port.Port, 1);
            }
        }

        private void OnLogFileDataReceived(LogfileDataOld obj, int callerId)
        {
            Debug.Log(obj.Message);
        }
        
    }
}