using System.Collections;
using System.Collections.Generic;
using DistractorTask.UserStudy.Core;
using DistractorTask.UserStudy.DistractorSelectionStage;

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
    
    

    public struct StudyCondition
    {

        internal StudyCondition(ConditionPermutation permutation, int trialCount, int repetitionsPerTrial)
        {
            TrialCount = trialCount;
            RepetitionsPerTrial = repetitionsPerTrial;
            LoadLevel = permutation.LoadLevel;
            NoiseLevel = permutation.NoiseLevel;
        }
        
        public StudyCondition(LoadLevel loadLevel, NoiseLevel noiseLevel, int trialCount, int repetitionsPerTrial)
        {
            TrialCount = trialCount;
            RepetitionsPerTrial = repetitionsPerTrial;
            LoadLevel = loadLevel;
            NoiseLevel = noiseLevel;
        }
        
        public LoadLevel LoadLevel;
        public NoiseLevel NoiseLevel;
        public int RepetitionsPerTrial;
        public int TrialCount;
    }
}