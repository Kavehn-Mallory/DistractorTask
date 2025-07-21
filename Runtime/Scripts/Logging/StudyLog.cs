using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using DistractorTask.Transport.DataContainer;
using UnityEditor;
using UnityEngine;

namespace DistractorTask.Logging
{
    public class StudyLog : IDisposable, IAsyncDisposable
    {
        private const string LogFileHeadings = "Time,Timestamp,Category,UserId,ParticipantType,CameraPosition,CameraRotation,MarkerPointCount,DistanceFromCamera,DistanceToWall,HitPointWallPosition,HitPointWallNormal,AnchorPointPosition,StudyName,StudyIndex,NoiseLevel,LoadLevel,TrialCount,RepetitionsPerTrial,AudioTaskReactionTime,TrialTargetIndex,TrialSelectedIndex,TrialSymbolOrder,AnchorPointIndex,StartTime,ReactionTime,LeftEyePosition,RightEyePosition,EyeDimensions,PupilDiameter,GazeBehaviour,GazeBehaviourDuration,VideoPath,AudioPath,Acceleration,AngularVelocity,LinearAcceleration,Attitude,Lux";
        
        private readonly string _startTime;

        private string _userId;
        
        
        private string KeyFrameLogPath
        {
            get
            {
#if UNITY_EDITOR
                return Application.streamingAssetsPath + $"/{_startTime}_{_userId}_Logfile.csv";
#else
                return Application.persistentDataPath + $"/{_startTime}_{_userId}_Logfile.csv";
#endif

            }

        }


        private StreamWriter _streamWriter;

        public StudyLog(string userId)
        {
            _userId = userId;
            _startTime = DateTime.Now.ToString("u", CultureInfo.InvariantCulture).Replace(':', '-');
            _streamWriter = new StreamWriter(KeyFrameLogPath);
            var logFileHeadings = LogFileHeadings.SanitizeCSVLine();
            _streamWriter.WriteLine(logFileHeadings);
        }
        


        public void Dispose()
        {
            _streamWriter?.Flush();
            _streamWriter?.Close();
            _streamWriter?.Dispose();
            _streamWriter = null;

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public async ValueTask DisposeAsync()
        {
            if (_streamWriter != null) await _streamWriter.DisposeAsync();
            _streamWriter = null;
        }

        public void WriteLogData(LogData logData)
        {
            _streamWriter.WriteLine(
                $"{DateTime.Now.TimeOfDay:c}{LoggingExtensions.Delimiter}{LogData.WriteLogData(logData)}");
        }

        public void WriteReceivedLogData(LogFileData logFileData)
        {
            _streamWriter.WriteLine(
                $"{DateTime.Now.TimeOfDay:c}{LoggingExtensions.Delimiter}{logFileData.Value}");
        }
    }
}