using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DistractorTask.Logging;
using DistractorTask.UserStudy.Core;
using Unity.Mathematics;
using UnityEngine;

namespace DistractorTask.Editor.UI
{
    public class UserStudyEvaluationTextBased : ScriptableObject
    {
        [SerializeField]
        private string[] filePaths = Array.Empty<string>();


        [SerializeField]
        private List<string> userIds = new();

        public const string NoUserIdFound = "No Valid Id";

        public const string UserIdsArrayFieldName = nameof(userIds);
        public const string DateRangeFieldName = nameof(dateRange);
        public List<string> UserIds => userIds;

        [SerializeField]
        private string dateRange = "Hello";

        public string DateRange => dateRange;
        public string[] FilePaths => filePaths;

        public void SetFilePaths(string[] paths, string[] validUserIds)
        {
            var validFilePaths = new List<string>();
            
            
            var minDate = DateTime.MaxValue;
            var maxDate = DateTime.MinValue;
            var counter = 0;
            for (var i = 0; i < paths.Length; i++)
            {
                
                var filePath = paths[i];
                var fileNameParts = Path.GetFileName(filePath).Split('_');
                if (fileNameParts.Length > 1)
                {
                    var fileName = fileNameParts[1];

                    foreach (var validId in validUserIds)
                    {
                        if (string.Compare(validId, fileName, CultureInfo.CurrentCulture,
                                CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0)
                        {
                            var date = GetDateOfStudy(fileNameParts[0]);
                    
                    
                            userIds.Add(fileName);
                            validFilePaths.Add(filePath);
                            if (minDate > date)
                            {
                                minDate = date;
                            }

                            if (maxDate < date)
                            {
                                maxDate = date;
                            }
                            break;
                        }
                    }
                    
                }
                else
                {
                    counter++;
                }
            }

            Debug.Log($"Removed {counter} invalid files");
            this.filePaths = validFilePaths.ToArray();
            //userIds = new List<string>(validUserIds);
            dateRange = $"{minDate.Date.ToString(CultureInfo.CurrentCulture)} - {maxDate.Date.ToString(CultureInfo.CurrentCulture)}";
        }

        public TrialData[] CheckTrialCountDataForStudies(string[] ids, int studyCount)
        {
            var result = new List<TrialData>();
            for (var idIndex = 0; idIndex < ids.Length; idIndex++)
            {
                var id = ids[idIndex];
                for (var i = 0; i < userIds.Count; i++)
                {
                    var userId = userIds[i];
                    if (string.Compare(userId, id, CultureInfo.CurrentCulture,
                            CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0)
                    {
                        result.Add(GetTrialCountsForStudyLog(filePaths[i], id, studyCount));
                    }
                }
            }

            return result.ToArray();
        }

        private TrialData GetTrialCountsForStudyLog(string filePath, string id, int studyCount)
        {
            var result = new int[studyCount];
            var trialCountForCurrentStudy = 0;
            using StreamReader reader = new StreamReader(filePath);
            var trialsWithoutStudy = 0;

            int studyIndex = -1;

            while (reader.Peek() >= 0)
            {
                var line = reader.ReadLine();
                var parts = line.Split(';');
                
                if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.StudyBegin))
                {
                    if (trialCountForCurrentStudy > 0 && studyIndex >= 0)
                    {
                        result[studyIndex] = trialCountForCurrentStudy;
                    }

                    studyIndex = int.Parse(parts[(int)LogFileHeaders.StudyIndex]);
                    trialCountForCurrentStudy = 0;
                    continue;
                }

                

                if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialConfirmation))
                {
                    if (studyIndex < 0)
                    {
                        trialsWithoutStudy++;
                        continue;
                    }
                    trialCountForCurrentStudy++;
                }

                if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.StudyEnd))
                {
                    Debug.Log(id);
                    result[studyIndex] = trialCountForCurrentStudy;
                    trialCountForCurrentStudy = 0;
                    studyIndex = -1;
                }

            }

            if (trialCountForCurrentStudy > 0)
            {
                result[studyIndex] = trialCountForCurrentStudy;
            }
            return new TrialData
            {
                Trials = result.ToArray(),
                Id = id
            };
        }


        public List<Duplicate> FindDuplicates()
        {
            var result = new List<Duplicate>();

            var dictionary = new Dictionary<string, int>();

            var hashset = new HashSet<string>();

            foreach (var userId in userIds)
            {
                if (hashset.Add(userId))
                {
                    continue;
                }

                dictionary.TryAdd(userId, 1);
                dictionary[userId] += 1;

            }

            foreach (var pair in dictionary)
            {
                result.Add(new Duplicate
                {
                    count = pair.Value,
                    fileName = pair.Key
                });
            }

            return result;
        }

        private DateTime GetDateOfStudy(string date)
        {
            var splitIt = date.Split(" ");
            var hours = splitIt[1].Replace('-', ':');
            var actualDateString = splitIt[0] + " " + hours;
            return DateTime.ParseExact(actualDateString, "u", CultureInfo.InvariantCulture);
        }
            
            
        public ReactionTimeData[] CalculateReactionTimeForCondition(LoadLevel loadLevel, NoiseLevel noiseLevel)
        {
            var timings = new List<ReactionTimeData>();
            foreach (var file in filePaths)
            {
                using StreamReader reader = new StreamReader(file);
                bool insideTrial = false;
                TimeSpan trialStart = new TimeSpan();
                var maxReactionTime = 0f;
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    var parts = line.Split(';');
                    if (parts[(int)LogFileHeaders.Category] == nameof(LogCategory.TrialBegin))
                    {
                        var noise = Enum.Parse<NoiseLevel>(parts[(int)LogFileHeaders.NoiseLevel]);
                        var load = Enum.Parse<LoadLevel>(parts[(int)LogFileHeaders.LoadLevel]);
                        insideTrial = (noise == noiseLevel && load == loadLevel);
                        trialStart = new TimeSpan(long.Parse(parts[1]));
                        continue;
                    }
                    
                    if (!insideTrial || parts[(int)LogFileHeaders.Category] != nameof(LogCategory.TrialConfirmation))
                    {
                        continue;
                    }

                    var start = new TimeSpan(long.Parse(parts[(int)LogFileHeaders.StartTime]));
                    
                    var reactionTime = (new TimeSpan(long.Parse(parts[(int)LogFileHeaders.ReactionTime])) - start).Ticks / TimeSpan.TicksPerMillisecond;

                    maxReactionTime = math.max(reactionTime, maxReactionTime);
                    
                    timings.Add(new ReactionTimeData
                    {
                        Time = (start - trialStart).Ticks,
                        Duration = reactionTime
                    });
                }
                Debug.Log($"Max reaction time: {maxReactionTime}");
            }
            
            return timings.ToArray();
        }
        
        public struct StudyData
        {
            public int TrialCount;
            public int ActualTrialCount;
            public int RepetitionsPerTrial;
            public int EventTimingIndex;
        }

        public struct TrialTiming
        {
            public NoiseLevel NoiseLevel;
            public LoadLevel LoadLevel;
            public ReactionTimeData[] ReactionTimes;
        }

        public struct TrialData
        {
            public string Id;
            public int[] Trials;
        }

        public struct EventTimings
        {
            public int StartIndex;
            public int EndIndex;
            public StudyEventType EventType;
            public int DataIndex;
        }

        public enum StudyEventType
        {
            StudyStage,
            Trial
        }
        
        public struct ReactionTimeData
        {
            public float Time;
            public float Duration;
        }

        [Serializable]
        public struct Duplicate
        {
            public string fileName;
            public int count;
        }
    }

    public class UserIdComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            Debug.Log($"Comparing {x} and {y}");
            return string.Compare(x, y, CultureInfo.CurrentCulture,
                CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0;
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }

    public static class EvaluationHelper
    {

    }

    public enum LogFileHeaders
    {
        Time,
        Timestamp,
        Category,
        UserId,
        ParticipantType,
        CameraPosition,
        CameraRotation,
        MarkerPointCount,
        DistanceFromCamera,
        DistanceToWall,
        HitPointWallPosition,
        HitPointWallNormal,
        AnchorPointPosition,
        StudyName,
        StudyIndex,
        NoiseLevel,
        LoadLevel,
        TrialCount,
        RepetitionsPerTrial,
        AudioTaskReactionTime,
        TrialTargetIndex,
        TrialSelectedIndex,
        TrialSymbolOrder,
        AnchorPointIndex,
        StartTime,
        ReactionTime,
        LeftEyePosition,
        RightEyePosition,
        EyeDimensions,
        PupilDiameter,
        GazeBehaviour,
        GazeBehaviourDuration,
        VideoPath,
        AudioPath,
        Acceleration,
        AngularVelocity,
        LinearAcceleration,
        Attitude,
        Lux
    }
}