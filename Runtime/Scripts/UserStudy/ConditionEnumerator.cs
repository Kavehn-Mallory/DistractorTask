using System.Collections;
using System.Collections.Generic;

namespace DistractorTask.UserStudy
{
    public class ConditionEnumerator : IEnumerator<TrialsEnumerator>
    {

        private StudyCondition _condition;
        private readonly TrialsEnumerator _trialsEnumerator;

        private int _currentTrial = -1;

        public ConditionEnumerator(StudyCondition studyCondition)
        {
            _condition = studyCondition;
            _trialsEnumerator = new TrialsEnumerator(studyCondition.repetitionsPerTrial);
        }
        
        public void Dispose()
        {
            
        }

        public bool MoveNext()
        {
            _currentTrial++;
            _trialsEnumerator.Reset();
            return _currentTrial < _condition.trialCount;
        }

        public void Reset()
        {
            _currentTrial = -1;
            _trialsEnumerator.Reset();
        }

        public int CurrentTrialIndex => _currentTrial;

        public TrialsEnumerator Current => _trialsEnumerator;

        object IEnumerator.Current => Current;
    }
}