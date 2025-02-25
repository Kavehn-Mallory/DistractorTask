using System;
using DistractorTask.Transport;
using MixedReality.Toolkit.UX;
using TMPro;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;
using UnityEngine;

namespace DistractorTask.UI
{
    public class ConnectivityChecker : MonoBehaviour
    {
        public FontIconSelector selector;
        public TextMeshProUGUI text;


        public string connectedIconName = "Icon 114";
        public string disconnectedIconName = "Icon 126";
        public string attemptingConnectionIconName = "Icon 3";
        public string connectionFailedIconName = "Icon 140";

        private const string Connected = "Connected";
        private const string Disconnected = "Disconnected";
        private const string AttemptingConnection = "Attempting to connect";
        private const string ConnectionFailed = "Connection failed";

        private void Start()
        {
            selector.CurrentIconName = disconnectedIconName;
            text.text = Disconnected;
            NetworkManager.Instance.RegisterToConnectionStateChange(NetworkHelper.GetLocalEndpointWithDefaultPort(), OnConnectionStateChanged);
        }

        private void OnConnectionStateChanged(ConnectionState obj)
        {
            if (obj == ConnectionState.Connected)
            {
                OnConnectionEstablished();
            }
            else if (obj == ConnectionState.ConnectionRequested)
            {
                OnConnectionRequested();
            }
            else if (NetworkHelper.DidConnectionThrowError(obj))
            {
                OnConnectionFailed();
            }
            else
            {
                OnConnectionDisconnected();
            }
        }

        private void OnConnectionDisconnected()
        {
            selector.CurrentIconName = disconnectedIconName;
            text.text = Disconnected;
        }
        

        

        private void OnConnectionEstablished()
        {
            selector.CurrentIconName = connectedIconName;
            text.text = Connected;
        }

        private void OnConnectionFailed()
        {
            selector.CurrentIconName = connectionFailedIconName;
            text.text = ConnectionFailed;
        }

        private void OnConnectionRequested()
        {
            selector.CurrentIconName = attemptingConnectionIconName;
            text.text = AttemptingConnection;
        }
    }
}