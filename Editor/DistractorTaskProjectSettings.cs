using DistractorTask.Core;
using DistractorTask.Settings;
using UnityEditor;
using UnityEngine;

namespace DistractorTask.Editor
{
    
    [FilePath(Constants.ProjectSettingsPath, FilePathAttribute.Location.ProjectFolder)]
    public class DistractorTaskProjectSettings : BaseSettings<DistractorTaskProjectSettings>
    {

        [SerializeField]
        private UserSettings settings = new();

        public UserSettings Settings => settings;
    }
}