using System;
using System.Collections;
using System.Collections.Generic;
using DistractorTask.UserStudy.Core;
using DistractorTask.UserStudy.DataDrivenSetup;

namespace DistractorTask.UserStudy
{
    public class StudyConditionsEnumerator : IEnumerator<StudyCondition>
    {
        
        private ConditionPermutation[] _permutations;
        private int _trialCount;
        private int _repetitionsPerTrial;
        private int _currentCondition;

        public StudyConditionsEnumerator(Study studyCondition, int startCondition = 0)
        {
            _trialCount = studyCondition.trialsPerCondition;
            _repetitionsPerTrial = studyCondition.selectionsPerTrial;
            
            _permutations = PermutationGenerator.GeneratePermutations(studyCondition.conditions, startCondition);
        }


        public bool MoveNext()
        {
            _currentCondition++;

            return _currentCondition < _permutations.Length;
        }

        public void Reset()
        {
            _currentCondition = -1;
        }

        public StudyCondition Current =>
            new StudyCondition(_permutations[_currentCondition], _trialCount, _repetitionsPerTrial);

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            
        }


    }
    
    internal struct ConditionPermutation
    {
        public LoadLevel LoadLevel;
        public NoiseLevel NoiseLevel;
    }
    
    

    [Serializable]
    public struct StudyCondition
    {
        
        public LoadLevel loadLevel;
        public NoiseLevel noiseLevel;
        public int repetitionsPerTrial;
        public int trialCount;

        internal StudyCondition(ConditionPermutation permutation, int trialCount, int repetitionsPerTrial)
        {
            this.trialCount = trialCount;
            this.repetitionsPerTrial = repetitionsPerTrial;
            loadLevel = permutation.LoadLevel;
            noiseLevel = permutation.NoiseLevel;
        }
        
        public StudyCondition(LoadLevel loadLevel, NoiseLevel noiseLevel, int trialCount, int repetitionsPerTrial)
        {
            this.trialCount = trialCount;
            this.repetitionsPerTrial = repetitionsPerTrial;
            this.loadLevel = loadLevel;
            this.noiseLevel = noiseLevel;
        }

        public override string ToString()
        {
            return
                $"{nameof(loadLevel)}: {loadLevel.ToString()}; {nameof(noiseLevel)}: {noiseLevel.ToString()}; {nameof(repetitionsPerTrial)}: {repetitionsPerTrial.ToString()}; {nameof(trialCount)}: {trialCount.ToString()}";
        }
    }
}