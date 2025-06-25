using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using DistractorTask.Core;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using Unity.Networking.Transport;
using UnityEditor;
using UnityEngine;

namespace DistractorTask.Logging
{
    public class StudyLog : IDisposable, IAsyncDisposable
    {
        private const string LogFileHeadings = "Time,Category,Endpoint,Sender,Data";

        private static StudyLog _instance;
        
        private readonly string _startTime;

        private const bool OptimizeForExcel = true;

        private const char Delimiter = OptimizeForExcel ? ';' : ',';
        
        
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

        public static void DisposeLogSystem()
        {
            _instance?.Dispose();
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
            var logFileHeadings = LogFileHeadings;
            if (OptimizeForExcel)
            {
                logFileHeadings = LogFileHeadings.Replace(',', ';');
            }
            _streamWriter.WriteLine(logFileHeadings);
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
            var message = SanitizeMessage(data.Serialize());
            var logfileData = (new LogfileData
            {
                NetworkEndpoint = NetworkEndpoint.AnyIpv4,
                Time = DateTime.Now.TimeOfDay,
                LogCategory = data.Category,
                Message = message
            });
            LogSystem._streamWriter.WriteLine($"{logfileData.Time:c}{Delimiter}{logfileData.LogCategory.ToString()}{Delimiter}{logfileData.NetworkEndpoint.ToString()}{Delimiter}{senderId}{Delimiter}{logfileData.Message}");
            if(!IsServer)
                NetworkManager.Instance.MulticastMessage(logfileData, NetworkExtensions.LoggingPort, -1);
        }

        public static void LogCustomKeyframe(LogCategory logCategory, string message)
        {
            message = SanitizeMessage(message);
            var logfileData = (new LogfileData
            {
                NetworkEndpoint = NetworkEndpoint.AnyIpv4,
                Time = DateTime.Now.TimeOfDay,
                LogCategory = logCategory,
                Message = message
            });
            LogSystem._streamWriter.WriteLine($"{logfileData.Time:c}{Delimiter}{logfileData.LogCategory.ToString()}{Delimiter}{logfileData.NetworkEndpoint.ToString()}{Delimiter}{logfileData.LogCategory.ToString()}{Delimiter}{logfileData.Message}");
            if(!IsServer)
                NetworkManager.Instance.MulticastMessage(logfileData, NetworkExtensions.LoggingPort, -1);
        }

        private static string SanitizeMessage(string message)
        {
            if (OptimizeForExcel)
            {
                return message.Replace(';', ',');
            }
            return message.Replace(',', ';');
        }

        public void Dispose()
        {
            _streamWriter?.Flush();
            _streamWriter?.Close();
            _streamWriter?.Dispose();
            UserId = "";

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public async ValueTask DisposeAsync()
        {
            if (_streamWriter != null) await _streamWriter.DisposeAsync();
        }
    }
}