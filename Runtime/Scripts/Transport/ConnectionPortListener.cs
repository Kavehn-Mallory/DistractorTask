using System;
using DistractorTask.Transport.DataContainer;
using UnityEngine;
using UnityEngine.Events;

namespace DistractorTask.Transport
{
    
    /// <summary>
    /// Uses the IpListeningPort to wait for connection request, opens ip switch connection and then allows for actual connection
    /// </summary>
    public class ConnectionPortListener : MonoBehaviour
    {

        [SerializeField]
        private UnityEvent onConnectionEstablished;

        public event Action<IpAddressData> OnDataReceived = delegate { };

        private void Start()
        {
            NetworkManager.Instance.StartListening(NetworkExtensions.IpListeningPort, null);
            NetworkManager.Instance.RegisterCallback<IpAddressData>(OnIpAddressDataReceived, NetworkExtensions.IpListeningPort);
            Debug.Log($"Listening to {NetworkExtensions.IpListeningPort}");
        }

        private void OnIpAddressDataReceived(IpAddressData ipAddressData, int callerId)
        {
            Debug.Log("Data received");
            if (callerId == GetInstanceID())
            {
                return;
            }
            Debug.Log($"Connecting to {ipAddressData.Endpoint.ToString()}");
            NetworkManager.Instance.Connect(ipAddressData.Endpoint, OnConnectionEstablished);
            OnDataReceived.Invoke(ipAddressData);
        }

        private void OnConnectionEstablished(ConnectionState connectionState)
        {
            if (connectionState != ConnectionState.Connected)
            {
                return;
            }
            onConnectionEstablished.Invoke();
        }
        
    }
    
}