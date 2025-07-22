using System;
using DistractorTask.Core;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using DistractorTask.UserStudy.DataDrivenSetup;
using DistractorTask.UserStudy.DistractorSelectionStage;
using DistractorTask.VideoPlayer;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DistractorTask.Logging
{
    public class LoggingComponent : Singleton<LoggingComponent>
    {

        [SerializeField]
        private bool isServer;
        

        private ControlPanel _controlPanel;
        private DistractorSelectionComponent _selectionComponent;
        private VideoPlayerController _videoPlayerController;

        [SerializeField, ReadOnly]
        private string userId;

        private const int IdLength = 8;

        private bool _isLoggingPaused;

        private StudyLog _studyLog;
        
        
        public void GenerateNewId()
        {
            userId = GenerateUserId();
        }
        
        [ContextMenu("Clear UserId")]
        public void ClearUserId()
        {
            userId = "";
        }

        public string UserId => userId;
        public bool IsPaused => _isLoggingPaused;


        private static string GenerateUserId()
        {
            string characters = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz1234567890";
            string generatedString = "";

            for(int i = 0; i < IdLength; i++)
                generatedString += characters[Random.Range(0, characters.Length)];

            return generatedString;
        }
        


        private void Start()
        {
            if (isServer)
            {
                NetworkManager.Instance.StartListening(NetworkExtensions.LoggingPort, OnConnectionStateChanged);
                NetworkManager.Instance.RegisterCallback<LogFileData>(OnLogFileDataReceived, NetworkExtensions.LoggingPort);

            }
            else
            {
                var portListeners = FindObjectsByType<ConnectionPortListener>(FindObjectsSortMode.None);

                foreach (var portListener in portListeners)
                {
                    portListener.OnDataReceived += OnIpDataReceived;
                }
            }
        }

        public void StartLogging()
        {
            if (isServer && userId == string.Empty)
            {
                GenerateNewId();
            }
            _studyLog = new StudyLog(userId);
            WriteLogData(LogData.CreateLogFileStartLogData(userId));


        }

        public void SetUserId(string id)
        {
            userId = id;
        }

        public void TogglePauseLogging()
        {
            Debug.Log("Is pausing");
            _isLoggingPaused = !_isLoggingPaused;
        }

        public void EndLogging()
        {
            _studyLog?.Dispose();
            _studyLog = null;
        }

        private void OnLogFileDataReceived(LogFileData logFileData, int arg2)
        {
            if (Instance._isLoggingPaused)
            {
                return;
            }
            if (_studyLog == null)
            {
                Debug.LogError("We received log data before starting the logging process", this);
                return;
            }
            _studyLog.WriteReceivedLogData(logFileData);
        }
        

        private void OnDisable()
        {
            _studyLog?.Dispose();
            _studyLog = null;
        }

        private void OnDestroy()
        {
            _studyLog?.Dispose();
            _studyLog = null;
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

        private static void WriteLogData(LogData logData)
        {
            if (Instance._isLoggingPaused)
            {
                return;
            }
            if (!Instance.isServer)
            {
                var logFileData = new LogFileData(logData);

                NetworkManager.Instance.MulticastMessage(logFileData, NetworkExtensions.LoggingPort, Instance.GetInstanceID());
            }
            
            Instance._studyLog?.WriteLogData(logData);


        }

        public static void Log(LogData logData)
        {
            WriteLogData(logData);
        }
    }
}