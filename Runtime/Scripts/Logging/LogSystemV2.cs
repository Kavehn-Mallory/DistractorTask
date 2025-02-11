using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using Unity.Networking.Transport;
using UnityEngine;

namespace DistractorTask.Logging
{
    public class LogSystemV2
    {


        private List<INetworkManager> _senders = new();
        
        private const string LogFileHeadings = "Time,TimeStamp,Category,Sender,Data";

        private StreamWriter _streamWriter;

        private readonly string _startTime;

        private static LogSystemV2 _instance;
        
        private static LogSystemV2 LogSystem {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogSystemV2();
                }

                return _instance;
            }
        }

        private LogSystemV2()
        {
            _startTime = DateTime.Now.ToString("u", CultureInfo.InvariantCulture).Replace(':', '-');
            _streamWriter = new StreamWriter(KeyFrameLogPath);
            _streamWriter.WriteLine(LogFileHeadings);
        }

        ~LogSystemV2()
        {
            if (_instance != null)
            {
                _streamWriter.Flush();
                _streamWriter.Close();
            }
        }
        
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


        public static void RegisterSender(INetworkManager sender)
        {
            if(LogSystem._senders.Contains(sender))
                LogSystem._senders.Add(sender);
        }

        public static bool UnregisterSender(INetworkManager sender)
        {
            return LogSystem._senders.Remove(sender);
        }

        public static void RegisterReceiver(INetworkManager receiver)
        {
            receiver.RegisterCallback<LogfileData>(OnLogDataReceived);
        }
        
        public static void UnregisterReceiver(INetworkManager receiver)
        {
            receiver.UnregisterCallback<LogfileData>(OnLogDataReceived);
        }

        private static void OnLogDataReceived(LogfileData logfileData)
        {
            LogSystem._streamWriter.WriteLine($"{logfileData.Time:c},{logfileData.LogCategory.ToString()},{logfileData.NetworkEndpoint.ToString()},{logfileData.Message}");
        }


        public static void LogKeyframe(LogCategory category, string message)
        {
            var logfileData = (new LogfileData
            {
                NetworkEndpoint = NetworkEndpoint.AnyIpv4,
                Time = DateTime.Now.TimeOfDay,
                LogCategory = category,
                Message = message
            });
            LogSystem._streamWriter.WriteLine($"{logfileData.Time:c},{logfileData.LogCategory.ToString()},{logfileData.NetworkEndpoint.ToString()},{logfileData.Message}");
            

            foreach (var sender in LogSystem._senders)
            {
                SendKeyframe(logfileData, sender);
            }
        }

        private static void SendKeyframe(LogfileData logfileData, INetworkManager sender)
        {
            logfileData.NetworkEndpoint = sender.NetworkEndpoint;
            sender.TransmitNetworkMessage(logfileData);
        }

        public static void LogKeyframe(LogCategory category, string message, INetworkManager sender)
        {
            var logfileData = (new LogfileData
            {
                NetworkEndpoint = NetworkEndpoint.AnyIpv4,
                Time = DateTime.Now.TimeOfDay,
                LogCategory = category,
                Message = message
            });
            LogSystem._streamWriter.WriteLine($"{logfileData.Time:c},{logfileData.LogCategory.ToString()},{logfileData.NetworkEndpoint.ToString()},{logfileData.Message}");
            
            SendKeyframe(logfileData, sender);
            
        }

        
    }
}