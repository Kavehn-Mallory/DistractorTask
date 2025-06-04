using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using DistractorTask.UserStudy.DataDrivenSetup;
using UnityEngine;

namespace DistractorTask.Logging
{
    public class LoggingComponent : MonoBehaviour
    {
        public bool isServer;


        private void Start()
        {
            if (isServer)
            {
                NetworkManager.Instance.StartListening(NetworkExtensions.LoggingPort, OnConnectionStateChanged);
            }
            else
            {
                var portListeners = FindObjectsOfType<ConnectionPortListener>();

                foreach (var portListener in portListeners)
                {
                    portListener.OnDataReceived += OnIpDataReceived;
                }
            }
            StudyLog.RegisterLog<MarkerPointCountData>();
        }

        private void OnIpDataReceived(IpAddressData ipAddressData)
        {
            NetworkManager.Instance.Connect(ipAddressData.Endpoint, OnConnectionEstablished);
        }

        private void OnConnectionEstablished(ConnectionState obj)
        {
            Debug.Log("Sending log data to server from now on");
        }

        private void OnConnectionStateChanged(ConnectionState obj)
        {
            if (obj == ConnectionState.Connected)
            {
                Debug.Log("Connection established!");
                return;
            }
            Debug.Log("Something went wrong");
        }
    }
}