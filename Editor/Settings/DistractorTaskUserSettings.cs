#if UNITY_EDITOR
using DistractorTask.Core;
using DistractorTask.Settings;
using UnityEditor;
using UnityEngine;

namespace DistractorTask.Editor.Settings
{
    [FilePath(Constants.UserSettingsPath, FilePathAttribute.Location.ProjectFolder)]
    public class DistractorTaskUserSettings : BaseSettings<DistractorTaskUserSettings>
    {
        [SerializeField]
        private bool useBootstrapper = true;

        public bool UseBootstrapper
        {
            get => useBootstrapper;
            set
            {
                useBootstrapper = value;
                Save();
            }
        }
        

    }
}
#endif

