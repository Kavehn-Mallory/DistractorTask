using System.Collections.Generic;
using DistractorTask.Core;
using DistractorTask.Settings;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DistractorTask.Editor.Settings
{
    public static class DistractorUserSettingsProviderUIElementsRegister
    {
        
        [MenuItem("Tools/" + Constants.MenuPrefixBase + "/Toggle Bootstrapper", true)]
        private static bool SetToggleState()
        {
            Menu.SetChecked("Tools/" + Constants.MenuPrefixBase + "/Toggle Bootstrapper", DistractorTaskUserSettings.instance.UseBootstrapper);
            return true;
        }

        [MenuItem("Tools/" + Constants.MenuPrefixBase + "/Toggle Bootstrapper")]
        public static void ToggleBootstrapper()
        {
            DistractorTaskUserSettings.instance.UseBootstrapper = !DistractorTaskUserSettings.instance.UseBootstrapper;
        }
        
        [MenuItem("Tools/" + Constants.MenuPrefixBase + "/Toggle Generate UserID", true)]
        private static bool SetGenerateIdToggleState()
        {
            Menu.SetChecked("Tools/" + Constants.MenuPrefixBase + "/Toggle Generate UserID", DistractorTaskUserSettings.instance.GenerateUserId);
            return true;
        }

        [MenuItem("Tools/" + Constants.MenuPrefixBase + "/Toggle Generate UserID")]
        public static void ToggleGenerateUserId()
        {
            DistractorTaskUserSettings.instance.GenerateUserId = !DistractorTaskUserSettings.instance.GenerateUserId;
        }

        
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the User scope.
            var provider = new SettingsProvider("Tools/DistractorTask", SettingsScope.User)
            {
                label = "Distractor Task",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = DistractorTaskUserSettings.GetSerializedSettings();

                    // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                    // isn't called because the SettingsProvider uses the UIElements drawing framework.
                    var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Constants.StyleSheetPath);
                    rootElement.styleSheets.Add(styleSheet);
                    var title = new Label()
                    {
                        text = "Distractor Task Settings"
                    };
                    
                    title.AddToClassList("title");
                    rootElement.Add(title);

                    var properties = new VisualElement()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Column,
                            marginLeft = 15
                        }
                    };
                    properties.AddToClassList("property-list");
                    rootElement.Add(properties);

                    properties.Add(new PropertyField(settings.FindProperty(DistractorTaskUserSettings.UseBootstrapperSettingName)));
                    properties.Add(new PropertyField(settings.FindProperty(DistractorTaskUserSettings.GenerateUserIdSettingName)));
                    properties.Add(new PropertyField(settings.FindProperty(DistractorTaskUserSettings.DistractorTaskSettingsSettingName)));
                    
                    properties.TrackPropertyValue(settings.FindProperty(DistractorTaskUserSettings.DistractorTaskSettingsSettingName), UpdateGlobalSettingsInstance);

                    properties.TrackPropertyValue(settings.FindProperty(DistractorTaskUserSettings.GenerateUserIdSettingName), UpdateSettingsAsset);

                    rootElement.Bind(settings);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Distractor", "Bootstrapper" })
            };

            return provider;
        }

        private static void UpdateGlobalSettingsInstance(SerializedProperty obj)
        {
            var settings = DistractorTaskUserSettings.GetSerializedSettings();
            var objectRef = settings.FindProperty((DistractorTaskUserSettings.DistractorTaskSettingsSettingName))
                .objectReferenceValue;
            if (!objectRef || objectRef is not DistractorTaskSettingsAsset settingsAsset)
            {
                return;
            }

            DistractorTaskSettingsAsset.Instance = settingsAsset;
        }

        private static void UpdateSettingsAsset(SerializedProperty obj)
        {
            var settings = DistractorTaskUserSettings.GetSerializedSettings();
            var objectRef = settings.FindProperty((DistractorTaskUserSettings.DistractorTaskSettingsSettingName))
                .objectReferenceValue;
            if (!objectRef)
            {
                return;
            }
            var asset = new SerializedObject(objectRef);

            asset.FindProperty("generateUserId").boolValue = obj.boolValue;
            asset.ApplyModifiedProperties();
            settings.ApplyModifiedProperties();
            AssetDatabase.SaveAssetIfDirty(objectRef);

        }
    }
}