using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DistractorTask.Editor.Settings
{
    public static class DistractorProjectSettingsProviderUIElementsRegister
    {


        private const string StyleSheetPath = "Packages/com.janwittke.distractortask/Editor/settings_ui.uss";
        
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("Tools/DistractorTask", SettingsScope.Project)
            {
                label = "Distractor Task",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = DistractorTaskProjectSettings.GetSerializedSettings();

                    // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                    // isn't called because the SettingsProvider uses the UIElements drawing framework.
                    var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
                    rootElement.styleSheets.Add(styleSheet);
                    var title = new Label()
                    {
                        text = "Port Settings"
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

                    properties.Add(new PropertyField(settings.FindProperty("settings.defaultPort")));
                    properties.Add(new PropertyField(settings.FindProperty("settings.ipListeningPort")));
                    properties.Add(new PropertyField(settings.FindProperty("settings.loggingPort")));
                    properties.Add(new PropertyField(settings.FindProperty("settings.videoPlayerPort")));

                    rootElement.Bind(settings);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Distractor", "Ports" })
            };

            return provider;
        }
    }
}