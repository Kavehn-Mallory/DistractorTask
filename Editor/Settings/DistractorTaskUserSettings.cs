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

        [SerializeField]
        private bool generateUserId = false;

        [SerializeField]
        private DistractorTaskSettingsAsset settingsAsset;

        public bool GenerateUserId
        {
            get => generateUserId;
            set
            {
                generateUserId = value;
                if (settingsAsset)
                {
                    settingsAsset.generateUserId = value;
                }
                Save();
            }
        }

        public bool UseBootstrapper
        {
            get => useBootstrapper;
            set
            {
                useBootstrapper = value;
                Save();
            }
        }

        public DistractorTaskSettingsAsset SettingsAsset
        {
            get => settingsAsset;
            set
            {
                settingsAsset = value;
                DistractorTaskSettingsAsset.Instance = settingsAsset;
                Save();
            }
        }

        public static string GenerateUserIdSettingName => nameof(generateUserId);
        public static string UseBootstrapperSettingName => nameof(useBootstrapper);
        
        public static string DistractorTaskSettingsSettingName => nameof(settingsAsset);


    }
}
#endif

