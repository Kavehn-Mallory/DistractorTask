using System;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using UnityEngine;

namespace DistractorTask.UserStudy.Core
{
    public class DisplayStudyManager : MonoBehaviour
    {

        [SerializeField]
        private Canvas ipAddressCanvas;
        
        private void Start()
        {
            NetworkManager.Instance.RegisterCallback<IpAddressData>(OnIpAddressDataReceived);
            NetworkManager.Instance.StartListening(NetworkHelper.GetLocalEndpoint(NetworkHelper.DisplayWallControlPort, true), null, ConnectionType.Multicast);
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
            throw new NotImplementedException();
        }
    }
}