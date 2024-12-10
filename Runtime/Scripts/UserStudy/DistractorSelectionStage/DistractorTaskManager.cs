using System;
using System.Collections.Generic;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using DistractorTask.UserStudy.Core;
using UnityEngine;

namespace DistractorTask.UserStudy.DistractorSelectionStage
{
    public class DistractorTaskManager : SendingStudyStageComponent<DistractorSelectionStageEvent>
    {
        public Study[] trials = Array.Empty<Study>();
        private int _currentTrialIndex;

        private int _currentConditionIndex;

        private void Awake()
        {
            GenerateTrialOptions();
        }

        private void GenerateTrialOptions()
        {
            for (var i = 0; i < trials.Length; i++)
            {
                var trial = trials[i];
                var noiseLevelValues = Mathf.Min(Count((uint)trial.conditions.noiseLevels),
                    Enum.GetNames(typeof(NoiseLevel)).Length);
                var loadLevels = Mathf.Min(Count((uint)trial.conditions.loadLevels),
                    Enum.GetNames(typeof(LoadLevel)).Length);


                var options = loadLevels * noiseLevelValues;

                Debug.Log($"Options: {options}");

                var optionList = new List<(LoadLevel, NoiseLevel)>();


                var loadLevel = GenerateLoadLevels(trial.conditions.loadLevels);
                var noiseLevel = GenerateNoiseLevels(trial.conditions.noiseLevels);


                foreach (var level in loadLevel)
                {
                    foreach (var noise in noiseLevel)
                    {
                        optionList.Add((level, noise));
                    }
                }

                foreach (var tuple in optionList)
                {
                    Debug.Log($"{tuple.Item1}, {tuple.Item2}");
                }

                trial.conditionList = optionList;
                trials[i] = trial;
            }
        }

        public override void StartStudy(INetworkManager manager)
        {
            Manager.RegisterCallback<ConfirmationData>(OnStudyBegin);
            base.StartStudy(manager);
        }

        private void OnStudyBegin(ConfirmationData data)
        {
            Manager.UnregisterCallback<ConfirmationData>(OnStudyBegin);
            Manager.RegisterCallback<TrialCompletedData>(OnTrialCompleted);
            _currentTrialIndex = 0;
            _currentConditionIndex = 0;
            StartTrial();
        }

        private void StartTrial()
        {
            var trial = trials[_currentTrialIndex];
            var positions = new int[]
            {
                0, 1, 2, 3, 4, 5
            };
            Manager.TransmitNetworkMessage(new DistractorSelectionTrialData
            {
                loadLevel = (byte)trial.conditionList[_currentConditionIndex].Item1,
                selectionCount = trial.selectionsPerTrial,
                markers = positions
            });
            _currentConditionIndex++;
        }

        private void OnTrialCompleted(TrialCompletedData obj)
        {
            if (_currentConditionIndex >= trials[_currentTrialIndex].conditionList.Count)
            {
                _currentTrialIndex++;
                //todo check if we have another condition 
                if (_currentTrialIndex >= trials.Length)
                {
                    //we are done
                    EndStudy(Manager);
                    return;
                }
                _currentConditionIndex = 0;
            }
            StartTrial();
            //todo check if there are more trials and start the next one 
        }
        
        public override void EndStudy(INetworkManager manager)
        {
            Manager.UnregisterCallback<TrialCompletedData>(OnTrialCompleted);
            base.EndStudy(manager);
        }
        


        private static List<NoiseLevel> GenerateNoiseLevels(NoiseLevel noiseLevel)
        {
            var result = new List<NoiseLevel>();

            AddNoiseLevel(noiseLevel, NoiseLevel.None, ref result);
            AddNoiseLevel(noiseLevel, NoiseLevel.Low, ref result);
            AddNoiseLevel(noiseLevel, NoiseLevel.Audio, ref result);
            AddNoiseLevel(noiseLevel, NoiseLevel.Visual, ref result);
            AddNoiseLevel(noiseLevel, NoiseLevel.High, ref result);
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

        private static void AddNoiseLevel(NoiseLevel noiseLevel, NoiseLevel targetNoiseLevel, ref List<NoiseLevel> result)
        {
            if ((noiseLevel & targetNoiseLevel) == targetNoiseLevel)
            {
                result.Add(targetNoiseLevel);
            }
        }


        private static uint Count(uint enumValue)
        {
            var v = enumValue;
            v = v - ((v >> 1) & 0x55555555); // reuse input as temporary
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // temp
            var c = ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
            return c;
        }
    }


    [Serializable]
    public struct Study
    {
        public int selectionsPerTrial;
        public int repeatsPerTrial;
        public Condition conditions;

        public List<(LoadLevel, NoiseLevel)> conditionList;

    }
}