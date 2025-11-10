using System;
using System.Collections.Generic;
using System.IO;
using DistractorTask.Logging;
using DistractorTask.UserStudy.Core;
using UnityEditor;
using UnityEngine;

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
            
            

            foreach (var filePath in paths)
            {
                using StreamReader reader = new StreamReader(filePath);
                int studyIndex = -1;


                var userId = "";
                NoiseLevel noiseLevel = NoiseLevel.None;
                LoadLevel loadLevel = LoadLevel.Low;
                int hasAudioTask = 0;
                bool insideTask = false;
                
                Dictionary<string, ulong> gazeBehaviourDurationDebug = new Dictionary<string, ulong>();

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
                        var gazeBehaviourType = parts[(int)LogFileHeaders.GazeBehaviour];
                        gazeBehaviourDurationDebug.TryAdd(gazeBehaviourType, 0);
                        gazeBehaviourDurationDebug[gazeBehaviourType] += duration;
                        
                        eyetrackingDataStreamWriter.WriteLine($"{parts[(int)LogFileHeaders.Time]};{userId};{GetPythonConformNoiseLevel(noiseLevel)};{loadLevel};{gazeBehaviourType};{duration};{hasAudioTask}");
                    }
                    
                }

                Debug.Log($"{userId} Eyetracking Data");
                foreach (var gazeBehaviourPair in gazeBehaviourDurationDebug)
                {
                    Debug.Log($"Spent {gazeBehaviourPair.Value} units in {gazeBehaviourPair.Key}");
                }
            }

            foreach (var streamWriter in streamWriters)
            {
                streamWriter.Dispose();
            }

            audioTaskStreamWriter.Dispose();


        }

        
    }
}