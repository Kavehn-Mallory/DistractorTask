using System;
using DistractorTask.Transport.DataContainer;
using UnityEngine;
using UnityEngine.Events;

namespace DistractorTask.Transport
{
    public class ConnectionPortListener : MonoBehaviour
    {

        [SerializeField]
        private ushort port = NetworkExtensions.IpListeningPort;

        [SerializeField]
        private UnityEvent onConnectionEstablished;

        public event Action<IpAddressData> OnDataReceived = delegate { };

        private void Start()
        {
            NetworkManager.Instance.RegisterCallback<IpAddressData>(OnIpAddressDataReceived, port);
            NetworkManager.Instance.StartListening(port, null);
        }

        private void OnIpAddressDataReceived(IpAddressData ipAddressData, int callerId)
        {
            if (callerId == GetInstanceID())
            {
                return;
            }
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