using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using Unity.Networking.Transport;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace DistractorTask.Logging
{
    public class LogSystem
    {
        private readonly INetworkManager _networkManager;

        private bool _isReceiver;

        private readonly NetworkEndpoint _endpoint;

        private static LogSystem _logSystem;
        
        private string _startTime;

        private readonly List<string> _keyFrames = new();

        public string KeyFrameLogPath
        {
            get
            {
#if UNITY_EDITOR
                return Application.streamingAssetsPath + $"/{_startTime}_Logfile.csv";
#else
                return Application.persistentDataPath + $"/{_startTime}_Logfile.csv";
#endif

            }

        }
        
        public string DataLogPath
        {
            get
            {
#if UNITY_EDITOR
                return Application.streamingAssetsPath + $"/{_startTime}_Logfile.json";
#else
                return Application.persistentDataPath + $"/{_startTime}LogFile.json";
#endif

            }

        }

        public const string LogFileHeadings = "TimeStamp,Category,Sender,Data";

        private LogSystem(INetworkManager networkManager, NetworkEndpoint endpoint)
        {
            _networkManager = networkManager;
            _endpoint = endpoint;
            _logSystem = this;
            _startTime = DateTime.Now.ToString("u", CultureInfo.InvariantCulture).Replace(':', '-');
        }
        

        private bool SendLog(LogCategory category, string message)
        {
            var logfileData = (new LogfileData
            {
                NetworkEndpoint = _endpoint,
                Time = DateTime.Now.TimeOfDay,
                LogCategory = category,
                Message = message
            });
            if (_isReceiver)
            {
                OnLogDataReceived(logfileData);
                return true;
            }
            return _networkManager.TransmitNetworkMessage(logfileData);
        }

        public static bool LogKeyframe(LogCategory category, string message)
        {
            if (_logSystem == null)
            {
                throw new NullReferenceException($"{nameof(LogSystem)} was never set up");
            }
            return _logSystem.SendLog(category, message);
        }


        public static LogSystem InitializeLogSystem(INetworkManager networkManager, NetworkEndpoint endpoint)
        {
            return new LogSystem(networkManager, endpoint);
        }
        
        public LogSystem AsReceiver()
        {
            if (_networkManager == null)
            {
                throw new NullReferenceException($"{nameof(LogSystem)} is missing a {nameof(INetworkManager)}");
            }

            _isReceiver = true;
            SetupFiles();
            _networkManager.RegisterCallback<LogfileData>(OnLogDataReceived);
            return this;
        }

        private void SetupFiles()
        {
            Debug.Log($"Trying to write to {KeyFrameLogPath}");
            _keyFrames.Clear();
            File.WriteAllText(KeyFrameLogPath, LogFileHeadings);
        }

        public void EndLogging()
        {
            if (!_isReceiver)
            {
                return;
            }
            _isReceiver = false;
            _networkManager.UnregisterCallback<LogfileData>(OnLogDataReceived);
            SaveFiles();
        }

        private void OnLogDataReceived(LogfileData obj)
        {
            _keyFrames.Add($"{obj.Time:c},{obj.LogCategory.ToString()},{obj.NetworkEndpoint.ToString()},{obj.Message}");
        }

        public void SaveFiles()
        {
            Debug.Log($"Writing {_keyFrames.Count} frames");
            File.AppendAllLines(KeyFrameLogPath, _keyFrames);
            _keyFrames.Clear();
#if UNITY_EDITOR
            AssetDatabase.Refresh();
            Debug.Log("Saved");
#endif
        }
    }
    
}