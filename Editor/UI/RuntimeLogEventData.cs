using UnityEngine;

namespace DistractorTask.Editor.UI
{
    public class RuntimeLogEventData : ScriptableObject
    {
        public string userId;
        public string timeStamp;
        public UserStudyEvaluationEditor.LogEvent[] logEvents;
    }
}