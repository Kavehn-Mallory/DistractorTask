using System;
using System.Linq;
using DistractorTask.UserStudy.Core;
using UnityEngine;

namespace DistractorTask.Logging
{
    [CreateAssetMenu(fileName = "TrialResults", menuName = "DistractorTask/Analysis/TrialResults", order = 0)]
    public class TrialResultsAsset : ScriptableObject
    {
        public string[] userIds;
        public SerializableTrialResult[] trialResults;

        public SerializableTrialTiming[] GetTimingsForUserId(string userId)
        {
            for (int i = 0; i < userIds.Length; i++)
            {
                if (userId == userIds[i])
                {
                    return trialResults.Where(result => result.id == i).Select(result => result.timing).ToArray();
                }
            }

            return Array.Empty<SerializableTrialTiming>();
        }
        
        public SerializableTrialTiming[] GetTimingsForCondition(LoadLevel loadLevel, NoiseLevel noiseLevel)
        {
            return trialResults.Where(result => result.timing.loadLevel == loadLevel && result.timing.noiseLevel == noiseLevel).Select(result => result.timing).ToArray();
        }
    }

    [Serializable]
    public struct SerializableTrialResult
    {
        public int id;
        public SerializableTrialTiming timing;
    }

    [Serializable]
    public struct SerializableTrialTiming
    {
        public NoiseLevel noiseLevel;
        public LoadLevel loadLevel;
        public int[] durations;

        public SerializableTrialTiming(TrialTiming timing)
        {
            this.noiseLevel = timing.NoiseLevel;
            this.loadLevel = timing.LoadLevel;
            durations = new int[timing.Durations.Length];

            for (int i = 0; i < durations.Length; i++)
            {
                durations[i] = timing.Durations[i].Milliseconds;
            }
        }
    }
}