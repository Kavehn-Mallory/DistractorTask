using System;
using DistractorTask.Transport.DataContainer;
using UnityEngine;
using UnityEngine.Events;

namespace DistractorTask.Transport
{
    public class ConnectionPortListener : MonoBehaviour
    {

        [SerializeField]
        private ushort port;

        [SerializeField]
        private UnityEvent onConnectionEstablished;
        
        private void Start()
        {
            NetworkManager.Instance.RegisterCallback<IpAddressData>(OnIpAddressDataReceived);
            NetworkManager.Instance.StartListening(NetworkHelper.GetLocalEndpoint(port, true), null, ConnectionType.Multicast);
        }

        private void OnIpAddressDataReceived(IpAddressData ipAddressData, int callerId)
        {
            if (callerId == GetInstanceID())
            {
                return;
            }
            NetworkManager.Instance.Connect(ipAddressData.Endpoint, OnConnectionEstablished);
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