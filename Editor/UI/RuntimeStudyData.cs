using System.Collections.Generic;
using UnityEngine;

namespace DistractorTask.Editor.UI
{
    public class RuntimeStudyData : ScriptableObject
    {
        public List<RuntimeLogEventData> logFiles;
        public UserStudySettings userStudySettings;
    }
}