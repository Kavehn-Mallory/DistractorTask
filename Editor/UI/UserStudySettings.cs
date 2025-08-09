using System;
using DistractorTask.UserStudy.DataDrivenSetup;
using UnityEngine;

namespace DistractorTask.Editor.UI
{
    [Serializable]
    public class UserStudySettings : ScriptableObject
    {
        public Study[] studies = Array.Empty<Study>();
    }
}