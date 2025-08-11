using System;
using System.Collections.Generic;
using System.IO;
using DistractorTask.Logging;
using DistractorTask.UserStudy.Core;
using Unity.Mathematics;
using UnityEngine;

namespace DistractorTask.Editor.UI
{
    public class UserStudyEvaluationTextBased
    {
        private string[] _filePaths;

        public UserStudyEvaluationTextBased(string[] filePaths)
        {
            _filePaths = filePaths;
        }
        
        
        
            
            
        public UserStudyEvaluation.ReactionTimeData[] CalculateReactionTimeForCondition(LoadLevel loadLevel, NoiseLevel noiseLevel)
        {
            var timings = new List<UserStudyEvaluation.ReactionTimeData>();
            foreach (var file in _filePaths)
            {
                using StreamReader reader = new StreamReader(file);
                bool insideTrial = false;
                TimeSpan trialStart = new TimeSpan();
                var maxReactionTime = 0f;
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    var parts = line.Split(';');
                    if (parts[2] == nameof(LogCategory.TrialBegin))
                    {
                        var noise = Enum.Parse<NoiseLevel>(parts[15]);
                        var load = Enum.Parse<LoadLevel>(parts[16]);
                        insideTrial = (noise == noiseLevel && load == loadLevel);
                        trialStart = new TimeSpan(long.Parse(parts[1]));
                        continue;
                    }
                    
                    if (!insideTrial || parts[2] != nameof(LogCategory.TrialConfirmation))
                    {
                        continue;
                    }

                    var start = new TimeSpan(long.Parse(parts[24]));
                    
                    var reactionTime = (new TimeSpan(long.Parse(parts[25])) - start).Ticks / TimeSpan.TicksPerMillisecond;

                    maxReactionTime = math.max(reactionTime, maxReactionTime);
                    
                    timings.Add(new UserStudyEvaluation.ReactionTimeData
                    {
                        Time = (start - trialStart).Ticks,
                        Duration = reactionTime
                    });
                }
                Debug.Log($"Max reaction time: {maxReactionTime}");
            }
            
            return timings.ToArray();
        }
    }
}