using System.Collections.Generic;
using DistractorTask.Core;
using DistractorTask.Settings;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DistractorTask.Editor
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

        
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("Tools/DistractorTask", SettingsScope.User)
            {
                label = "Custom UI Elements",
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

                    properties.Add(new PropertyField(settings.FindProperty("useBootstrapper")));

                    rootElement.Bind(settings);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Distractor", "Bootstrapper" })
            };

            return provider;
        }
    }
}