using System;
using System.Collections.Generic;
using System.Globalization;
using DistractorTask.UserStudy.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace DistractorTask.Logging
{
    [Serializable]
    public class LogData
    {
        #region ReadOnly Properties

        public long TimeStamp => timeStamp;
        public LogCategory LogCategory => logCategory;
        public string UserId1 => userId;
        public string ParticipantType1 => participantType;
        public Vector3 CameraPosition1 => cameraPosition;
        public Quaternion CameraRotation1 => cameraRotation;
        public int MarkerPointCount1 => markerPointCount;
        public float DistanceFromCamera1 => distanceFromCamera;
        public float DistanceToWall1 => distanceToWall;
        public Vector3 HitPointWallPosition1 => hitPointWallPosition;
        public Vector3 HitPointWallNormal1 => hitPointWallNormal;
        public Vector3 AnchorPointPosition1 => anchorPointPosition;
        public string StudyName1 => studyName;
        public int StudyIndex1 => studyIndex;
        public NoiseLevel NoiseLevel1 => noiseLevel;
        public LoadLevel LoadLevel1 => loadLevel;
        public int TrialCount1 => trialCount;
        public int RepetitionsPerTrial1 => repetitionsPerTrial;
        public int AudioTaskReactionTime1 => audioTaskReactionTime;
        public int TrialTargetIndex1 => trialTargetIndex;
        public int TrialSelectedIndex1 => trialSelectedIndex;
        public string TrialSymbolOrder1 => trialSymbolOrder;
        public int AnchorPointIndex1 => anchorPointIndex;
        public long StartTime1 => startTime;
        public long ReactionTime1 => reactionTime;
        public Vector3 LeftEyePosition1 => leftEyePosition;
        public Vector3 RightEyePosition1 => rightEyePosition;
        public Vector2 EyeDimensions1 => eyeDimensions;
        public Vector2 PupilDiameter1 => pupilDiameter;
        public string GazeBehaviour1 => gazeBehaviour;
        public ulong GazeBehaviourDuration1 => gazeBehaviourDuration;
        public string VideoPath1 => videoPath;
        public string AudioPath1 => audioPath;
        public Vector3 Acceleration1 => acceleration;
        public Vector3 AngularVelocity1 => angularVelocity;
        public Vector3 LinearAcceleration1 => linearAcceleration;
        public Quaternion Attitude1 => attitude;
        public float Lux1 => lux;

        #endregion

        [FormerlySerializedAs("_timeStamp")] [SerializeField]
        private long timeStamp;
        [FormerlySerializedAs("_logCategory")] [SerializeField]
        private LogCategory logCategory;
        [FormerlySerializedAs("_userId")] [SerializeField]
        private string userId;
        [FormerlySerializedAs("_participantType")] [SerializeField]
        private string participantType;
        [FormerlySerializedAs("_cameraPosition")] [SerializeField]
        private Vector3 cameraPosition;
        [FormerlySerializedAs("_cameraRotation")] [SerializeField]
        private Quaternion cameraRotation;
        [FormerlySerializedAs("_markerPointCount")] [SerializeField]
        private int markerPointCount;
        [FormerlySerializedAs("_distanceFromCamera")] [SerializeField]
        private float distanceFromCamera;
        [FormerlySerializedAs("_distanceToWall")] [SerializeField]
        private float distanceToWall;
        [FormerlySerializedAs("_hitPointWallPosition")] [SerializeField]
        private Vector3 hitPointWallPosition;
        [FormerlySerializedAs("_hitPointWallNormal")] [SerializeField]
        private Vector3 hitPointWallNormal;
        [FormerlySerializedAs("_anchorPointPosition")] [SerializeField]
        private Vector3 anchorPointPosition;
        [FormerlySerializedAs("_studyName")] [SerializeField]
        private string studyName;
        [FormerlySerializedAs("_studyIndex")] [SerializeField]
        private int studyIndex;
        [FormerlySerializedAs("_noiseLevel")] [SerializeField]
        private NoiseLevel noiseLevel;
        [FormerlySerializedAs("_loadLevel")] [SerializeField]
        private LoadLevel loadLevel;
        [FormerlySerializedAs("_trialCount")] [SerializeField]
        private int trialCount;
        [FormerlySerializedAs("_repetitionsPerTrial")] [SerializeField]
        private int repetitionsPerTrial;
        [FormerlySerializedAs("_audioTaskReactionTime")] [SerializeField]
        private int audioTaskReactionTime;
        [FormerlySerializedAs("_trialTargetIndex")] [SerializeField]
        private int trialTargetIndex;
        [FormerlySerializedAs("_trialSelectedIndex")] [SerializeField]
        private int trialSelectedIndex;
        [FormerlySerializedAs("_trialSymbolOrder")] [SerializeField]
        private string trialSymbolOrder;
        [FormerlySerializedAs("_anchorPointIndex")] [SerializeField]
        private int anchorPointIndex;
        [FormerlySerializedAs("_startTime")] [SerializeField]
        private long startTime;
        [FormerlySerializedAs("_reactionTime")] [SerializeField]
        private long reactionTime;
        [FormerlySerializedAs("_leftEyePosition")] [SerializeField]
        private Vector3 leftEyePosition;
        [FormerlySerializedAs("_rightEyePosition")] [SerializeField]
        private Vector3 rightEyePosition;
        [FormerlySerializedAs("_eyeDimensions")] [SerializeField]
        private Vector2 eyeDimensions;
        [FormerlySerializedAs("_pupilDiameter")] [SerializeField]
        private Vector2 pupilDiameter;
        [FormerlySerializedAs("_gazeBehaviour")] [SerializeField]
        private string gazeBehaviour;
        [FormerlySerializedAs("_gazeBehaviourDuration")] [SerializeField]
        private ulong gazeBehaviourDuration;
        [FormerlySerializedAs("_videoPath")] [SerializeField]
        private string videoPath;
        [FormerlySerializedAs("_audioPath")] [SerializeField]
        private string audioPath;
        [FormerlySerializedAs("_acceleration")] [SerializeField]
        private Vector3 acceleration;
        [FormerlySerializedAs("_angularVelocity")] [SerializeField]
        private Vector3 angularVelocity;
        [FormerlySerializedAs("_linearAcceleration")] [SerializeField]
        private Vector3 linearAcceleration;
        [FormerlySerializedAs("_attitude")] [SerializeField]
        private Quaternion attitude;
        [FormerlySerializedAs("_lux")] [SerializeField]
        private float lux;


        //todo we could change this back to the actual names of the headers and not the names of the properties. The original idea does not work 

         private const string Timestamp = nameof(timeStamp);
         private const string Category = nameof(logCategory);
         private const string UserId = nameof(userId);
         private const string ParticipantType = nameof(participantType);
         private const string CameraPosition = nameof(cameraPosition);
         private const string CameraRotation = nameof(cameraRotation);
         private const string MarkerPointCount = nameof(markerPointCount);
         private const string DistanceFromCamera = nameof(distanceFromCamera);
         private const string DistanceToWall = nameof(distanceToWall);
         private const string HitPointWallPosition = nameof(hitPointWallPosition);
         private const string HitPointWallNormal = nameof(hitPointWallNormal);
         private const string AnchorPointPosition = nameof(anchorPointPosition);
         private const string StudyName = nameof(studyName);
         private const string StudyIndex = nameof(studyIndex);
         private const string NoiseLevel = nameof(noiseLevel);
         private const string LoadLevel = nameof(loadLevel);
         private const string TrialCount = nameof(trialCount);
         private const string RepetitionsPerTrial = nameof(repetitionsPerTrial);
         private const string AudioTaskReactionTime = nameof(audioTaskReactionTime);
         private const string TrialTargetIndex = nameof(trialTargetIndex);
         private const string TrialSelectedIndex = nameof(trialSelectedIndex);
         private const string TrialSymbolOrder = nameof(trialSymbolOrder);
         private const string AnchorPointIndex = nameof(anchorPointIndex);
         private const string StartTime = nameof(startTime);
         private const string ReactionTime = nameof(reactionTime);
         private const string LeftEyePosition = nameof(leftEyePosition);
         private const string RightEyePosition = nameof(rightEyePosition);
         private const string EyeDimensions = nameof(eyeDimensions);
         private const string PupilDiameter = nameof(pupilDiameter);
         private const string GazeBehaviour = nameof(gazeBehaviour);
         private const string GazeBehaviourDuration = nameof(gazeBehaviourDuration);
         private const string VideoPath = nameof(videoPath);
         private const string AudioPath = nameof(audioPath);
         private const string Acceleration = nameof(acceleration);
         private const string AngularVelocity = nameof(angularVelocity);
         private const string LinearAcceleration = nameof(linearAcceleration);
         private const string Attitude = nameof(attitude);
         private const string Lux = nameof(lux);



        public static LogData LoadLogDataFromCsvLine(string header, string line)
        {
            var headerElements = header.Split(';');
            var elements = line.Split(';');

            var indices = CreatePropertyIndices(headerElements, nameof(Category));

            var logFile = new LogData();
            

            logFile.timeStamp = long.Parse(GetPropertyValue(elements, indices[nameof(Timestamp)]), CultureInfo.InvariantCulture);
            logFile.logCategory = Enum.Parse<LogCategory>(GetPropertyValue(elements, indices[nameof(Category)]));



            switch (logFile.logCategory)
            {
                case LogCategory.LogFileStart:
                    logFile.userId = GetPropertyValue(elements, indices[nameof(UserId)]);
                    break;
                case LogCategory.MarkerPointBegin:
                    logFile.markerPointCount = int.Parse(GetPropertyValue(elements, indices[nameof(MarkerPointCount)]));
                    break;
                case LogCategory.MarkerPointConfirmed:
                    logFile.cameraPosition = GetPropertyValue(elements, indices[nameof(CameraPosition)]).ReadVector3FromCSV();
                    logFile.cameraRotation = GetPropertyValue(elements, indices[nameof(CameraRotation)]).ReadQuaternionFromCSV();
                    logFile.distanceFromCamera = float.Parse(GetPropertyValue(elements, indices[nameof(DistanceFromCamera)]), CultureInfo.InvariantCulture);
                    logFile.distanceToWall = float.Parse(GetPropertyValue(elements, indices[nameof(DistanceToWall)]), CultureInfo.InvariantCulture);
                    logFile.hitPointWallPosition = GetPropertyValue(elements, indices[nameof(HitPointWallPosition)]).ReadVector3FromCSV();
                    logFile.hitPointWallNormal = GetPropertyValue(elements, indices[nameof(HitPointWallNormal)]).ReadVector3FromCSV();
                    logFile.anchorPointPosition = GetPropertyValue(elements, indices[nameof(AnchorPointPosition)]).ReadVector3FromCSV();
                    logFile.anchorPointIndex = int.Parse(GetPropertyValue(elements, indices[nameof(AnchorPointIndex)]));
                    break;
                case LogCategory.StudyBegin:
                    logFile.studyName = GetPropertyValue(elements, indices[nameof(StudyName)]);
                    logFile.studyIndex = int.Parse(GetPropertyValue(elements, indices[nameof(StudyIndex)]));
                    logFile.participantType = GetPropertyValue(elements, indices[nameof(ParticipantType)]);
                    break;
                case LogCategory.TrialBegin:
                    logFile.noiseLevel = Enum.Parse<NoiseLevel>(GetPropertyValue(elements, indices[nameof(NoiseLevel)]));
                    logFile.loadLevel = Enum.Parse<LoadLevel>(GetPropertyValue(elements, indices[nameof(LoadLevel)]));
                    logFile.trialCount = int.Parse(GetPropertyValue(elements, indices[nameof(TrialCount)]));
                    logFile.repetitionsPerTrial = int.Parse(GetPropertyValue(elements, indices[nameof(RepetitionsPerTrial)]));
                    logFile.audioTaskReactionTime = int.Parse(GetPropertyValue(elements, indices[nameof(AudioTaskReactionTime)]));
                    break;
                case LogCategory.TrialConfirmation:
                    logFile.trialTargetIndex = int.Parse(GetPropertyValue(elements, indices[nameof(TrialTargetIndex)]));
                    logFile.trialSelectedIndex = int.Parse(GetPropertyValue(elements, indices[nameof(TrialSelectedIndex)]));
                    logFile.trialSymbolOrder = GetPropertyValue(elements, indices[nameof(TrialSymbolOrder)]);
                    logFile.startTime = long.Parse(GetPropertyValue(elements, indices[nameof(StartTime)]), CultureInfo.InvariantCulture);
                    logFile.reactionTime = long.Parse(GetPropertyValue(elements, indices[nameof(ReactionTime)]), CultureInfo.InvariantCulture);
                    logFile.trialCount = int.Parse(GetPropertyValue(elements, indices[nameof(TrialCount)]));
                    logFile.repetitionsPerTrial = int.Parse(GetPropertyValue(elements, indices[nameof(RepetitionsPerTrial)]));
                    logFile.anchorPointIndex = int.Parse(GetPropertyValue(elements, indices[nameof(AnchorPointIndex)]));
                    break;
                case LogCategory.AudioTaskConfirmation:
                    logFile.startTime = long.Parse(GetPropertyValue(elements, indices[nameof(StartTime)]), CultureInfo.InvariantCulture);
                    logFile.reactionTime = long.Parse(GetPropertyValue(elements, indices[nameof(ReactionTime)]), CultureInfo.InvariantCulture);
                    break;
                case LogCategory.EyeTracking:
                    logFile.cameraPosition = GetPropertyValue(elements, indices[nameof(CameraPosition)]).ReadVector3FromCSV();
                    logFile.cameraRotation = GetPropertyValue(elements, indices[nameof(CameraRotation)]).ReadQuaternionFromCSV();
                    logFile.leftEyePosition = GetPropertyValue(elements, indices[nameof(LeftEyePosition)]).ReadVector3FromCSV();
                    logFile.rightEyePosition = GetPropertyValue(elements, indices[nameof(RightEyePosition)]).ReadVector3FromCSV();
                    logFile.eyeDimensions = GetPropertyValue(elements, indices[nameof(EyeDimensions)]).ReadVector2FromCSV();
                    logFile.pupilDiameter = GetPropertyValue(elements, indices[nameof(PupilDiameter)]).ReadVector2FromCSV();
                    logFile.reactionTime = long.Parse(GetPropertyValue(elements, indices[nameof(ReactionTime)]), CultureInfo.InvariantCulture);
                    logFile.startTime = long.Parse(GetPropertyValue(elements, indices[nameof(StartTime)]), CultureInfo.InvariantCulture);
                    logFile.gazeBehaviour = GetPropertyValue(elements, indices[nameof(GazeBehaviour)]);
                    logFile.gazeBehaviourDuration = ulong.Parse(GetPropertyValue(elements, indices[nameof(GazeBehaviourDuration)]), CultureInfo.InvariantCulture);

                    break;
                case LogCategory.FrameCapture:
                    logFile.startTime = long.Parse(GetPropertyValue(elements, indices[nameof(StartTime)]), CultureInfo.InvariantCulture);
                    logFile.cameraPosition = GetPropertyValue(elements, indices[nameof(CameraPosition)]).ReadVector3FromCSV();
                    logFile.cameraRotation = GetPropertyValue(elements, indices[nameof(CameraRotation)]).ReadQuaternionFromCSV();
                    logFile.videoPath = GetPropertyValue(elements, indices[nameof(VideoPath)]);
                    break;
                case LogCategory.VideoPlayerChange:
                    logFile.videoPath = GetPropertyValue(elements, indices[nameof(VideoPath)]);
                    logFile.audioPath = GetPropertyValue(elements, indices[nameof(AudioPath)]);
                    break;
                case LogCategory.GyroValues:
                    logFile.acceleration = GetPropertyValue(elements, indices[nameof(Acceleration)]).ReadVector3FromCSV();
                    logFile.angularVelocity = GetPropertyValue(elements, indices[nameof(AngularVelocity)]).ReadVector3FromCSV();
                    logFile.linearAcceleration = GetPropertyValue(elements, indices[nameof(LinearAcceleration)]).ReadVector3FromCSV();
                    logFile.attitude = GetPropertyValue(elements, indices[nameof(Attitude)]).ReadQuaternionFromCSV();
                    break;
                case LogCategory.Lux:
                    logFile.lux = float.Parse(GetPropertyValue(elements, indices[nameof(Lux)]), CultureInfo.InvariantCulture);
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
            
            csvData.Add(nameof(timeStamp), "");
            csvData.Add(nameof(logCategory), "");
            csvData.Add(nameof(userId), "");
            csvData.Add(nameof(participantType), "");
            csvData.Add(nameof(cameraPosition), "");
            csvData.Add(nameof(cameraRotation), "");
            csvData.Add(nameof(markerPointCount), "");
            csvData.Add(nameof(distanceFromCamera), "");
            csvData.Add(nameof(distanceToWall), "");
            csvData.Add(nameof(hitPointWallPosition), "");
            csvData.Add(nameof(hitPointWallNormal), "");
            csvData.Add(nameof(anchorPointPosition), "");
            csvData.Add(nameof(studyName), "");
            csvData.Add(nameof(studyIndex), "");
            csvData.Add(nameof(noiseLevel), "");
            csvData.Add(nameof(loadLevel), "");
            csvData.Add(nameof(trialCount), "");
            csvData.Add(nameof(repetitionsPerTrial), "");
            csvData.Add(nameof(audioTaskReactionTime), "");
            csvData.Add(nameof(trialTargetIndex), "");
            csvData.Add(nameof(trialSelectedIndex), "");
            csvData.Add(nameof(trialSymbolOrder), "");
            csvData.Add(nameof(anchorPointIndex), "");
            csvData.Add(nameof(startTime), "");
            csvData.Add(nameof(reactionTime), "");
            csvData.Add(nameof(leftEyePosition), "");
            csvData.Add(nameof(rightEyePosition), "");
            csvData.Add(nameof(eyeDimensions), "");
            csvData.Add(nameof(pupilDiameter), "");
            csvData.Add(nameof(gazeBehaviour), "");
            csvData.Add(nameof(gazeBehaviourDuration), "");
            csvData.Add(nameof(videoPath), "");
            csvData.Add(nameof(audioPath), "");
            csvData.Add(nameof(acceleration), "");
            csvData.Add(nameof(angularVelocity), "");
            csvData.Add(nameof(linearAcceleration), "");
            csvData.Add(nameof(attitude), "");
            csvData.Add(nameof(lux), "");
            
            
            return csvData;
        }

        



        public static string WriteLogData(LogData logData)
        {
            var csvData = InitializeDictionary();

            csvData[nameof(timeStamp)] = $"{logData.timeStamp.ToString()}";
            csvData[nameof(logCategory)] = logData.logCategory.ToString();
            
            
            switch (logData.logCategory)
            {
                case LogCategory.LogFileStart:
                    csvData[nameof(userId)] = logData.userId;
                    break;
                case LogCategory.MarkerPointBegin:
                    csvData[nameof(markerPointCount)] = logData.markerPointCount.ToString();
                    break;
                case LogCategory.MarkerPointConfirmed:
                    csvData[nameof(cameraPosition)] = logData.cameraPosition.WriteVector3ToCSVString();
                    csvData[nameof(cameraRotation)] = logData.cameraRotation.WriteQuaternionToCSVString();
                    csvData[nameof(distanceFromCamera)] = logData.distanceFromCamera.ToString(CultureInfo.InvariantCulture);
                    csvData[nameof(distanceToWall)] = logData.distanceToWall.ToString(CultureInfo.InvariantCulture);
                    csvData[nameof(hitPointWallPosition)] = logData.hitPointWallPosition.WriteVector3ToCSVString();
                    csvData[nameof(hitPointWallNormal)] = logData.hitPointWallNormal.WriteVector3ToCSVString();
                    csvData[nameof(anchorPointPosition)] = logData.anchorPointPosition.WriteVector3ToCSVString();
                    csvData[nameof(anchorPointIndex)] = logData.anchorPointIndex.ToString();
                    break;
                case LogCategory.StudyBegin:
                    csvData[nameof(studyName)] = logData.studyName;
                    csvData[nameof(studyIndex)] = logData.studyIndex.ToString();
                    csvData[nameof(participantType)] = logData.participantType;
                    break;
                case LogCategory.TrialBegin:
                    csvData[nameof(noiseLevel)] = logData.noiseLevel.ToString();
                    csvData[nameof(loadLevel)] = logData.loadLevel.ToString();
                    csvData[nameof(trialCount)] = logData.trialCount.ToString();
                    csvData[nameof(repetitionsPerTrial)] = logData.repetitionsPerTrial.ToString();
                    csvData[nameof(audioTaskReactionTime)] = logData.audioTaskReactionTime.ToString();
                    break;
                case LogCategory.TrialConfirmation:
                    csvData[nameof(trialTargetIndex)] = logData.trialTargetIndex.ToString();
                    csvData[nameof(trialSelectedIndex)] = logData.trialSelectedIndex.ToString();
                    csvData[nameof(trialSymbolOrder)] = logData.trialSymbolOrder.ToString();
                    csvData[nameof(startTime)] = logData.startTime.ToString(CultureInfo.InvariantCulture);
                    csvData[nameof(reactionTime)] = logData.reactionTime.ToString(CultureInfo.InvariantCulture);
                    csvData[nameof(trialCount)] = logData.trialCount.ToString();
                    csvData[nameof(repetitionsPerTrial)] = logData.repetitionsPerTrial.ToString();
                    csvData[nameof(anchorPointIndex)] = logData.anchorPointIndex.ToString();
                    break;
                case LogCategory.AudioTaskConfirmation:
                    csvData[nameof(startTime)] = logData.startTime.ToString();
                    csvData[nameof(reactionTime)] = logData.reactionTime.ToString();
                    break;
                case LogCategory.EyeTracking:
                    csvData[nameof(cameraPosition)] = logData.cameraPosition.WriteVector3ToCSVString();
                    csvData[nameof(cameraRotation)] = logData.cameraRotation.WriteQuaternionToCSVString();
                    csvData[nameof(leftEyePosition)] = logData.leftEyePosition.WriteVector3ToCSVString();
                    csvData[nameof(rightEyePosition)] = logData.rightEyePosition.WriteVector3ToCSVString();
                    csvData[nameof(eyeDimensions)] = logData.eyeDimensions.WriteVector2ToCSVString();
                    csvData[nameof(pupilDiameter)] = logData.pupilDiameter.WriteVector2ToCSVString();
                    csvData[nameof(reactionTime)] = logData.reactionTime.ToString();
                    csvData[nameof(startTime)] = logData.startTime.ToString();
                    csvData[nameof(gazeBehaviour)] = logData.gazeBehaviour;
                    csvData[nameof(gazeBehaviourDuration)] = logData.gazeBehaviourDuration.ToString();
                    
                    break;
                case LogCategory.FrameCapture:
                    csvData[nameof(startTime)] = logData.audioTaskReactionTime.ToString();
                    csvData[nameof(cameraPosition)] = logData.cameraPosition.WriteVector3ToCSVString();
                    csvData[nameof(cameraRotation)] = logData.cameraRotation.WriteQuaternionToCSVString();
                    csvData[nameof(videoPath)] = logData.videoPath;
                    break;
                case LogCategory.VideoPlayerChange:
                    csvData[nameof(videoPath)] = logData.videoPath;
                    csvData[nameof(audioPath)] = logData.audioPath;
                    break;
                case LogCategory.GyroValues:
                    csvData[nameof(acceleration)] = logData.acceleration.WriteVector3ToCSVString();
                    csvData[nameof(angularVelocity)] = logData.angularVelocity.WriteVector3ToCSVString();
                    csvData[nameof(linearAcceleration)] = logData.linearAcceleration.WriteVector3ToCSVString();
                    csvData[nameof(attitude)] = logData.attitude.WriteQuaternionToCSVString();
                    break;
                case LogCategory.Lux:
                    csvData[nameof(lux)] = logData.lux.ToString(CultureInfo.InvariantCulture);
                    break;
            }
            
            
            return $"{csvData[nameof(timeStamp)]};{csvData[nameof(logCategory)]};{csvData[nameof(userId)]};{csvData[nameof(participantType)]};{csvData[nameof(cameraPosition)]};{csvData[nameof(cameraRotation)]};{csvData[nameof(markerPointCount)]};{csvData[nameof(distanceFromCamera)]};{csvData[nameof(distanceToWall)]};{csvData[nameof(hitPointWallPosition)]};{csvData[nameof(hitPointWallNormal)]};{csvData[nameof(anchorPointPosition)]};{csvData[nameof(studyName)]};{csvData[nameof(studyIndex)]};{csvData[nameof(noiseLevel)]};{csvData[nameof(loadLevel)]};{csvData[nameof(trialCount)]};{csvData[nameof(repetitionsPerTrial)]};{csvData[nameof(audioTaskReactionTime)]};{csvData[nameof(trialTargetIndex)]};{csvData[nameof(trialSelectedIndex)]};{csvData[nameof(trialSymbolOrder)]};{csvData[nameof(anchorPointIndex)]};{csvData[nameof(startTime)]};{csvData[nameof(reactionTime)]};{csvData[nameof(leftEyePosition)]};{csvData[nameof(rightEyePosition)]};{csvData[nameof(eyeDimensions)]};{csvData[nameof(pupilDiameter)]};{csvData[nameof(gazeBehaviour)]};{csvData[nameof(gazeBehaviourDuration)]};{csvData[nameof(videoPath)]};{csvData[nameof(audioPath)]};{csvData[nameof(acceleration)]};{csvData[nameof(angularVelocity)]};{csvData[nameof(linearAcceleration)]};{csvData[nameof(attitude)]};{csvData[nameof(lux)]}";
        }
        
        

        public static LogData CreateLogFileStartLogData(string userId)
        {
            var result = new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.LogFileStart,
                userId = userId,
                
            };
            return result;

        }

        public static LogData CreateMarkerPointBeginLogData(int markerPointCount)
        {
            var result = new LogData()
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.MarkerPointBegin,
                markerPointCount = markerPointCount
            };
            return result;
        }

        public static LogData CreateMarkerPointConfirmedLogData(Vector3 cameraPosition, Quaternion cameraRotation,
            float distanceFromCamera, float distanceToWall, Vector3 hitPointWall, Vector3 hitPointWallNormal,
            Vector3 anchorPointPosition, int anchorPointIndex)
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.MarkerPointConfirmed,
                cameraPosition = cameraPosition,
                cameraRotation = cameraRotation,
                distanceFromCamera = distanceFromCamera,
                distanceToWall = distanceToWall,
                hitPointWallPosition = hitPointWall,
                hitPointWallNormal = hitPointWallNormal,
                anchorPointPosition = anchorPointPosition,
                anchorPointIndex = anchorPointIndex
            };
        }

        public static LogData CreateMarkerPointEndLogData()
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.MarkerPointEnd
            };
        }

        public static LogData CreateStudyBeginLogData(string studyName, int studyIndex, string participantType)
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.StudyBegin,
                studyName = studyName,
                studyIndex = studyIndex,
                participantType = participantType
            };
        }

        public static LogData CreateMarkerPointActivatedLogData(int activeMarkerPointIndex)
        {
            var result = new LogData()
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.MarkerPointBegin,
                markerPointCount = activeMarkerPointIndex
            };
            return result;
        }

        public static LogData CreateTrialBeginLogData(NoiseLevel noiseLevel, LoadLevel loadLevel, int trialCount,
            int repetitionsPerTrial, int audioTaskReactionTime)
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.TrialBegin,
                noiseLevel = noiseLevel,
                loadLevel = loadLevel,
                trialCount = trialCount,
                repetitionsPerTrial = repetitionsPerTrial,
                audioTaskReactionTime = audioTaskReactionTime
            };
        }

        public static LogData CreateTrialConfirmationLogData(int trialTargetIndex, int trialSelectedIndex,
            string trialSymbolOrder, long startTime, long reactionTime, int currentTrial, int currentRepetition,
            int anchorPointIndex)
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.TrialConfirmation,
                trialTargetIndex = trialTargetIndex,
                trialSelectedIndex = trialSelectedIndex,
                trialSymbolOrder = trialSymbolOrder,
                startTime = startTime,
                reactionTime = reactionTime,
                trialCount = currentTrial,
                repetitionsPerTrial = currentRepetition,
                anchorPointIndex = anchorPointIndex
            };
        }

        public static LogData CreateTrialEndLogData()
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.TrialEnd
            };
        }

        public static LogData CreateStudyEndLogData()
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.StudyEnd
            };
        }

        public static LogData CreateAudioTaskConfirmationLogData(long startTime, long reactionTime)
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.AudioTaskConfirmation,
                startTime = startTime,
                reactionTime = reactionTime
            };
        }
        
        
        public static LogData CreateEyeTrackingLogData(Vector3 cameraPosition, Quaternion cameraRotation, Vector3 leftEyePosition, Vector3 rightEyePosition, Vector2 eyeDimensions, Vector2 pupilDiameter, long currentTimeStamp, string gazeBehaviour, long gazeBehaviourStartTime, ulong gazeBehaviourDuration)
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.EyeTracking,
                cameraPosition = cameraPosition,
                cameraRotation = cameraRotation,
                leftEyePosition = leftEyePosition,
                rightEyePosition = rightEyePosition,
                eyeDimensions = eyeDimensions,
                pupilDiameter = pupilDiameter,
                startTime = gazeBehaviourStartTime,
                reactionTime = currentTimeStamp,
                gazeBehaviour = gazeBehaviour,
                gazeBehaviourDuration = gazeBehaviourDuration
                
            };
        }
        

        public static LogData CreateFrameCaptureLogData(long startTime, Vector3 cameraPosition, Quaternion cameraRotation, string filePath)
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.FrameCapture,
                startTime = startTime,
                cameraPosition = cameraPosition,
                cameraRotation = cameraRotation,
                videoPath = filePath
            };
        }

        public static LogData CreateVideoPlayerChangeLogData(string videoClipName, string audioClipName)
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.VideoPlayerChange,
                videoPath = videoClipName,
                audioPath = audioClipName
            };
        }

        public static LogData CreateGyroValuesLogData(Vector3 acceleration, Vector3 angularVelocity,
            Vector3 linearAcceleration, Quaternion attitude)
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.GyroValues,
                acceleration = acceleration,
                angularVelocity = angularVelocity,
                linearAcceleration = linearAcceleration,
                attitude = attitude
            };
        }

        public static LogData CreateLuxLogData(float luxValue)
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.Lux,
                lux = luxValue
            };
        }


        public static LogData CreateTriggerPressedData()
        {
            return new LogData
            {
                timeStamp = GetCurrentTimestamp(),
                logCategory = LogCategory.TriggerPressed,
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