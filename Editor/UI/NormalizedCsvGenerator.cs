using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DistractorTask.Logging;
using DistractorTask.UserStudy.Core;
using MagicLeap.OpenXR.Features.EyeTracker;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
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
        
        

        public const string Header =
            "Time;Timestamp;UserId;StudyIndex;NoiseLevel;LoadLevel;TrialCount;RepetitionsPerTrial;TrialTargetIndex;TrialSelectedIndex;TrialSymbolOrder;AnchorPointIndex;StartTime;ReactionTime;HasAudioTask";

        public const string AudioTaskHeader = "Time;UserId;NoiseLevel;LoadLevel;ReactionTime";
        
        

        public const string EyeTrackingDataHeader = "UserId;StudyIndex;NoiseLevel;LoadLevel;GazeBehaviour;Duration;NormalizedDuration;HasAudioTask";
        
        public const string EyeTrackingSwitchDataHeader = "UserId;StudyIndex;NoiseLevel;LoadLevel;GazeBehaviourBefore;GazeBehaviourAfter;Count";

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
            var eyetrackingSwitchDataStreamWriter = new StreamWriter(path + "/" + EyeTrackingSwitchDataFileName);

            foreach (var streamWriter in streamWriters)
            {
                streamWriter.WriteLine(Header);
            }
            
            audioTaskStreamWriter.WriteLine(AudioTaskHeader);
            eyetrackingDataStreamWriter.WriteLine(EyeTrackingDataHeader);
            eyetrackingSwitchDataStreamWriter.WriteLine(EyeTrackingSwitchDataHeader);

            Dictionary<EyeTrackingEvent, int> eyeTrackingSwitchEvents = new Dictionary<EyeTrackingEvent, int>();
            
            eyeTrackingSwitchEvents.InitializeEyetrackingSwitchData();
            
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


                var userId = "";
                NoiseLevel noiseLevel = NoiseLevel.None;
                LoadLevel loadLevel = LoadLevel.Low;
                int hasAudioTask = 0;
                bool insideTask = false;
                long lastStartTimeInTicks = 0;
                int identicalStartTimeCounter = 0;
                int identicalStartTimeWithDifferentDurationsCounter = 0;
                string lastGazeBehaviour = "";
                ulong lastDuration = 0;
                
                
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