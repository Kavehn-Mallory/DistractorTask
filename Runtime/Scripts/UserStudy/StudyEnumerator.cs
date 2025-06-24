using System.Collections;
using System.Collections.Generic;
using DistractorTask.UserStudy.DataDrivenSetup;

namespace DistractorTask.UserStudy
{
    public class StudyEnumerator : IEnumerator<Study>
    {

        private int _currentStudyIndex = -1;
        private Study[] _studies;

        public int CurrentStudyIndex => _currentStudyIndex;

        public StudyEnumerator(Study[] studies)
        {
            _studies = studies;
        }
        
        
        public void Dispose()
        {
            
        }

        public bool MoveNext()
        {
            _currentStudyIndex++;
            return _currentStudyIndex < _studies.Length;
        }

        public void Reset()
        {
            _currentStudyIndex = -1;
        }

        public Study Current => _studies[_currentStudyIndex];

        object IEnumerator.Current => Current;
    }
}