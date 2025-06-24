using System;
using System.Globalization;
using System.IO;
using DistractorTask.Core;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using Unity.Networking.Transport;
using UnityEngine;

namespace DistractorTask.Logging
{
    public class StudyLog
    {
        private const string LogFileHeadings = "Time,Category,Endpoint,Sender,Data";

        private static StudyLog _instance;
        
        private readonly string _startTime;
        
        
        //todo we should probably do that differently. maybe the stream writer is only allocated when the first message attempt is started? currently we have no way of guaranteeing that the id won't change at some random time 
        
        private static StudyLog LogSystem {
            get
            {
                if (_instance == null)
                {
                    _instance = new StudyLog();
                    //UserId = "";
                }

                return _instance;
            }
        }

        public static string UserId = "";

        private static bool IsServer => UserId != string.Empty;
        
        private string KeyFrameLogPath
        {
            get
            {
#if UNITY_EDITOR
                return Application.streamingAssetsPath + $"/{_startTime}_{UserId}_Logfile.csv";
#else
                return Application.persistentDataPath + $"/{_startTime}_{UserId}_Logfile.csv";
#endif

            }

        }


        private StreamWriter _streamWriter;

        private StudyLog()
        {
            _startTime = DateTime.Now.ToString("u", CultureInfo.InvariantCulture).Replace(':', '-');
            _streamWriter = new StreamWriter(KeyFrameLogPath);
            _streamWriter.WriteLine(LogFileHeadings);
        }
        
        ~StudyLog()
        {
            if (_instance != null)
            {
                _streamWriter.Flush();
                _streamWriter.Close();
                _streamWriter.Dispose();
            }

            UserId = "";
        }

        public static void RegisterLog<T>(ushort port) where T : ISerializer, ILogSerializer, new()
        {
            NetworkManager.Instance.RegisterCallback<T>(LogKeyframe, port);
        }
        
        public static void UnregisterLog<T>(ushort port) where T : ISerializer, ILogSerializer, new()
        {
            NetworkManager.Instance.UnregisterCallback<T>(LogKeyframe, port);
        }

        public static void RegisterLog<T>() where T : ISerializer, ILogSerializer, new()
        {
            NetworkManager.Instance.RegisterCallbackAllPorts<T>(LogKeyframe);
        }
        
        public static void UnregisterLog<T>() where T : ISerializer, ILogSerializer, new ()
        {
            NetworkManager.Instance.UnregisterCallbackAllPorts<T>(LogKeyframe);
        }

        private static void LogKeyframe<T>(T data, int senderId) where T : ISerializer, ILogSerializer, new()
        {
            var message = data.Serialize().Replace(',', ';');
            var logfileData = (new LogfileData
            {
                NetworkEndpoint = NetworkEndpoint.AnyIpv4,
                Time = DateTime.Now.TimeOfDay,
                LogCategory = data.Category,
                Message = message
            });
            LogSystem._streamWriter.WriteLine($"{logfileData.Time:c},{logfileData.LogCategory.ToString()},{logfileData.NetworkEndpoint.ToString()},{senderId},{logfileData.Message}");
            if(!IsServer)
                NetworkManager.Instance.MulticastMessage(logfileData, NetworkExtensions.LoggingPort, -1);
        }

        public static void LogCustomKeyframe(LogCategory logCategory, string message)
        {
            message = message.Replace(',', ';');
            var logfileData = (new LogfileData
            {
                NetworkEndpoint = NetworkEndpoint.AnyIpv4,
                Time = DateTime.Now.TimeOfDay,
                LogCategory = logCategory,
                Message = message
            });
            LogSystem._streamWriter.WriteLine($"{logfileData.Time:c},{logfileData.LogCategory.ToString()},{logfileData.NetworkEndpoint.ToString()},{logfileData.LogCategory.ToString()},{logfileData.Message}");
            if(!IsServer)
                NetworkManager.Instance.MulticastMessage(logfileData, NetworkExtensions.LoggingPort, -1);
        }
        
    }
}