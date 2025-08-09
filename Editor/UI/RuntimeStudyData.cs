using System;
using System.Collections.Generic;
using UnityEngine;

namespace DistractorTask.Editor.UI
{
    public class RuntimeStudyData : ScriptableObject
    {
        [SerializeField]
        public List<RuntimeLogEventData> logFiles;

        [HideInInspector, SerializeField]
        public string[] userIds = Array.Empty<string>();
        
        [SerializeField]
        public UserStudySettings userStudySettings;

        [ContextMenu("GenerateUserIds")]
        public void GenerateUserIds()
        {
            userIds = new string[logFiles.Count];

            for (int i = 0; i < logFiles.Count; i++)
            {
                userIds[i] = logFiles[i].userId;
            }
        }
    }
}