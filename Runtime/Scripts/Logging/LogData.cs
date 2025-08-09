using System;
using System.Collections.Generic;
using System.Globalization;
using DistractorTask.UserStudy.Core;
using UnityEngine;

namespace DistractorTask.Logging
{
    [Serializable]
    public class LogData
    {
        #region ReadOnly Properties

        public long TimeStamp => _timeStamp;
        public LogCategory LogCategory => _logCategory;
        public string UserId1 => _userId;
        public string ParticipantType1 => _participantType;
        public Vector3 CameraPosition1 => _cameraPosition;
        public Quaternion CameraRotation1 => _cameraRotation;
        public int MarkerPointCount1 => _markerPointCount;
        public float DistanceFromCamera1 => _distanceFromCamera;
        public float DistanceToWall1 => _distanceToWall;
        public Vector3 HitPointWallPosition1 => _hitPointWallPosition;
        public Vector3 HitPointWallNormal1 => _hitPointWallNormal;
        public Vector3 AnchorPointPosition1 => _anchorPointPosition;
        public string StudyName1 => _studyName;
        public int StudyIndex1 => _studyIndex;
        public NoiseLevel NoiseLevel1 => _noiseLevel;
        public LoadLevel LoadLevel1 => _loadLevel;
        public int TrialCount1 => _trialCount;
        public int RepetitionsPerTrial1 => _repetitionsPerTrial;
        public int AudioTaskReactionTime1 => _audioTaskReactionTime;
        public int TrialTargetIndex1 => _trialTargetIndex;
        public int TrialSelectedIndex1 => _trialSelectedIndex;
        public string TrialSymbolOrder1 => _trialSymbolOrder;
        public int AnchorPointIndex1 => _anchorPointIndex;
        public long StartTime1 => _startTime;
        public long ReactionTime1 => _reactionTime;
        public Vector3 LeftEyePosition1 => _leftEyePosition;
        public Vector3 RightEyePosition1 => _rightEyePosition;
        public Vector2 EyeDimensions1 => _eyeDimensions;
        public Vector2 PupilDiameter1 => _pupilDiameter;
        public string GazeBehaviour1 => _gazeBehaviour;
        public ulong GazeBehaviourDuration1 => _gazeBehaviourDuration;
        public string VideoPath1 => _videoPath;
        public string AudioPath1 => _audioPath;
        public Vector3 Acceleration1 => _acceleration;
        public Vector3 AngularVelocity1 => _angularVelocity;
        public Vector3 LinearAcceleration1 => _linearAcceleration;
        public Quaternion Attitude1 => _attitude;
        public float Lux1 => _lux;

        #endregion

        [SerializeField]
        private long _timeStamp;
        [SerializeField]
        private LogCategory _logCategory;
        [SerializeField]
        private string _userId;
        [SerializeField]
        private string _participantType;
        [SerializeField]
        private Vector3 _cameraPosition;
        [SerializeField]
        private Quaternion _cameraRotation;
        [SerializeField]
        private int _markerPointCount;
        [SerializeField]
        private float _distanceFromCamera;
        [SerializeField]
        private float _distanceToWall;
        [SerializeField]
        private Vector3 _hitPointWallPosition;
        [SerializeField]
        private Vector3 _hitPointWallNormal;
        [SerializeField]
        private Vector3 _anchorPointPosition;
        [SerializeField]
        private string _studyName;
        [SerializeField]
        private int _studyIndex;
        [SerializeField]
        private NoiseLevel _noiseLevel;
        [SerializeField]
        private LoadLevel _loadLevel;
        [SerializeField]
        private int _trialCount;
        [SerializeField]
        private int _repetitionsPerTrial;
        [SerializeField]
        private int _audioTaskReactionTime;
        [SerializeField]
        private int _trialTargetIndex;
        [SerializeField]
        private int _trialSelectedIndex;
        [SerializeField]
        private string _trialSymbolOrder;
        [SerializeField]
        private int _anchorPointIndex;
        [SerializeField]
        private long _startTime;
        [SerializeField]
        private long _reactionTime;
        [SerializeField]
        private Vector3 _leftEyePosition;
        [SerializeField]
        private Vector3 _rightEyePosition;
        [SerializeField]
        private Vector2 _eyeDimensions;
        [SerializeField]
        private Vector2 _pupilDiameter;
        [SerializeField]
        private string _gazeBehaviour;
        [SerializeField]
        private ulong _gazeBehaviourDuration;
        [SerializeField]
        private string _videoPath;
        [SerializeField]
        private string _audioPath;
        [SerializeField]
        private Vector3 _acceleration;
        [SerializeField]
        private Vector3 _angularVelocity;
        [SerializeField]
        private Vector3 _linearAcceleration;
        [SerializeField]
        private Quaternion _attitude;
        [SerializeField]
        private float _lux;


        //todo we could change this back to the actual names of the headers and not the names of the properties. The original idea does not work 

         private const string Timestamp = nameof(_timeStamp);
         private const string Category = nameof(_logCategory);
         private const string UserId = nameof(_userId);
         private const string ParticipantType = nameof(_participantType);
         private const string CameraPosition = nameof(_cameraPosition);
         private const string CameraRotation = nameof(_cameraRotation);
         private const string MarkerPointCount = nameof(_markerPointCount);
         private const string DistanceFromCamera = nameof(_distanceFromCamera);
         private const string DistanceToWall = nameof(_distanceToWall);
         private const string HitPointWallPosition = nameof(_hitPointWallPosition);
         private const string HitPointWallNormal = nameof(_hitPointWallNormal);
         private const string AnchorPointPosition = nameof(_anchorPointPosition);
         private const string StudyName = nameof(_studyName);
         private const string StudyIndex = nameof(_studyIndex);
         private const string NoiseLevel = nameof(_noiseLevel);
         private const string LoadLevel = nameof(_loadLevel);
         private const string TrialCount = nameof(_trialCount);
         private const string RepetitionsPerTrial = nameof(_repetitionsPerTrial);
         private const string AudioTaskReactionTime = nameof(_audioTaskReactionTime);
         private const string TrialTargetIndex = nameof(_trialTargetIndex);
         private const string TrialSelectedIndex = nameof(_trialSelectedIndex);
         private const string TrialSymbolOrder = nameof(_trialSymbolOrder);
         private const string AnchorPointIndex = nameof(_anchorPointIndex);
         private const string StartTime = nameof(_startTime);
         private const string ReactionTime = nameof(_reactionTime);
         private const string LeftEyePosition = nameof(_leftEyePosition);
         private const string RightEyePosition = nameof(_rightEyePosition);
         private const string EyeDimensions = nameof(_eyeDimensions);
         private const string PupilDiameter = nameof(_pupilDiameter);
         private const string GazeBehaviour = nameof(_gazeBehaviour);
         private const string GazeBehaviourDuration = nameof(_gazeBehaviourDuration);
         private const string VideoPath = nameof(_videoPath);
         private const string AudioPath = nameof(_audioPath);
         private const string Acceleration = nameof(_acceleration);
         private const string AngularVelocity = nameof(_angularVelocity);
         private const string LinearAcceleration = nameof(_linearAcceleration);
         private const string Attitude = nameof(_attitude);
         private const string Lux = nameof(_lux);



        public static LogData LoadLogDataFromCSVLine(string header, string line)
        {
            var headerElements = header.Split(';');
            var elements = line.Split(';');

            var indices = CreatePropertyIndices(headerElements, nameof(Category));

            var logFile = new LogData();
            

            logFile._timeStamp = long.Parse(GetPropertyValue(elements, indices[nameof(Timestamp)]), CultureInfo.InvariantCulture);
            logFile._logCategory = Enum.Parse<LogCategory>(GetPropertyValue(elements, indices[nameof(Category)]));



            switch (logFile._logCategory)
            {
                case LogCategory.LogFileStart:
                    logFile._userId = GetPropertyValue(elements, indices[nameof(UserId)]);
                    break;
                case LogCategory.MarkerPointBegin:
                    logFile._markerPointCount = int.Parse(GetPropertyValue(elements, indices[nameof(MarkerPointCount)]));
                    break;
                case LogCategory.MarkerPointConfirmed:
                    logFile._cameraPosition = GetPropertyValue(elements, indices[nameof(CameraPosition)]).ReadVector3FromCSV();
                    logFile._cameraRotation = GetPropertyValue(elements, indices[nameof(CameraRotation)]).ReadQuaternionFromCSV();
                    logFile._distanceFromCamera = float.Parse(GetPropertyValue(elements, indices[nameof(DistanceFromCamera)]), CultureInfo.InvariantCulture);
                    logFile._distanceToWall = float.Parse(GetPropertyValue(elements, indices[nameof(DistanceToWall)]), CultureInfo.InvariantCulture);
                    logFile._hitPointWallPosition = GetPropertyValue(elements, indices[nameof(HitPointWallPosition)]).ReadVector3FromCSV();
                    logFile._hitPointWallNormal = GetPropertyValue(elements, indices[nameof(HitPointWallNormal)]).ReadVector3FromCSV();
                    logFile._anchorPointPosition = GetPropertyValue(elements, indices[nameof(AnchorPointPosition)]).ReadVector3FromCSV();
                    logFile._anchorPointIndex = int.Parse(GetPropertyValue(elements, indices[nameof(AnchorPointIndex)]));
                    break;
                case LogCategory.StudyBegin:
                    logFile._studyName = GetPropertyValue(elements, indices[nameof(StudyName)]);
                    logFile._studyIndex = int.Parse(GetPropertyValue(elements, indices[nameof(StudyIndex)]));
                    logFile._participantType = GetPropertyValue(elements, indices[nameof(ParticipantType)]);
                    break;
                case LogCategory.TrialBegin:
                    logFile._noiseLevel = Enum.Parse<NoiseLevel>(GetPropertyValue(elements, indices[nameof(NoiseLevel)]));
                    logFile._loadLevel = Enum.Parse<LoadLevel>(GetPropertyValue(elements, indices[nameof(LoadLevel)]));
                    logFile._trialCount = int.Parse(GetPropertyValue(elements, indices[nameof(TrialCount)]));
                    logFile._repetitionsPerTrial = int.Parse(GetPropertyValue(elements, indices[nameof(RepetitionsPerTrial)]));
                    logFile._audioTaskReactionTime = int.Parse(GetPropertyValue(elements, indices[nameof(AudioTaskReactionTime)]));
                    break;
                case LogCategory.TrialConfirmation:
                    logFile._trialTargetIndex = int.Parse(GetPropertyValue(elements, indices[nameof(TrialTargetIndex)]));
                    logFile._trialSelectedIndex = int.Parse(GetPropertyValue(elements, indices[nameof(TrialSelectedIndex)]));
                    logFile._trialSymbolOrder = GetPropertyValue(elements, indices[nameof(TrialSymbolOrder)]);
                    logFile._startTime = long.Parse(GetPropertyValue(elements, indices[nameof(StartTime)]), CultureInfo.InvariantCulture);
                    logFile._reactionTime = long.Parse(GetPropertyValue(elements, indices[nameof(ReactionTime)]), CultureInfo.InvariantCulture);
                    logFile._trialCount = int.Parse(GetPropertyValue(elements, indices[nameof(TrialCount)]));
                    logFile._repetitionsPerTrial = int.Parse(GetPropertyValue(elements, indices[nameof(RepetitionsPerTrial)]));
                    logFile._anchorPointIndex = int.Parse(GetPropertyValue(elements, indices[nameof(AnchorPointIndex)]));
                    break;
                case LogCategory.AudioTaskConfirmation:
                    logFile._startTime = long.Parse(GetPropertyValue(elements, indices[nameof(StartTime)]), CultureInfo.InvariantCulture);
                    logFile._reactionTime = long.Parse(GetPropertyValue(elements, indices[nameof(ReactionTime)]), CultureInfo.InvariantCulture);
                    break;
                case LogCategory.EyeTracking:
                    logFile._cameraPosition = GetPropertyValue(elements, indices[nameof(CameraPosition)]).ReadVector3FromCSV();
                    logFile._cameraRotation = GetPropertyValue(elements, indices[nameof(CameraRotation)]).ReadQuaternionFromCSV();
                    logFile._leftEyePosition = GetPropertyValue(elements, indices[nameof(LeftEyePosition)]).ReadVector3FromCSV();
                    logFile._rightEyePosition = GetPropertyValue(elements, indices[nameof(RightEyePosition)]).ReadVector3FromCSV();
                    logFile._eyeDimensions = GetPropertyValue(elements, indices[nameof(EyeDimensions)]).ReadVector2FromCSV();
                    logFile._pupilDiameter = GetPropertyValue(elements, indices[nameof(PupilDiameter)]).ReadVector2FromCSV();
                    logFile._reactionTime = long.Parse(GetPropertyValue(elements, indices[nameof(ReactionTime)]), CultureInfo.InvariantCulture);
                    logFile._startTime = long.Parse(GetPropertyValue(elements, indices[nameof(StartTime)]), CultureInfo.InvariantCulture);
                    logFile._gazeBehaviour = GetPropertyValue(elements, indices[nameof(GazeBehaviour)]);
                    logFile._gazeBehaviourDuration = ulong.Parse(GetPropertyValue(elements, indices[nameof(GazeBehaviourDuration)]), CultureInfo.InvariantCulture);

                    break;
                case LogCategory.FrameCapture:
                    logFile._startTime = long.Parse(GetPropertyValue(elements, indices[nameof(StartTime)]), CultureInfo.InvariantCulture);
                    logFile._cameraPosition = GetPropertyValue(elements, indices[nameof(CameraPosition)]).ReadVector3FromCSV();
                    logFile._cameraRotation = GetPropertyValue(elements, indices[nameof(CameraRotation)]).ReadQuaternionFromCSV();
                    logFile._videoPath = GetPropertyValue(elements, indices[nameof(VideoPath)]);
                    break;
                case LogCategory.VideoPlayerChange:
                    logFile._videoPath = GetPropertyValue(elements, indices[nameof(VideoPath)]);
                    logFile._audioPath = GetPropertyValue(elements, indices[nameof(AudioPath)]);
                    break;
                case LogCategory.GyroValues:
                    logFile._acceleration = GetPropertyValue(elements, indices[nameof(Acceleration)]).ReadVector3FromCSV();
                    logFile._angularVelocity = GetPropertyValue(elements, indices[nameof(AngularVelocity)]).ReadVector3FromCSV();
                    logFile._linearAcceleration = GetPropertyValue(elements, indices[nameof(LinearAcceleration)]).ReadVector3FromCSV();
                    logFile._attitude = GetPropertyValue(elements, indices[nameof(Attitude)]).ReadQuaternionFromCSV();
                    break;
                case LogCategory.Lux:
                    logFile._lux = float.Parse(GetPropertyValue(elements, indices[nameof(Lux)]), CultureInfo.InvariantCulture);
                    break;

            }

            return logFile;
        }

        private static string GetPropertyValue(string[] elements, int v)
        {
            if(v == -1)
            {
                return "";
            }
            return elements[v];
        }

        private static Dictionary<string, int> CreatePropertyIndices(string[] elements, string v)
        {
            var headers = new Dictionary<string, int>();

            headers.Add(nameof(Timestamp), -1);
            headers.Add(nameof(Category), -1);
            headers.Add(nameof(UserId), -1);
            headers.Add(nameof(ParticipantType), -1);
            headers.Add(nameof(CameraPosition), -1);
            headers.Add(nameof(CameraRotation), -1);
            headers.Add(nameof(MarkerPointCount), -1);
            headers.Add(nameof(DistanceFromCamera), -1);
            headers.Add(nameof(DistanceToWall), -1);
            headers.Add(nameof(HitPointWallPosition), -1);
            headers.Add(nameof(HitPointWallNormal), -1);
            headers.Add(nameof(AnchorPointPosition), -1);
            headers.Add(nameof(StudyName), -1);
            headers.Add(nameof(StudyIndex), -1);
            headers.Add(nameof(NoiseLevel), -1);
            headers.Add(nameof(LoadLevel), -1);
            headers.Add(nameof(TrialCount), -1);
            headers.Add(nameof(RepetitionsPerTrial), -1);
            headers.Add(nameof(AudioTaskReactionTime), -1);
            headers.Add(nameof(TrialTargetIndex), -1);
            headers.Add(nameof(TrialSelectedIndex), -1);
            headers.Add(nameof(TrialSymbolOrder), -1);
            headers.Add(nameof(AnchorPointIndex), -1);
            headers.Add(nameof(StartTime), -1);
            headers.Add(nameof(ReactionTime), -1);
            headers.Add(nameof(LeftEyePosition), -1);
            headers.Add(nameof(RightEyePosition), -1);
            headers.Add(nameof(EyeDimensions), -1);
            headers.Add(nameof(PupilDiameter), -1);
            headers.Add(nameof(GazeBehaviour), -1);
            headers.Add(nameof(GazeBehaviourDuration), -1);
            headers.Add(nameof(VideoPath), -1);
            headers.Add(nameof(AudioPath), -1);
            headers.Add(nameof(Acceleration), -1);
            headers.Add(nameof(AngularVelocity), -1);
            headers.Add(nameof(LinearAcceleration), -1);
            headers.Add(nameof(Attitude), -1);
            headers.Add(nameof(Lux), -1);

            for (var i = 0; i < elements.Length; i++)
            {
                var currentElement = elements[i];
                if (headers.ContainsKey(currentElement))
                {
                    headers[currentElement] = i;
                }
            }

            return headers;
        }


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
            csvData.Add(nameof(_gazeBehaviour), "");
            csvData.Add(nameof(_gazeBehaviourDuration), "");
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
                    csvData[nameof(_startTime)] = logData._startTime.ToString(CultureInfo.InvariantCulture);
                    csvData[nameof(_reactionTime)] = logData._reactionTime.ToString(CultureInfo.InvariantCulture);
                    csvData[nameof(_trialCount)] = logData._trialCount.ToString();
                    csvData[nameof(_repetitionsPerTrial)] = logData._repetitionsPerTrial.ToString();
                    csvData[nameof(_anchorPointIndex)] = logData._anchorPointIndex.ToString();
                    break;
                case LogCategory.AudioTaskConfirmation:
                    csvData[nameof(_startTime)] = logData._startTime.ToString();
                    csvData[nameof(_reactionTime)] = logData._reactionTime.ToString();
                    break;
                case LogCategory.EyeTracking:
                    csvData[nameof(_cameraPosition)] = logData._cameraPosition.WriteVector3ToCSVString();
                    csvData[nameof(_cameraRotation)] = logData._cameraRotation.WriteQuaternionToCSVString();
                    csvData[nameof(_leftEyePosition)] = logData._leftEyePosition.WriteVector3ToCSVString();
                    csvData[nameof(_rightEyePosition)] = logData._rightEyePosition.WriteVector3ToCSVString();
                    csvData[nameof(_eyeDimensions)] = logData._eyeDimensions.WriteVector2ToCSVString();
                    csvData[nameof(_pupilDiameter)] = logData._pupilDiameter.WriteVector2ToCSVString();
                    csvData[nameof(_reactionTime)] = logData._reactionTime.ToString();
                    csvData[nameof(_startTime)] = logData._startTime.ToString();
                    csvData[nameof(_gazeBehaviour)] = logData._gazeBehaviour;
                    csvData[nameof(_gazeBehaviourDuration)] = logData._gazeBehaviourDuration.ToString();
                    
                    break;
                case LogCategory.FrameCapture:
                    csvData[nameof(_startTime)] = logData._audioTaskReactionTime.ToString();
                    csvData[nameof(_cameraPosition)] = logData._cameraPosition.WriteVector3ToCSVString();
                    csvData[nameof(_cameraRotation)] = logData._cameraRotation.WriteQuaternionToCSVString();
                    csvData[nameof(_videoPath)] = logData._videoPath;
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
            
            
            return $"{csvData[nameof(_timeStamp)]};{csvData[nameof(_logCategory)]};{csvData[nameof(_userId)]};{csvData[nameof(_participantType)]};{csvData[nameof(_cameraPosition)]};{csvData[nameof(_cameraRotation)]};{csvData[nameof(_markerPointCount)]};{csvData[nameof(_distanceFromCamera)]};{csvData[nameof(_distanceToWall)]};{csvData[nameof(_hitPointWallPosition)]};{csvData[nameof(_hitPointWallNormal)]};{csvData[nameof(_anchorPointPosition)]};{csvData[nameof(_studyName)]};{csvData[nameof(_studyIndex)]};{csvData[nameof(_noiseLevel)]};{csvData[nameof(_loadLevel)]};{csvData[nameof(_trialCount)]};{csvData[nameof(_repetitionsPerTrial)]};{csvData[nameof(_audioTaskReactionTime)]};{csvData[nameof(_trialTargetIndex)]};{csvData[nameof(_trialSelectedIndex)]};{csvData[nameof(_trialSymbolOrder)]};{csvData[nameof(_anchorPointIndex)]};{csvData[nameof(_startTime)]};{csvData[nameof(_reactionTime)]};{csvData[nameof(_leftEyePosition)]};{csvData[nameof(_rightEyePosition)]};{csvData[nameof(_eyeDimensions)]};{csvData[nameof(_pupilDiameter)]};{csvData[nameof(_gazeBehaviour)]};{csvData[nameof(_gazeBehaviourDuration)]};{csvData[nameof(_videoPath)]};{csvData[nameof(_audioPath)]};{csvData[nameof(_acceleration)]};{csvData[nameof(_angularVelocity)]};{csvData[nameof(_linearAcceleration)]};{csvData[nameof(_attitude)]};{csvData[nameof(_lux)]}";
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
        
        
        public static LogData CreateEyeTrackingLogData(Vector3 cameraPosition, Quaternion cameraRotation, Vector3 leftEyePosition, Vector3 rightEyePosition, Vector2 eyeDimensions, Vector2 pupilDiameter, long currentTimeStamp, string gazeBehaviour, long gazeBehaviourStartTime, ulong gazeBehaviourDuration)
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
                _pupilDiameter = pupilDiameter,
                _startTime = gazeBehaviourStartTime,
                _reactionTime = currentTimeStamp,
                _gazeBehaviour = gazeBehaviour,
                _gazeBehaviourDuration = gazeBehaviourDuration
                
            };
        }
        

        public static LogData CreateFrameCaptureLogData(long startTime, Vector3 cameraPosition, Quaternion cameraRotation, string filePath)
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.FrameCapture,
                _startTime = startTime,
                _cameraPosition = cameraPosition,
                _cameraRotation = cameraRotation,
                _videoPath = filePath
            };
        }

        public static LogData CreateVideoPlayerChangeLogData(string videoClipName, string audioClipName)
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.VideoPlayerChange,
                _videoPath = videoClipName,
                _audioPath = audioClipName
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


        public static LogData CreateTriggerPressedData()
        {
            return new LogData
            {
                _timeStamp = GetCurrentTimestamp(),
                _logCategory = LogCategory.TriggerPressed,
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
        Lux,
        TriggerPressed
    }
}