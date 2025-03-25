#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DistractorTask.Settings
{
    [FilePath("UserSettings/Packages/com.janwittke.distractortask/DistractorTaskSettings.asset", FilePathAttribute.Location.ProjectFolder)]
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

