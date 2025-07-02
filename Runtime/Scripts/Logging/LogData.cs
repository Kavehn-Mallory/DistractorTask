using System;
using System.Collections.Generic;
using System.Globalization;
using DistractorTask.UserStudy.Core;
using UnityEngine;

namespace DistractorTask.Logging
{
    public class LogData
    {
        private long _timeStamp;
        private LogCategory _logCategory;
        private string _userId;
        private string _participantType;
        private Vector3 _cameraPosition;
        private Quaternion _cameraRotation;
        private int _markerPointCount;
        private float _distanceFromCamera;
        private float _distanceToWall;
        private Vector3 _hitPointWallPosition;
        private Vector3 _hitPointWallNormal;
        private Vector3 _anchorPointPosition;
        private string _studyName;
        private int _studyIndex;
        private NoiseLevel _noiseLevel;
        private LoadLevel _loadLevel;
        private int _trialCount;
        private int _repetitionsPerTrial;
        private int _audioTaskReactionTime;
        private int _trialTargetIndex;
        private int _trialSelectedIndex;
        private string _trialSymbolOrder;
        private int _anchorPointIndex;
        private long _startTime;
        private long _reactionTime;
        private Vector3 _leftEyePosition;
        private Vector3 _rightEyePosition;
        private Vector2 _eyeDimensions;
        private Vector2 _pupilDiameter;
        private string _videoPath;
        private string _audioPath;
        private Vector3 _acceleration;
        private Vector3 _angularVelocity;
        private Vector3 _linearAcceleration;
        private Quaternion _attitude;
        private float _lux;
        
        //Acceleration,AngularVelocity,LinearAcceleration,Attitude,Lux


        public static long GetCurrentTimestamp()
        {
            return DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).Ticks;
        }
        
        private static Dictionary<string, string> InitializeDictionary()
        {
            Dictionary<string, string> csvData = new Dictionary<string, string>();
            
            csvData.Add(nameof(_timeStamp), "");
            csvData.Add(nameof(_logCategory), "");
            csvData.Add(nameof(_userId), "");
            csvData.Add(nameof(_participantType), "");
            csvData.Add(nameof(_cameraPosition), "");
            csvData.Add(nameof(_cameraRotation), "");
            csvData.Add(nameof(_markerPointCount), "");
            csvData.Add(nameof(_distanceFromCamera), "");
            csvData.Add(nameof(_distanceToWall), "");
            csvData.Add(nameof(_hitPointWallPosition), "");
            csvData.Add(nameof(_hitPointWallNormal), "");
            csvData.Add(nameof(_anchorPointPosition), "");
            csvData.Add(nameof(_studyName), "");
            csvData.Add(nameof(_studyIndex), "");
            csvData.Add(nameof(_noiseLevel), "");
            csvData.Add(nameof(_loadLevel), "");
            csvData.Add(nameof(_trialCount), "");
            csvData.Add(nameof(_repetitionsPerTrial), "");
            csvData.Add(nameof(_audioTaskReactionTime), "");
            csvData.Add(nameof(_trialTargetIndex), "");
            csvData.Add(nameof(_trialSelectedIndex), "");
            csvData.Add(nameof(_trialSymbolOrder), "");
            csvData.Add(nameof(_anchorPointIndex), "");
            csvData.Add(nameof(_startTime), "");
            csvData.Add(nameof(_reactionTime), "");
            csvData.Add(nameof(_leftEyePosition), "");
            csvData.Add(nameof(_rightEyePosition), "");
            csvData.Add(nameof(_eyeDimensions), "");
            csvData.Add(nameof(_pupilDiameter), "");
            csvData.Add(nameof(_videoPath), "");
            csvData.Add(nameof(_audioPath), "");
            csvData.Add(nameof(_acceleration), "");
            csvData.Add(nameof(_angularVelocity), "");
            csvData.Add(nameof(_linearAcceleration), "");
            csvData.Add(nameof(_attitude), "");
            csvData.Add(nameof(_lux), "");
            
            
            return csvData;
        }

        
        

        public static string WriteLogData(LogData logData)
        {
            var csvData = InitializeDictionary();

            csvData[nameof(_timeStamp)] = $"{logData._timeStamp.ToString()}";
            csvData[nameof(_logCategory)] = logData._logCategory.ToString();
            
            
            switch (logData._logCategory)
            {
                case LogCategory.LogFileStart:
                    csvData[nameof(_userId)] = logData._userId;
                    break;
                case LogCategory.MarkerPointBegin:
                    csvData[nameof(_markerPointCount)] = logData._markerPointCount.ToString();
                    break;
                case LogCategory.MarkerPointConfirmed:
                    csvData[nameof(_cameraPosition)] = logData._cameraPosition.WriteVector3ToCSVString();
                    csvData[nameof(_cameraRotation)] = logData._cameraRotation.WriteQuaternionToCSVString();
                    csvData[nameof(_distanceFromCamera)] = logData._distanceFromCamera.ToString(CultureInfo.InvariantCulture);
                    csvData[nameof(_distanceToWall)] = logData._distanceToWall.ToString(CultureInfo.InvariantCulture);
                    csvData[nameof(_hitPointWallPosition)] = logData._hitPointWallPosition.WriteVector3ToCSVString();
                    csvData[nameof(_hitPointWallNormal)] = logData._hitPointWallNormal.WriteVector3ToCSVString();
                    csvData[nameof(_anchorPointPosition)] = logData._anchorPointPosition.WriteVector3ToCSVString();
                    csvData[nameof(_anchorPointIndex)] = logData._anchorPointIndex.ToString();
                    break;
                case LogCategory.StudyBegin:
                    csvData[nameof(_studyName)] = logData._studyName;
                    csvData[nameof(_studyIndex)] = logData._studyIndex.ToString();
                    csvData[nameof(_participantType)] = logData._participantType;
                    break;
                case LogCategory.TrialBegin:
                    csvData[nameof(_noiseLevel)] = logData._noiseLevel.ToString();
                    csvData[nameof(_loadLevel)] = logData._loadLevel.ToString();
                    csvData[nameof(_trialCount)] = logData._trialCount.ToString();
                    csvData[nameof(_repetitionsPerTrial)] = logData._repetitionsPerTrial.ToString();
                    csvData[nameof(_audioTaskReactionTime)] = logData._audioTaskReactionTime.ToString();
                    break;
                case LogCategory.TrialConfirmation:
                    csvData[nameof(_trialTargetIndex)] = logData._trialTargetIndex.ToString();
                    csvData[nameof(_trialSelectedIndex)] = logData._trialSelectedIndex.ToString();
                    csvData[nameof(_trialSymbolOrder)] = logData._trialSymbolOrder.ToString();
                    csvData[nameof(_startTime)] = logData._audioTaskReactionTime.ToString(CultureInfo.InvariantCulture);
                    csvData[nameof(_reactionTime)] = logData._reactionTime.ToString(CultureInfo.InvariantCulture);
                    csvData[nameof(_trialCount)] = logData._trialCount.ToString();
                    csvData[nameof(_repetitionsPerTrial)] = logData._repetitionsPerTrial.ToString();
                    csvData[nameof(_anchorPointIndex)] = logData._anchorPointIndex.ToString();
                    break;
                case LogCategory.AudioTaskConfirmation:
                    csvData[nameof(_startTime)] = logData._audioTaskReactionTime.ToString("c");
                    csvData[nameof(_reactionTime)] = logData._reactionTime.ToString("c");
                    break;
                case LogCategory.EyeTracking:
                    csvData[nameof(_cameraPosition)] = logData._cameraPosition.WriteVector3ToCSVString();
                    csvData[nameof(_cameraRotation)] = logData._cameraRotation.WriteQuaternionToCSVString();
                    csvData[nameof(_leftEyePosition)] = logData._leftEyePosition.WriteVector3ToCSVString();
                    csvData[nameof(_rightEyePosition)] = logData._rightEyePosition.WriteVector3ToCSVString();
                    csvData[nameof(_eyeDimensions)] = logData._eyeDimensions.WriteVector2ToCSVString();
                    csvData[nameof(_pupilDiameter)] = logData._pupilDiameter.WriteVector2ToCSVString();
                    break;
                case LogCategory.FrameCapture:
                    csvData[nameof(_startTime)] = logData._audioTaskReactionTime.ToString("c");
                    csvData[nameof(_cameraPosition)] = logData._cameraPosition.WriteVector3ToCSVString();
                    csvData[nameof(_cameraRotation)] = logData._cameraRotation.WriteQuaternionToCSVString();
                    break;
                case LogCategory.VideoPlayerChange:
                    csvData[nameof(_videoPath)] = logData._videoPath;
                    csvData[nameof(_audioPath)] = logData._audioPath;
                    break;
                case LogCategory.GyroValues:
                    csvData[nameof(_acceleration)] = logData._acceleration.WriteVector3ToCSVString();
                    csvData[nameof(_angularVelocity)] = logData._angularVelocity.WriteVector3ToCSVString();
                    csvData[nameof(_linearAcceleration)] = logData._linearAcceleration.WriteVector3ToCSVString();
                    csvData[nameof(_attitude)] = logData._attitude.WriteQuaternionToCSVString();
                    break;
                case LogCategory.Lux:
                    csvData[nameof(_lux)] = logData._lux.ToString(CultureInfo.InvariantCulture);
                    break;
            }
            
            
            return $"{csvData[nameof(_timeStamp)]};{csvData[nameof(_logCategory)]};{csvData[nameof(_userId)]};{csvData[nameof(_participantType)]};{csvData[nameof(_cameraPosition)]};{csvData[nameof(_cameraRotation)]};{csvData[nameof(_markerPointCount)]};{csvData[nameof(_distanceFromCamera)]};{csvData[nameof(_distanceToWall)]};{csvData[nameof(_hitPointWallPosition)]};{csvData[nameof(_hitPointWallNormal)]};{csvData[nameof(_anchorPointPosition)]};{csvData[nameof(_studyName)]};{csvData[nameof(_studyIndex)]};{csvData[nameof(_noiseLevel)]};{csvData[nameof(_loadLevel)]};{csvData[nameof(_trialCount)]};{csvData[nameof(_repetitionsPerTrial)]};{csvData[nameof(_audioTaskReactionTime)]};{csvData[nameof(_trialTargetIndex)]};{csvData[nameof(_trialSelectedIndex)]};{csvData[nameof(_trialSymbolOrder)]};{csvData[nameof(_anchorPointIndex)]};{csvData[nameof(_startTime)]};{csvData[nameof(_reactionTime)]};{csvData[nameof(_leftEyePosition)]};{csvData[nameof(_rightEyePosition)]};{csvData[nameof(_eyeDimensions)]};{csvData[nameof(_pupilDiameter)]};{csvData[nameof(_videoPath)]};{csvData[nameof(_audioPath)]};{csvData[nameof(_acceleration)]};{csvData[nameof(_angularVelocity)]};{csvData[nameof(_linearAcceleration)]};{csvData[nameof(_attitude)]};{csvData[nameof(_lux)]}";
        }
        
        

        public static LogData CreateLogFileStartLogData(string userId)
        {
            var result = new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.LogFileStart,
                _userId = userId,
                
            };
            return result;

        }

        public static LogData CreateMarkerPointBeginLogData(int markerPointCount)
        {
            var result = new LogData()
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.MarkerPointBegin,
                _markerPointCount = markerPointCount
            };
            return result;
        }

        public static LogData CreateMarkerPointConfirmedLogData(Vector3 cameraPosition, Quaternion cameraRotation,
            float distanceFromCamera, float distanceToWall, Vector3 hitPointWall, Vector3 hitPointWallNormal,
            Vector3 anchorPointPosition, int anchorPointIndex)
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.MarkerPointConfirmed,
                _cameraPosition = cameraPosition,
                _cameraRotation = cameraRotation,
                _distanceFromCamera = distanceFromCamera,
                _distanceToWall = distanceToWall,
                _hitPointWallPosition = hitPointWall,
                _hitPointWallNormal = hitPointWallNormal,
                _anchorPointPosition = anchorPointPosition,
                _anchorPointIndex = anchorPointIndex
            };
        }

        public static LogData CreateMarkerPointEndLogData()
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.MarkerPointEnd
            };
        }

        public static LogData CreateStudyBeginLogData(string studyName, int studyIndex, string participantType)
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.StudyBegin,
                _studyName = studyName,
                _studyIndex = studyIndex,
                _participantType = participantType
            };
        }

        public static LogData CreateMarkerPointActivatedLogData(int activeMarkerPointIndex)
        {
            var result = new LogData()
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.MarkerPointBegin,
                _markerPointCount = activeMarkerPointIndex
            };
            return result;
        }

        public static LogData CreateTrialBeginLogData(NoiseLevel noiseLevel, LoadLevel loadLevel, int trialCount,
            int repetitionsPerTrial, int audioTaskReactionTime)
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.TrialBegin,
                _noiseLevel = noiseLevel,
                _loadLevel = loadLevel,
                _trialCount = trialCount,
                _repetitionsPerTrial = repetitionsPerTrial,
                _audioTaskReactionTime = audioTaskReactionTime
            };
        }

        public static LogData CreateTrialConfirmationLogData(int trialTargetIndex, int trialSelectedIndex,
            string trialSymbolOrder, long startTime, long reactionTime, int currentTrial, int currentRepetition,
            int anchorPointIndex)
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.TrialConfirmation,
                _trialTargetIndex = trialTargetIndex,
                _trialSelectedIndex = trialSelectedIndex,
                _trialSymbolOrder = trialSymbolOrder,
                _startTime = startTime,
                _reactionTime = reactionTime,
                _trialCount = currentTrial,
                _repetitionsPerTrial = currentRepetition,
                _anchorPointIndex = anchorPointIndex
            };
        }

        public static LogData CreateTrialEndLogData()
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.TrialEnd
            };
        }

        public static LogData CreateStudyEndLogData()
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.StudyEnd
            };
        }

        public static LogData CreateAudioTaskConfirmationLogData(long startTime, long reactionTime)
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.AudioTaskConfirmation,
                _startTime = startTime,
                _reactionTime = reactionTime
            };
        }
        
        
        public static LogData CreateEyeTrackingLogData(Vector3 cameraPosition, Quaternion cameraRotation, Vector3 leftEyePosition, Vector3 rightEyePosition, Vector2 eyeDimensions, Vector2 pupilDiameter)
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.EyeTracking,
                _cameraPosition = cameraPosition,
                _cameraRotation = cameraRotation,
                _leftEyePosition = leftEyePosition,
                _rightEyePosition = rightEyePosition,
                _eyeDimensions = eyeDimensions,
                _pupilDiameter = pupilDiameter
            };
        }
        

        public static LogData CreateFrameCaptureLogData(long startTime, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.FrameCapture,
                _startTime = startTime,
                _cameraPosition = cameraPosition,
                _cameraRotation = cameraRotation
            };
        }

        public static LogData CreateVideoPlayerChangeLogData(string videoPath, string audioPath)
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.VideoPlayerChange,
                _videoPath = videoPath,
                _audioPath = audioPath
            };
        }

        public static LogData CreateGyroValuesLogData(Vector3 acceleration, Vector3 angularVelocity,
            Vector3 linearAcceleration, Quaternion attitude)
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.GyroValues,
                _acceleration = acceleration,
                _angularVelocity = angularVelocity,
                _linearAcceleration = linearAcceleration,
                _attitude = attitude
            };
        }

        public static LogData CreateLuxLogData(float luxValue)
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.Lux,
                _lux = luxValue
            };
        }

        
    }
    
    public enum LogCategory
    {
        DefaultLogData,
        LogFileStart,
        MarkerPointBegin,
        MarkerPointConfirmed,
        MarkerPointEnd,
        StudyBegin,
        TrialBegin,
        TrialConfirmation,
        TrialEnd,
        StudyEnd,
        AudioTaskConfirmation,
        EyeTracking,
        FrameCapture,
        VideoPlayerChange,
        GyroValues,
        Lux
    }
}