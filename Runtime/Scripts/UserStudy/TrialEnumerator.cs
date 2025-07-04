using System.Collections;
using System.Collections.Generic;

namespace DistractorTask.UserStudy
{
    /// <summary>
    /// Repeats the specified condition trial count times. For each trial, creates a TrialRepetitionEnumerator 
    /// </summary>
    public class TrialEnumerator : IEnumerator<TrialRepetitionEnumerator>
    {

        private StudyCondition _condition;
        private readonly TrialRepetitionEnumerator _trialRepetitionEnumerator;

        private int _currentTrial = -1;

        public TrialEnumerator(StudyCondition studyCondition)
        {
            _condition = studyCondition;
            _trialRepetitionEnumerator = new TrialRepetitionEnumerator(studyCondition.repetitionsPerTrial);
        }
        
        public void Dispose()
        {
            
        }

        public bool MoveNext()
        {
            _currentTrial++;
            _trialRepetitionEnumerator.Reset();
            return _currentTrial < _condition.trialCount;
        }

        public void Reset()
        {
            _currentTrial = -1;
            _trialRepetitionEnumerator.Reset();
        }

        public int CurrentTrialIndex => _currentTrial;

        public TrialRepetitionEnumerator Current => _trialRepetitionEnumerator;

        object IEnumerator.Current => Current;

        public int TrialCount => _condition.trialCount;
        public int RepetitionsPerTrial => _condition.repetitionsPerTrial;
    }
}