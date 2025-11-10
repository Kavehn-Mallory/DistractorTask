using System;
using System.Collections.Generic;
using System.IO;
using DistractorTask.Logging;
using DistractorTask.UserStudy.Core;
using MagicLeap.OpenXR.Features.EyeTracker;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.MagicLeap;

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
        
        

        public const string Header =
            "Time;Timestamp;UserId;StudyIndex;NoiseLevel;LoadLevel;TrialCount;RepetitionsPerTrial;TrialTargetIndex;TrialSelectedIndex;TrialSymbolOrder;AnchorPointIndex;StartTime;ReactionTime;HasAudioTask";

        public const string AudioTaskHeader = "Time;UserId;NoiseLevel;LoadLevel;ReactionTime";
        
        

        public const string EyeTrackingDataHeader = "Time;UserId;NoiseLevel;LoadLevel;GazeBehaviour;Duration;HasAudioTask";

        public const string NoAudioResponseValue = "None";

        private static string GetPythonConformNoiseLevel(NoiseLevel noiseLevel)
        {
            if(noiseLevel == NoiseLevel.None)
            {
                return "No Noise";
            }

            return $"{noiseLevel}";
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

            foreach (var streamWriter in streamWriters)
            {
                streamWriter.WriteLine(Header);
            }
            
            audioTaskStreamWriter.WriteLine(AudioTaskHeader);
            eyetrackingDataStreamWriter.WriteLine(EyeTrackingDataHeader);
            
            Dictionary<string, int> gazeBehaviourDurations = new Dictionary<string, int>();
                
            gazeBehaviourDurations.Add(nameof(GazeBehaviorType.Unknown), 0);
            gazeBehaviourDurations.Add(nameof(GazeBehaviorType.EyesClosed), 0);
            gazeBehaviourDurations.Add(nameof(GazeBehaviorType.Blink), 0);
            gazeBehaviourDurations.Add(nameof(GazeBehaviorType.BlinkLeft), 0);
            gazeBehaviourDurations.Add(nameof(GazeBehaviorType.BlinkRight), 0);
            gazeBehaviourDurations.Add(nameof(GazeBehaviorType.Fixation), 0);
            gazeBehaviourDurations.Add(nameof(GazeBehaviorType.Pursuit), 0);
            gazeBehaviourDurations.Add(nameof(GazeBehaviorType.Saccade), 0);

            foreach (var filePath in paths)
            {
                using StreamReader reader = new StreamReader(filePath);
                int studyIndex = -1;


                var userId = "";
                NoiseLevel noiseLevel = NoiseLevel.None;
                LoadLevel loadLevel = LoadLevel.Low;
                int hasAudioTask = 0;
                bool insideTask = false;
                string lastStartTime = "";
                int identicalStartTimeCounter = 0;
                string lastGazeBehaviour = "";
                
                
                gazeBehaviourDurations.ResetEyetrackingData();
                

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
                    

                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialConfirmation))
                    {
                        if (!insideTask)
                        {
                            Debug.LogWarning($"File {filePath} has TrialConfirmation-Data without TrialStart in Study {studyIndex}");
                        }
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
                    }

                    if (insideTask && parts[(int)LogFileHeaders.Category] == nameof(LogCategory.EyeTracking))
                    {

                        var duration = ulong.Parse(parts[(int)LogFileHeaders.GazeBehaviourDuration]);
                        var startTime = parts[(int)LogFileHeaders.StartTime];

                        
                        var gazeBehaviourType = parts[(int)LogFileHeaders.GazeBehaviour];
                        //gazeBehaviourDurations.TryAdd(gazeBehaviourType, 0);
                        TimeSpan timeStampDuration = TimeSpan.FromTicks((long)(duration / 100));;
                        
                        if (startTime != lastStartTime)
                        {
                            
                            gazeBehaviourDurations[gazeBehaviourType] += timeStampDuration.Milliseconds;
                            identicalStartTimeCounter++;
                        }
                        else if(!lastGazeBehaviour.Equals(gazeBehaviourType))
                        {
                            gazeBehaviourDurations[gazeBehaviourType] += timeStampDuration.Milliseconds;
                            identicalStartTimeCounter--;
                        }

                        lastStartTime = startTime;
                        lastGazeBehaviour = gazeBehaviourType;
                        eyetrackingDataStreamWriter.WriteLine($"{parts[(int)LogFileHeaders.Time]};{userId};{GetPythonConformNoiseLevel(noiseLevel)};{loadLevel};{gazeBehaviourType};{duration};{hasAudioTask}");
                    }
                    
                }

                float timer = 0;
                Debug.Log($"{userId} Eyetracking Data with {identicalStartTimeCounter} identical timings");
                foreach (var gazeBehaviourPair in gazeBehaviourDurations)
                {
                    timer += gazeBehaviourPair.Value;
                    Debug.Log($"Spent {gazeBehaviourPair.Value.ToString()} milliseconds in {gazeBehaviourPair.Key}");
                }
                Debug.Log($"{userId} spent {timer / (1000f * 60f)} minutes in the application?");
            }

            foreach (var streamWriter in streamWriters)
            {
                streamWriter.Dispose();
            }

            audioTaskStreamWriter.Dispose();


        }

        
    }

    public static class EyeTrackingDataExtension
    {
        public static void ResetEyetrackingData(this Dictionary<string, int> gazeBehaviourDurations)
        {
            gazeBehaviourDurations[nameof(GazeBehaviorType.Unknown)] = 0;
            gazeBehaviourDurations[nameof(GazeBehaviorType.EyesClosed)] = 0;
            gazeBehaviourDurations[nameof(GazeBehaviorType.Blink)] = 0;
            gazeBehaviourDurations[nameof(GazeBehaviorType.BlinkLeft)] = 0;
            gazeBehaviourDurations[nameof(GazeBehaviorType.BlinkRight)] = 0;
            gazeBehaviourDurations[nameof(GazeBehaviorType.Fixation)] = 0;
            gazeBehaviourDurations[nameof(GazeBehaviorType.Pursuit)] = 0;
            gazeBehaviourDurations[nameof(GazeBehaviorType.Saccade)] = 0;
        }
    }
}