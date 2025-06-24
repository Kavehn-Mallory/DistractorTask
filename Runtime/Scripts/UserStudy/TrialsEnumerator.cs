using System.Collections;
using System.Collections.Generic;

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

        public object Current => _currentTrial;
    }

    public class DistractorTaskEnumerator : IEnumerator<DistractorTaskState>
    {
        private readonly TrialsEnumerator _trialsEnumerator;

        private readonly int _taskCount;
        private readonly int _startGroup;

        private int _currentTask;
        

        //todo add the load values / setup here, I think I already generate it somewhere? 
        //Then we can just iterate it and then return the current value or something?
        public DistractorTaskEnumerator(int trialCount, int startGroup, int groupCount)
        {
            _trialsEnumerator = new TrialsEnumerator(trialCount);
            _taskCount = groupCount;
            _startGroup = startGroup;
            _currentTask = -1;
        }

        public bool MoveNext()
        {
            _currentTask++;

            _trialsEnumerator.Reset();
            return _currentTask < _taskCount;
        }

        public void Reset()
        {
            _currentTask = -1;
            _trialsEnumerator.Reset();
        }

        public DistractorTaskState Current => new(_trialsEnumerator, CalculateTaskGroup());

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            
        }

        private int CalculateTaskGroup()
        {
            return (_startGroup + _currentTask) % _taskCount;
        }
    }

    public readonly struct DistractorTaskState
    {
        public readonly TrialsEnumerator TrialsEnumerator;
        public readonly int CurrentTaskGroup;

        public DistractorTaskState(TrialsEnumerator trialsEnumerator, int currentTaskGroup)
        {
            TrialsEnumerator = trialsEnumerator;
            CurrentTaskGroup = currentTaskGroup;
        }
    }
    
    
    
}