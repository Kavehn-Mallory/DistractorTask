using DistractorTask.Core;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using DistractorTask.UserStudy.DataDrivenSetup;
using UnityEngine;

namespace DistractorTask.Logging
{
    public class LoggingComponent : MonoBehaviour
    {
        public bool IsServer => userId != string.Empty;

        [SerializeField, ReadOnly]
        private string userId;

        private const int IdLength = 8;


        [ContextMenu("Generate new UserId")]
        private void GenerateNewId()
        {
            userId = GenerateUserId();
        }
        
        [ContextMenu("Clear UserId")]
        private void ClearUserId()
        {
            userId = "";
        }

        private string GenerateUserId()
        {
            string characters = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz1234567890";
            string generated_string = "";

            for(int i = 0; i < IdLength; i++)
                generated_string += characters[Random.Range(0, characters.Length)];

            return generated_string;
        }


        private void Start()
        {
            if (IsServer)
            {
                StudyLog.UserId = userId;
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
            var loggingEndpoint = ipAddressData.Endpoint.WithPort(NetworkExtensions.LoggingPort);
            NetworkManager.Instance.Connect(loggingEndpoint, OnConnectionEstablished);
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