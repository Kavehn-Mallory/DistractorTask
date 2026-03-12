using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Codice.Client.Selector;
using DistractorTask.Logging;
using DistractorTask.UserStudy.Core;
using JetBrains.Annotations;
using MagicLeap.OpenXR.Features.EyeTracker;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using UnityEngine.XR.MagicLeap;
using UnityEngine.XR.OpenXR.Features.MagicLeapSupport.NativeInterop;

namespace DistractorTask.Editor.UI
{
    public class NormalizedCsvGenerator
    {

        public const string StudyTutorial1FileName = "TutorialStudy1Values.csv";
        public const string Study1FileName = "Study1Values.csv";
        public const string Study13FileName = "Study1_3Values.csv";
        
        public const string Study2FileName = "Study2Values.csv";
        
        public const string StudyTutorial3FileName = "TutorialStudy3Values.csv";
        public const string Study3FileName = "Study3Values.csv";

        public const string AudioTaskResultFileName = "AudioTaskResult.csv";

        public const string EyeTrackingDataFileName = "EyeTrackingData.csv";
        
        public const string EyeTrackingSwitchDataFileName = "EyeTrackingSwitchData.csv";
        
        public const string EyeTrackingPupilDataFileName = "EyeTrackingPupilData.csv";

        public const string ErrorRateDataFileName = "ErrorRateData.csv";
        public const string TaskPerformanceOverTimeFileName = "TaskPerformanceOverTime.csv";

        public const string Header =
            "Time;Timestamp;UserId;StudyIndex;NoiseLevel;LoadLevel;TrialCount;RepetitionsPerTrial;TrialTargetIndex;TrialSelectedIndex;TrialSymbolOrder;AnchorPointIndex;StartTime;ReactionTime;HasAudioTask";

        public const string AudioTaskHeader = "Time;UserId;NoiseLevel;LoadLevel;ReactionTime";
        
        

        public const string EyeTrackingDataHeader = "UserId;StudyIndex;NoiseLevel;LoadLevel;GazeBehaviour;Duration;NormalizedDuration;HasAudioTask";
        
        public const string EyeTrackingSwitchDataHeader = "UserId;StudyIndex;NoiseLevel;LoadLevel;GazeBehaviourBefore;GazeBehaviourAfter;Count";

        public const string EyeTrackingPupilDataHeader =
            "UserId;StudyIndex;TrialIndex;Region;NoiseLevel;LoadLevel;PupilDiameterLeft;PupilDiameterRight;LuxValue;AdjustedPupilDiameterLeft;AdjustedPupilDiameterRight";
        
        public const string NoAudioResponseValue = "None";

        public const string TaskPerformanceOverTimeHeader =
            "UserId;StudyIndex;NoiseLevel;LoadLevel;TimeInStudy;TimeInCondition;ReactionTime;NormalizedTimeInStudy;NormalizedTimeInCondition";

        public const string ErrorRateHeader = "UserId;StudyIndex;NoiseLevel;LoadLevel;Response;ResponsePercentage";
        
        public const string HeaderStudyTrialCondition = "Time;Category;CameraPosition;CameraRotation;DistanceFromCamera;DistanceToWall;HitPointWallPosition;HitPointWallNormal;AnchorPointPosition;AudioTaskReactionTime;TrialTargetIndex;TrialSelectedIndex;TrialSymbolOrder;AnchorPointIndex;ReactionTime;LeftEyePosition;RightEyePosition;EyeDimensions;PupilDiameter;GazeBehaviour;GazeBehaviourStartTime;GazeBehaviourDuration;Acceleration;AngularVelocity;LinearAcceleration;Attitude;Lux";


        public static readonly int[] TutorialStudyIndices = new[] { 0, 4 };

        private static string GetPythonConformNoiseLevel(NoiseLevel noiseLevel)
        {
            if(noiseLevel == NoiseLevel.None)
            {
                return "No Noise";
            }

            return $"{noiseLevel}";
        }


        public static void GeneratePerStudyConditionCSVFiles(string[] paths)
        {
            var path = EditorUtility.OpenFolderPanel("Select target folder for generated logfiles", Application.persistentDataPath,
                "");
            
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            

            foreach (var filePath in paths)
            {
                
                using StreamReader reader = new StreamReader(filePath);
                int studyIndex = -1;
                
                var userId = "";
                bool insideTask = false;

                StreamWriter streamWriter = null;
                
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    var parts = line.Split(';');

                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.LogFileStart))
                    {
                        userId = parts[(int)(LogFileHeaders.UserId)];
                        continue;
                    }
                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.StudyBegin))
                    {
                        studyIndex = int.Parse(parts[(int)LogFileHeaders.StudyIndex]);

                        if (streamWriter != null)
                        {
                            streamWriter.Flush();
                            streamWriter.Dispose();
                            streamWriter = null;
                        }
                        continue;
                    }
                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialBegin))
                    {
                        var loadLevel = Enum.Parse<LoadLevel>(parts[(int)LogFileHeaders.LoadLevel]);
                        var noiseLevel = Enum.Parse<NoiseLevel>(parts[(int)LogFileHeaders.NoiseLevel]);
                        insideTask = true;
                        if (!TutorialStudyIndices.Contains(studyIndex))
                        {
                            streamWriter = CreateStreamWriter(path,$"{userId}_{studyIndex}_{loadLevel.ToString()}_{GetPythonConformNoiseLevel(noiseLevel)}");
                        
                            streamWriter.WriteLine(HeaderStudyTrialCondition);
                        }
                        continue;
                        
                    }
                    
                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialEnd))
                    {
                        if (streamWriter != null)
                        {
                            streamWriter.Flush();
                            streamWriter.Dispose();
                            streamWriter = null;
                        }
                        insideTask = false;
                        continue;
                    }
                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.StudyEnd))
                    {
                        if (streamWriter != null)
                        {
                            streamWriter.Flush();
                            streamWriter.Dispose();
                            streamWriter = null;
                        }
                        insideTask = false;
                        continue;
                    }
                    
                    if (insideTask && streamWriter != null)
                    {
                        var startTime = parts[(int)LogFileHeaders.StartTime];
                        var endTime = parts[(int)LogFileHeaders.ReactionTime];
                        string reactionTime = startTime;
                        string gazeBehaviourStartTime = startTime;
                        if (parts[(int)LogFileHeaders.Category] != nameof(LogCategory.EyeTracking) && !string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
                        {
                            reactionTime = TimeSpan.FromTicks((long.Parse(parts[(int)LogFileHeaders.ReactionTime]) -
                                                                   long.Parse(parts[(int)LogFileHeaders.StartTime]))).TotalMilliseconds.ToString();
                            gazeBehaviourStartTime = "";
                        }
                        var l = $"{parts[(int)LogFileHeaders.Time]};{parts[(int)LogFileHeaders.Category]};{parts[(int)LogFileHeaders.CameraPosition]};{parts[(int)LogFileHeaders.CameraRotation]};{parts[(int)LogFileHeaders.DistanceFromCamera]};{parts[(int)LogFileHeaders.DistanceToWall]};{parts[(int)LogFileHeaders.HitPointWallPosition]};{parts[(int)LogFileHeaders.HitPointWallNormal]};{parts[(int)LogFileHeaders.AnchorPointPosition]};{parts[(int)LogFileHeaders.AudioTaskReactionTime]};{parts[(int)LogFileHeaders.TrialTargetIndex]};{parts[(int)LogFileHeaders.TrialSelectedIndex]};{parts[(int)LogFileHeaders.TrialSymbolOrder]};{parts[(int)LogFileHeaders.AnchorPointIndex]};{reactionTime};{parts[(int)LogFileHeaders.LeftEyePosition]};{parts[(int)LogFileHeaders.RightEyePosition]};{parts[(int)LogFileHeaders.EyeDimensions]};{parts[(int)LogFileHeaders.PupilDiameter]};{parts[(int)LogFileHeaders.GazeBehaviour]};{gazeBehaviourStartTime};{parts[(int)LogFileHeaders.GazeBehaviourDuration]};{parts[(int)LogFileHeaders.Acceleration]};{parts[(int)LogFileHeaders.AngularVelocity]};{parts[(int)LogFileHeaders.LinearAcceleration]};{parts[(int)LogFileHeaders.Attitude]};{parts[(int)LogFileHeaders.Lux]}";

                        streamWriter.WriteLine(l);
                    }
                    
                    
                }
                

            }

            
        }


        private static StreamWriter CreateStreamWriter(string basePath, string fileName)
        {
            if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(
                    $"The given path or file name are empty. Path: {basePath} and FileName: {fileName}");
            }

            if (!fileName.EndsWith(".csv"))
            {
                fileName += ".csv";
            }
            return new StreamWriter(basePath + "/" + fileName);
        }

        public static void GenerateErrorRateCSVFiles(string[] paths)
        {
            var path = EditorUtility.OpenFolderPanel("Select folder that contains the logfiles", Application.persistentDataPath,
                "");
            
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var errorRateStreamWriter = CreateStreamWriter(path, ErrorRateDataFileName);
            
            errorRateStreamWriter.WriteLine(ErrorRateHeader);

            var errorRates = new List<ErrorRateData>();


            foreach (var filePath in paths)
            {
                using StreamReader reader = new StreamReader(filePath);
                int studyIndex = -1;
                
                var userId = "";
                NoiseLevel noiseLevel = NoiseLevel.None;
                LoadLevel loadLevel = LoadLevel.Low;
                bool insideTask = false;


                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    var parts = line.Split(';');

                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.LogFileStart))
                    {
                        userId = parts[(int)(LogFileHeaders.UserId)];
                    }
                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.StudyBegin))
                    {
                        studyIndex = int.Parse(parts[(int)LogFileHeaders.StudyIndex]);
                    }
                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialBegin))
                    {
                        loadLevel = Enum.Parse<LoadLevel>(parts[(int)LogFileHeaders.LoadLevel]);
                        noiseLevel = Enum.Parse<NoiseLevel>(parts[(int)LogFileHeaders.NoiseLevel]);
                        insideTask = true;

                        var errorRateItem = new ErrorRateData
                        {
                            CorrectlySelected = 0,
                            IncorrectlySelected = 0,
                            NothingSelected = 0,
                            NoiseLevel = noiseLevel,
                            LoadLevel = loadLevel
                        };

                        if (!errorRates.Contains(errorRateItem))
                        {
                            errorRates.Add(errorRateItem);
                        }
                    }
                    

                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialConfirmation))
                    {
                        if (!insideTask)
                        {
                            Debug.LogWarning($"File {filePath} has TrialConfirmation-Data without TrialStart in Study {studyIndex}");
                        }

                        var selectedIndex = int.Parse(parts[(int)LogFileHeaders.TrialSelectedIndex]);
                        var targetIndex = int.Parse(parts[(int)LogFileHeaders.TrialTargetIndex]);


                        var index = errorRates.FindIndex(e => e.LoadLevel == loadLevel && e.NoiseLevel == noiseLevel);
                        var errorRate = errorRates[index];

                        if (selectedIndex == -1)
                        {
                            errorRate.NothingSelected++;
                        }
                        else if (selectedIndex == targetIndex)
                        {
                            errorRate.CorrectlySelected++;
                        }
                        else
                        {
                            errorRate.IncorrectlySelected++;
                        }

                        errorRates[index] = errorRate;
                        
                    }
                    

                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialEnd))
                    {
                        insideTask = false;
                    }
                    
                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.StudyEnd))
                    {
                        if (!TutorialStudyIndices.Contains(studyIndex))
                        {
                            foreach (var errorRateData in errorRates)
                            {
                                var total = errorRateData.Total;
                                if (total == 0)
                                {
                                    continue;
                                }
                                var correctlySelectedPercent = ((float)errorRateData.CorrectlySelected) / total;
                                var incorrectlySelectedPercent = ((float)errorRateData.IncorrectlySelected) / total;
                                var nothingSelectedPercent = ((float)errorRateData.NothingSelected) / total;
                            
                                Assert.AreApproximatelyEqual(1f, correctlySelectedPercent + incorrectlySelectedPercent + nothingSelectedPercent);
                                errorRateStreamWriter.WriteLine($"{userId};{studyIndex};{GetPythonConformNoiseLevel(noiseLevel)};{errorRateData.LoadLevel};Success;{correctlySelectedPercent}");
                                errorRateStreamWriter.WriteLine($"{userId};{studyIndex};{GetPythonConformNoiseLevel(noiseLevel)};{errorRateData.LoadLevel};Failure;{incorrectlySelectedPercent}");
                                errorRateStreamWriter.WriteLine($"{userId};{studyIndex};{GetPythonConformNoiseLevel(noiseLevel)};{errorRateData.LoadLevel};Missed;{nothingSelectedPercent}");
                            }
                        }
                        errorRates.Clear();
                        
                    }
                    
                }
                
                
                

            }


            errorRateStreamWriter.Dispose();
        }

        public static void GenerateTaskPerformanceOverTimeCSVFile(string[] paths)
        {
            var path = EditorUtility.OpenFolderPanel("Select target folder for generated logfiles", Application.persistentDataPath,
                "");
            
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var taskPerformanceStreamWriter = CreateStreamWriter(path, TaskPerformanceOverTimeFileName);
            
            taskPerformanceStreamWriter.WriteLine(TaskPerformanceOverTimeHeader);


            foreach (var filePath in paths)
            {
                using StreamReader reader = new StreamReader(filePath);
                int studyIndex = -1;

                var userId = "";
                bool insideTask = false;
                var taskPerformanceStorage = new List<TaskPerformanceTrialStorage>();
                double2 minMaxInStudyTime = new double2(double.MaxValue, 0);

                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    var parts = line.Split(';');

                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.LogFileStart))
                    {
                        userId = parts[(int)(LogFileHeaders.UserId)];
                    }
                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.StudyBegin))
                    {
                        studyIndex = int.Parse(parts[(int)LogFileHeaders.StudyIndex]);
                        var studyStartTime = long.Parse(parts[(int)LogFileHeaders.Timestamp]);
                        minMaxInStudyTime.x = TimeSpan.FromTicks(studyStartTime).TotalMilliseconds;
                    }
                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.StudyEnd))
                    {
                        var studyEndTime = long.Parse(parts[(int)LogFileHeaders.Timestamp]);
                        minMaxInStudyTime.y = TimeSpan.FromTicks(studyEndTime).TotalMilliseconds;
                        
                        foreach (var taskPerformance in taskPerformanceStorage)
                        {
                            foreach (var trialTime in taskPerformance.TrialTimes)
                            {
                                var normalizedTimeInTrial =
                                    trialTime.TrialTiming.Normalize(taskPerformance.MinMaxTime.x, taskPerformance.MinMaxTime.y);
                                var normalizedTimeInStudy =
                                    trialTime.TrialTiming.Normalize(minMaxInStudyTime.x, minMaxInStudyTime.y);
                                
                                //we are recalculating the correct timings by using the start times and subtracting those from the timestamps 
                                var l = $"{userId};{studyIndex};{GetPythonConformNoiseLevel(taskPerformance.NoiseLevel)};{taskPerformance.LoadLevel};{trialTime.TrialTiming - minMaxInStudyTime.x};{trialTime.TrialTiming - taskPerformance.MinMaxTime.x};{trialTime.ReactionTime};{normalizedTimeInStudy};{normalizedTimeInTrial}";
                                taskPerformanceStreamWriter.WriteLine(l);
                            }
                        }
                        
                        taskPerformanceStorage.Clear();
                    }
                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialBegin))
                    {

                        var trialStartTime = long.Parse(parts[(int)LogFileHeaders.Timestamp]);
                        if (taskPerformanceStorage.Count > 0 && insideTask)
                        {
                            //if we are still in the task, we did not have an end trial 
                            Debug.LogWarning("Found trial without TrialEnd-Event");
                            var test = taskPerformanceStorage[^1];
                            test.MinMaxTime.y = TimeSpan.FromTicks(trialStartTime).TotalMilliseconds;
                            taskPerformanceStorage[^1] = test;
                        }
                        
                        var loadLevel = Enum.Parse<LoadLevel>(parts[(int)LogFileHeaders.LoadLevel]);
                        var noiseLevel = Enum.Parse<NoiseLevel>(parts[(int)LogFileHeaders.NoiseLevel]);
                        insideTask = true;
                        
                        taskPerformanceStorage.Add(new TaskPerformanceTrialStorage
                        {
                            LoadLevel = loadLevel,
                            TrialTimes = new List<TrialTime>(),
                            NoiseLevel = noiseLevel,
                            MinMaxTime = new double2(TimeSpan.FromTicks(trialStartTime).TotalMilliseconds, 0)
                        });

                    }
                    

                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialConfirmation))
                    {
                        if (!insideTask)
                        {
                            Debug.LogWarning($"File {filePath} has TrialConfirmation-Data without TrialStart in Study {studyIndex}");
                        }

                        if (!TutorialStudyIndices.Contains(studyIndex))
                        {
                            var trialTime = long.Parse(parts[(int)LogFileHeaders.Timestamp]);
                            
                            var reactionTime = TimeSpan.FromTicks((long.Parse(parts[(int)LogFileHeaders.ReactionTime]) -
                                                                   long.Parse(parts[(int)LogFileHeaders.StartTime]))).TotalMilliseconds;
                        
                            //"UserId;StudyIndex;NoiseLevel;LoadLevel;TimeInStudy;TimeInCondition;ReactionTime";
                            /*var l = $"{userId};{studyIndex};{GetPythonConformNoiseLevel(noiseLevel)};{loadLevel};{lengthInStudyTime};{lengthInTrialTime};{reactionTime}";
                            taskPerformanceStreamWriter.WriteLine(l);*/
                            var taskPerformance = taskPerformanceStorage[^1];
                            taskPerformance.TrialTimes.Add(new TrialTime
                            {
                                TrialTiming = TimeSpan.FromTicks(trialTime).TotalMilliseconds,
                                ReactionTime = reactionTime
                            });
                            taskPerformanceStorage[^1] = taskPerformance;
                        }

                        
                    }
                    

                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialEnd))
                    {
                        insideTask = false;
                        var trialTime = long.Parse(parts[(int)LogFileHeaders.Timestamp]);
                        var test = taskPerformanceStorage[^1];
                        test.MinMaxTime.y = TimeSpan.FromTicks(trialTime).TotalMilliseconds;
                        taskPerformanceStorage[^1] = test;
                    }
                    
                }


            }



            taskPerformanceStreamWriter.Dispose();
        }
        
        public static void GenerateCSVFilesForPython(string[] paths, float maxReactionTimeInMilliseconds = 2000f)
        {
            var path = EditorUtility.OpenFolderPanel("Select folder that contains the logfiles", Application.persistentDataPath,
                "");
            
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var streamWriters = new StreamWriter[6];
            streamWriters[0] = new StreamWriter(path + "/" + StudyTutorial1FileName);
            streamWriters[1] = new StreamWriter(path + "/" + Study1FileName);
            streamWriters[2] = new StreamWriter(path + "/" + Study13FileName);
            
            streamWriters[3] = new StreamWriter(path + "/" + Study2FileName);
            
            streamWriters[4] = new StreamWriter(path + "/" + StudyTutorial3FileName);
            streamWriters[5] = new StreamWriter(path + "/" + Study3FileName);

            var audioTaskStreamWriter = new StreamWriter(path + "/" + AudioTaskResultFileName);

            var eyetrackingDataStreamWriter = new StreamWriter(path + "/" + EyeTrackingDataFileName);
            var eyetrackingSwitchDataStreamWriter = new StreamWriter(path + "/" + EyeTrackingSwitchDataFileName);
            var pupilDiameterDataStreamWriter = new StreamWriter(path + "/" + EyeTrackingPupilDataFileName);

            foreach (var streamWriter in streamWriters)
            {
                streamWriter.WriteLine(Header);
            }
            
            audioTaskStreamWriter.WriteLine(AudioTaskHeader);
            eyetrackingDataStreamWriter.WriteLine(EyeTrackingDataHeader);
            eyetrackingSwitchDataStreamWriter.WriteLine(EyeTrackingSwitchDataHeader);
            pupilDiameterDataStreamWriter.WriteLine(EyeTrackingPupilDataHeader);

            Dictionary<EyeTrackingEvent, int> eyeTrackingSwitchEvents = new Dictionary<EyeTrackingEvent, int>();
            
            eyeTrackingSwitchEvents.InitializeEyetrackingSwitchData();

            List<PupilDataStorage> pupilDataStorage = new List<PupilDataStorage>();
            
            Dictionary<string, int> gazeBehaviourDurations = new Dictionary<string, int>
            {
                { nameof(GazeBehaviorType.EyesClosed), 0 },
                { nameof(GazeBehaviorType.Blink), 0 },
                { nameof(GazeBehaviorType.BlinkLeft), 0 },
                { nameof(GazeBehaviorType.BlinkRight), 0 },
                { nameof(GazeBehaviorType.Fixation), 0 },
                { nameof(GazeBehaviorType.Pursuit), 0 },
                { nameof(GazeBehaviorType.Saccade), 0 }
            };

            foreach (var filePath in paths)
            {
                using StreamReader reader = new StreamReader(filePath);
                int studyIndex = -1;
                var luxValue = 0f;
                var invalidLux = 0;
                var userId = "";
                var trialCounter = 0;
                NoiseLevel noiseLevel = NoiseLevel.None;
                LoadLevel loadLevel = LoadLevel.Low;
                int hasAudioTask = 0;
                bool insideTask = false;
                long lastStartTimeInTicks = 0;
                int identicalStartTimeCounter = 0;
                int identicalStartTimeWithDifferentDurationsCounter = 0;
                string lastGazeBehaviour = "";
                ulong lastDuration = 0;
                pupilDataStorage.Clear();
                
                gazeBehaviourDurations.ResetEyetrackingData();
                eyeTrackingSwitchEvents.ResetEyetrackingSwitchData();
                

                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    var parts = line.Split(';');

                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.LogFileStart))
                    {
                        userId = parts[(int)(LogFileHeaders.UserId)];
                    }
                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.StudyBegin))
                    {
                        studyIndex = int.Parse(parts[(int)LogFileHeaders.StudyIndex]);
                    }
                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialBegin))
                    {
                        loadLevel = Enum.Parse<LoadLevel>(parts[(int)LogFileHeaders.LoadLevel]);
                        noiseLevel = Enum.Parse<NoiseLevel>(parts[(int)LogFileHeaders.NoiseLevel]);
                        insideTask = true;
                        hasAudioTask = (int.Parse(parts[(int)LogFileHeaders.AudioTaskReactionTime])) == 2 ? 1 : 0;
                    }

                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.Lux))
                    {
                        if (!float.TryParse(parts[(int)LogFileHeaders.Lux], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out luxValue))
                        {
                            invalidLux++;
                        }
                        //luxValue = float.Parse(parts[(int)LogFileHeaders.Lux], CultureInfo.InvariantCulture);
                    }

                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialConfirmation))
                    {
                        if (!insideTask)
                        {
                            Debug.LogWarning($"File {filePath} has TrialConfirmation-Data without TrialStart in Study {studyIndex}");
                        }

                        var region = parts[(int)LogFileHeaders.AnchorPointIndex];
                        
                        foreach (var pupilData in pupilDataStorage)
                        {
                            var adjustedDiameter = pupilData.PupilDiameter * pupilData.LuxValue;
                            pupilDiameterDataStreamWriter.WriteLine($"{userId};{studyIndex};{trialCounter};{region};{GetPythonConformNoiseLevel(noiseLevel)};{loadLevel};{pupilData.PupilDiameter.x.ToString(CultureInfo.InvariantCulture)};{pupilData.PupilDiameter.y.ToString(CultureInfo.InvariantCulture)};{luxValue.ToString(CultureInfo.InvariantCulture)};{adjustedDiameter.x.ToString(CultureInfo.InvariantCulture)};{adjustedDiameter.y.ToString(CultureInfo.InvariantCulture)}");

                        }
                        pupilDataStorage.Clear();
                        trialCounter++;
                        
                        var l = $"{parts[(int)LogFileHeaders.Time]};{parts[(int)LogFileHeaders.Timestamp]};{userId};{studyIndex};{GetPythonConformNoiseLevel(noiseLevel)};{loadLevel};{parts[(int)LogFileHeaders.TrialCount]};{parts[(int)LogFileHeaders.RepetitionsPerTrial]};{parts[(int)LogFileHeaders.TrialTargetIndex]};{parts[(int)LogFileHeaders.TrialSelectedIndex]};{parts[(int)LogFileHeaders.TrialSymbolOrder]};{parts[(int)LogFileHeaders.AnchorPointIndex]};{parts[(int)LogFileHeaders.StartTime]};{parts[(int)LogFileHeaders.ReactionTime]};{hasAudioTask}";
                        streamWriters[studyIndex].WriteLine(l);
                        
                    }

                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.AudioTaskConfirmation))
                    {
                        
                        var start = new TimeSpan(long.Parse(parts[(int)LogFileHeaders.StartTime]));
                        var reactionTime = (new TimeSpan(long.Parse(parts[(int)LogFileHeaders.ReactionTime])) - start).Ticks / TimeSpan.TicksPerMillisecond;
                        var reactionTimeValue = reactionTime > maxReactionTimeInMilliseconds
                            ? NoAudioResponseValue
                            : reactionTime.ToString();
                        
                        audioTaskStreamWriter.WriteLine($"{parts[(int)LogFileHeaders.Time]};{userId};{GetPythonConformNoiseLevel(noiseLevel)};{loadLevel};{reactionTimeValue}");
                    }

                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialEnd))
                    {
                        insideTask = false;
                        hasAudioTask = 0;
                        
                        //write eyetracker data
                        foreach (var switchEvent in eyeTrackingSwitchEvents)
                        {
                            eyetrackingSwitchDataStreamWriter.WriteLine($"{userId};{studyIndex};{GetPythonConformNoiseLevel(noiseLevel)};{loadLevel};{switchEvent.Key.FirstEvent};{switchEvent.Key.SecondEvent};{switchEvent.Value}");

                            
                        }
                        eyeTrackingSwitchEvents.ResetEyetrackingSwitchData();
                        
                        //calculate current event. It ends now 
                        var startTime = long.Parse(parts[(int)LogFileHeaders.Timestamp]);
                        var length = (TimeSpan.FromTicks(startTime) - TimeSpan.FromTicks(lastStartTimeInTicks))
                            .Milliseconds;

                        if (lastGazeBehaviour != "")
                        {
                            gazeBehaviourDurations[lastGazeBehaviour] += length;
                        }
                        
                        var totalTime = 0;
                        foreach (var gazeBehaviourDuration in gazeBehaviourDurations)
                        {
                            totalTime += gazeBehaviourDuration.Value;
                        }
                        
                        foreach (var gazeBehaviourDuration in gazeBehaviourDurations)
                        {
                            //we save both the actual time and the normalized time for this combination of noise level and load level 
                            var normalizedTime = totalTime != 0 ? ((float)gazeBehaviourDuration.Value) / totalTime : 0;
                            eyetrackingDataStreamWriter.WriteLine($"{userId};{studyIndex};{GetPythonConformNoiseLevel(noiseLevel)};{loadLevel};{gazeBehaviourDuration.Key};{gazeBehaviourDuration.Value};{normalizedTime.ToString(CultureInfo.InvariantCulture)};{hasAudioTask}");
                        }
                        gazeBehaviourDurations.ResetEyetrackingData();
                        

                        lastGazeBehaviour = "";
                    }

                    if (insideTask && parts[(int)LogFileHeaders.Category] == nameof(LogCategory.EyeTracking))
                    {
                        if (luxValue != 0 && parts[(int)LogFileHeaders.PupilDiameter].TryReadVector2FromCSV(out var pupilDiameter))
                        {
                     
                            if (pupilDiameter.x >= 0 && pupilDiameter.y >= 0)
                            {
                                pupilDataStorage.Add(new PupilDataStorage
                                {
                                    PupilDiameter = pupilDiameter,
                                    LuxValue = luxValue
                                });
                            }
                            
                            
                        }
                        var startTime = long.Parse(parts[(int)LogFileHeaders.Timestamp]);
                        
                        var gazeBehaviourType = parts[(int)LogFileHeaders.GazeBehaviour];

                        if (lastGazeBehaviour == "")
                        {
                            if (gazeBehaviourType != nameof(GazeBehaviorType.Unknown))
                            {
                                //this is the first event in this trial, this should avoid any event being carried over from the previous trial
                                lastStartTimeInTicks = startTime;
                                lastGazeBehaviour = gazeBehaviourType;
                            }
                        }
                        else if (!lastGazeBehaviour.Equals(gazeBehaviourType))
                        {
                            
                            
                            
                            var length = (TimeSpan.FromTicks(startTime) - TimeSpan.FromTicks(lastStartTimeInTicks))
                                .Milliseconds;
                            
                            gazeBehaviourDurations[lastGazeBehaviour] += length;
                            
                            if (gazeBehaviourType != nameof(GazeBehaviorType.Unknown))
                            {
                                var switchEvent = new EyeTrackingEvent
                                {
                                    FirstEvent = lastGazeBehaviour,
                                    SecondEvent = gazeBehaviourType
                                };
                            
                                Debug.Log($"Switch event {switchEvent.FirstEvent} to {switchEvent.SecondEvent}");
                                eyeTrackingSwitchEvents[switchEvent] += 1;
                            }
                            else
                            {
                                //we reset to avoid adding the unknown type
                                gazeBehaviourType = "";
                            }
                            
                            lastStartTimeInTicks = startTime;
                            lastGazeBehaviour = gazeBehaviourType;
                        }
                        
                        
                    }
                    
                }
                Debug.Log($"Invalid lux values: {invalidLux}");

                
            }

            foreach (var streamWriter in streamWriters)
            {
                streamWriter.Dispose();
            }

            audioTaskStreamWriter.Dispose();
            eyetrackingDataStreamWriter.Dispose();
            eyetrackingSwitchDataStreamWriter.Dispose();


        }

        
    }

    public struct PupilDataStorage
    {
        public Vector2 PupilDiameter;
        public float LuxValue;
    }

    public struct TaskPerformanceTrialStorage
    {
        public double2 MinMaxTime;
        public NoiseLevel NoiseLevel;
        public LoadLevel LoadLevel;
        public List<TrialTime> TrialTimes;
    }

    public struct TrialTime
    {
        public double ReactionTime;
        public double TrialTiming;
    }

    public struct ErrorRateData : IEquatable<ErrorRateData>
    {
        public NoiseLevel NoiseLevel;
        public LoadLevel LoadLevel;
        public int CorrectlySelected;
        public int IncorrectlySelected;
        public int NothingSelected;

        public int Total => CorrectlySelected + IncorrectlySelected + NothingSelected;

        public bool Equals(ErrorRateData other)
        {
            return NoiseLevel == other.NoiseLevel && LoadLevel == other.LoadLevel;
        }

        public override bool Equals(object obj)
        {
            return obj is ErrorRateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)NoiseLevel, (int)LoadLevel);
        }
    }
    

    public struct EyeTrackingEvent : IEquatable<EyeTrackingEvent>
    {
        public string FirstEvent;
        public string SecondEvent;

        public bool Equals(EyeTrackingEvent other)
        {
            return FirstEvent == other.FirstEvent && SecondEvent == other.SecondEvent;
        }

        public override bool Equals(object obj)
        {
            return obj is EyeTrackingEvent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FirstEvent, SecondEvent);
        }
    }

    public static class EyeTrackingDataExtension
    {
        public static void ResetEyetrackingData(this Dictionary<string, int> gazeBehaviourDurations)
        {
            gazeBehaviourDurations[nameof(GazeBehaviorType.EyesClosed)] = 0;
            gazeBehaviourDurations[nameof(GazeBehaviorType.Blink)] = 0;
            gazeBehaviourDurations[nameof(GazeBehaviorType.BlinkLeft)] = 0;
            gazeBehaviourDurations[nameof(GazeBehaviorType.BlinkRight)] = 0;
            gazeBehaviourDurations[nameof(GazeBehaviorType.Fixation)] = 0;
            gazeBehaviourDurations[nameof(GazeBehaviorType.Pursuit)] = 0;
            gazeBehaviourDurations[nameof(GazeBehaviorType.Saccade)] = 0;
        }

        public static void InitializeEyetrackingSwitchData(
            this Dictionary<EyeTrackingEvent, int> gazeBehaviourSwitchData)
        {
            
            for (int i = 1; i < 8; i++)
            {
                for (int j = 1; j < 8; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    gazeBehaviourSwitchData.Add(new EyeTrackingEvent
                    {
                        FirstEvent = ((GazeBehaviorType)i).ToString(),
                        SecondEvent = ((GazeBehaviorType)j).ToString()
                    }, 0);
                }
            }
        }
        
        public static void ResetEyetrackingSwitchData(
            this Dictionary<EyeTrackingEvent, int> gazeBehaviourSwitchData)
        {
            
            for (int i = 1; i < 8; i++)
            {
                for (int j = 1; j < 8; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    var data = new EyeTrackingEvent
                    {
                        FirstEvent = ((GazeBehaviorType)i).ToString(),
                        SecondEvent = ((GazeBehaviorType)j).ToString()
                    };
                    gazeBehaviourSwitchData[data] = 0;
                }
            }
        }

        public interface ILogFileProcessor
        {
            public void ProcessLogFileEvent(LogCategory logEvent, ILogFileState state);
        }

        public interface ILogFileState
        {
            public int StudyIndex { get; }
            public NoiseLevel NoiseLevel { get; }
            public LoadLevel LoadLevel { get; }
            
            public bool InsideTrial { get; }
            
            public string UserId { get; }
        }
    }
}