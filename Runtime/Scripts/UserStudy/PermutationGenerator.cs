using System;
using System.Collections.Generic;
using System.Linq;
using DistractorTask.UserStudy.Core;
using Unity.Mathematics;

namespace DistractorTask.UserStudy
{
    internal static class PermutationGenerator
    {
        internal static ConditionPermutation[] GeneratePermutations(Condition condition, int startCondition)
        {
            var noiseLevelCount = math.min(Count((uint)condition.noiseLevels),
                Enum.GetNames(typeof(NoiseLevel)).Length);
            var loadLevelCount = math.min(Count((uint)condition.loadLevels),
                Enum.GetNames(typeof(LoadLevel)).Length);
            
            var permutations = (int)(noiseLevelCount * loadLevelCount);

            var result = new List<ConditionPermutation>(permutations);

            var noiseLevels = GenerateNoiseLevels(condition.noiseLevels);
            var loadLevels = GenerateLoadLevels(condition.loadLevels);

            for (int loadLevel = 0; loadLevel < loadLevelCount; loadLevel++)
            {
                for (int noiseLevel = 0; noiseLevel < noiseLevelCount; noiseLevel++)
                {
                    result.Add(new ConditionPermutation
                    {
                        LoadLevel = loadLevels[loadLevel],
                        NoiseLevel = noiseLevels[noiseLevel]
                    });
                }
            }
            
            if (condition.hasAudioTask)
            {
                var resultWithAudioTask = new ConditionPermutation[result.Count * 2];
                for (int i = 0; i < result.Count; i++)
                {
                    var permutation = result[i];
                    resultWithAudioTask[i * 2] = permutation;
                    permutation.HasAudioTask = true;
                    resultWithAudioTask[i * 2 + 1] = permutation;
                }

                result = resultWithAudioTask.ToList();
            }
            
            for (int i = 0; i < startCondition; i++)
            {
                var elementZero = result[0];
                result.RemoveAt(0);
                result.Add(elementZero);
            }

            return result.ToArray();
        }

        private static uint Count(uint enumValue)
        {
            var v = enumValue;
            v = v - ((v >> 1) & 0x55555555); // reuse input as temporary
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // temp
            var c = ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
            return c;
        }

        private static List<NoiseLevel> GenerateNoiseLevels(NoiseLevel noiseLevel)
        {
            var result = new List<NoiseLevel>();

            AddNoiseLevel(noiseLevel, NoiseLevel.None, ref result);
            AddNoiseLevel(noiseLevel, NoiseLevel.Low, ref result);
            AddNoiseLevel(noiseLevel, NoiseLevel.Audio, ref result);
            AddNoiseLevel(noiseLevel, NoiseLevel.Visual, ref result);
            AddNoiseLevel(noiseLevel, NoiseLevel.High, ref result);
            AddNoiseLevel(noiseLevel, NoiseLevel.Max, ref result);
            return result;
        }

        private static List<LoadLevel> GenerateLoadLevels(LoadLevel loadLevel)
        {
            var result = new List<LoadLevel>();

            AddLoadLevel(loadLevel, LoadLevel.Low, ref result);
            AddLoadLevel(loadLevel, LoadLevel.High, ref result);
            return result;
        }

        private static void AddLoadLevel(LoadLevel noiseLevel, LoadLevel targetNoiseLevel, ref List<LoadLevel> result)
        {
            if ((noiseLevel & targetNoiseLevel) == targetNoiseLevel)
            {
                result.Add(targetNoiseLevel);
            }
        }

        private static void AddNoiseLevel(NoiseLevel noiseLevel, NoiseLevel targetNoiseLevel,
            ref List<NoiseLevel> result)
        {
            if ((noiseLevel & targetNoiseLevel) == targetNoiseLevel)
            {
                result.Add(targetNoiseLevel);
            }
        }
    }
}