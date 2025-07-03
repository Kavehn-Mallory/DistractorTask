using System.Collections;

namespace DistractorTask.UserStudy
{
    public class TrialsEnumerator : IEnumerator
    {
        private int _currentTrial;
        private int _trialCount;


        public TrialsEnumerator(int trialCount)
        {
            _trialCount = trialCount;
            _currentTrial = -1;
        }

        public bool MoveNext()
        {
            _currentTrial++;
            return _currentTrial < _trialCount;
        }

        public void Reset()
        {
            _currentTrial = -1;
        }

        public int CurrentRepetition => _currentTrial;

        public object Current => _currentTrial;
    }
    
    
}