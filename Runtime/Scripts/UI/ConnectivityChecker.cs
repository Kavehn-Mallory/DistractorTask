using DistractorTask.Transport;
using TMPro;
using UnityEngine;

namespace DistractorTask.UI
{
    public class ConnectivityChecker : MonoBehaviour
    {
        public TextMeshProUGUI text;


        //todo update this 
        public string connectedIconName = "Icon 114";
        public string disconnectedIconName = "Icon 126";
        public string attemptingConnectionIconName = "Icon 3";
        public string connectionFailedIconName = "Icon 140";

        private const string Connected = "Connected";
        private const string Disconnected = "Disconnected";
        private const string AttemptingConnection = "Attempting to connect";
        private const string ConnectionFailed = "Connection failed";

        public ushort portToCheck = NetworkExtensions.DefaultPort;

        private void Start()
        {
            text.text = Disconnected;
            NetworkManager.Instance.RegisterToConnectionStateChange(portToCheck, OnConnectionStateChanged);
        }

        public void OnConnectionStateChanged(ConnectionState obj)
        {
            if (obj == ConnectionState.Connected)
            {
                OnConnectionEstablished();
            }
            else if (obj == ConnectionState.ConnectionRequested)
            {
                OnConnectionRequested();
            }
            else if (NetworkExtensions.DidConnectionThrowError(obj))
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
            text.text = Disconnected;
        }
        

        

        private void OnConnectionEstablished()
        {
            text.text = Connected;
        }

        private void OnConnectionFailed()
        {
            text.text = ConnectionFailed;
        }

        private void OnConnectionRequested()
        {
            text.text = AttemptingConnection;
        }
    }
}