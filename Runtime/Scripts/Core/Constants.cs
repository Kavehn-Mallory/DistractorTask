namespace DistractorTask.Core
{
    public static class Constants
    {
        public const string UserSettingsPath =
            "UserSettings/Packages/" + PackageNameReverseDomain + "/DistractorTaskSettings.asset";
        
        public const string ProjectSettingsPath =
            "ProjectSettings/Packages/" + PackageNameReverseDomain + "/DistractorTaskSettings.asset";

        public const string PackageNameReverseDomain = "com.janwittke.distractortask";

        public const string MenuPrefixBase = "Distractor Task";
        
        public const string StyleSheetPath = "Packages/" + PackageNameReverseDomain + "/Editor/settings_ui.uss";

        public const string RuntimeSettingsPath = "Assets/Settings/" + PackageNameReverseDomain;

        public const string RuntimeSettingsFileName = "RuntimeUserSettings.asset";

        public const string RuntimeSettingsFullPath = RuntimeSettingsPath + "/" + RuntimeSettingsFileName;

    }
}