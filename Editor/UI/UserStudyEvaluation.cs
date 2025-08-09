using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DistractorTask.Logging;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DistractorTask.Editor.UI
{
    public class UserStudyEvaluation : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset visualTreeAsset = default;

        [SerializeField]
        private UserStudyEvaluationSettings settings;

        private const string DefaultSettingsPath = "Assets/DistractorTask/UserStudyEvaluationSettings";

        [MenuItem("Window/DistractorTask/UserStudyEvaluation")]
        public static void ShowExample()
        {
            UserStudyEvaluation wnd = GetWindow<UserStudyEvaluation>();
            wnd.titleContent = new GUIContent("UserStudyEvaluation");
        }

        public void CreateGUI()
        {
            if (!settings)
            {
                Debug.Log("Creating settings");
                settings = AssetDatabase.LoadAssetAtPath<UserStudyEvaluationSettings>(
                    $"{DefaultSettingsPath}/UserStudyEvaluationSettings.asset") ?? ScriptableObject.CreateInstance<UserStudyEvaluationSettings>();
                
            }
            if (!EditorUtility.IsPersistent(settings))
            {
                Directory.CreateDirectory(DefaultSettingsPath);
                AssetDatabase.CreateAsset(settings, $"{DefaultSettingsPath}/UserStudyEvaluationSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            

            // Instantiate UXML
            VisualElement labelFromUXML = visualTreeAsset.Instantiate();
            root.Add(labelFromUXML);
            var toolbar = root.Q<Toolbar>();
            if (toolbar != null)
            {
                SetupToolbar(toolbar);
            }
            
            var tabView = root.Q<TabView>();

            if (tabView != null)
            {
                SetupTabs(tabView);

                tabView.activeTabChanged += OnActiveTabChanged;
            }
            
            

            LoadGeneratedStudyData(settings.lastOpenedPath);
            
        }

        private void OnActiveTabChanged(Tab oldTab, Tab newTab)
        {
            //throw new System.NotImplementedException();
        }

        private void SetupTabs(TabView tabView)
        {
            var overviewTab = tabView.Q<Tab>("Overview");
            var participantTab = tabView.Q<Tab>("PerParticipant");
            var conditionTab = tabView.Q<Tab>("PerCondition");
            
            
        }

        private void SetupToolbar(Toolbar toolbar)
        {
            var toolbarMenu = toolbar.Q<ToolbarMenu>("File");
            toolbarMenu.menu.AppendAction("Load Study Log Files", OnLoadLogFiles);
            toolbarMenu.menu.AppendAction("Load Generated Study Data", OnLoadGeneratedStudyData);
        }

        private void OnLoadGeneratedStudyData(DropdownMenuAction obj)
        {
            throw new NotImplementedException();
        }

        private void LoadGeneratedStudyData(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
        }

        private void OnLoadLogFiles(DropdownMenuAction dropdownMenuAction)
        {
            var path = EditorUtility.OpenFolderPanel("Select folder that contains the logfiles", Application.persistentDataPath,
                "");
            
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            var targetPath = EditorUtility.SaveFilePanelInProject("Select target location for generated study data", "Assets", "asset", "asset");

            if (string.IsNullOrEmpty(targetPath))
            {
                return;
            }
            
            var files = Directory.GetFiles(path);

            var parent = ScriptableObject.CreateInstance<RuntimeStudyData>();
            parent.logFiles = new List<RuntimeLogEventData>();
            Debug.Log(targetPath);
            AssetDatabase.CreateAsset(parent, $"{targetPath}");
            AssetDatabase.SaveAssets();

            var failedAssets = 0;
            for (var i = 0; i < files.Length; i++)
            {
                var file = files[i];
                EditorUtility.DisplayProgressBar("Generating files", $"Creating file {i + 1} out of {files.Length}",
                    ((float)i) / files.Length);
                if (Path.GetExtension(file) != ".csv")
                {
                    continue;
                }

                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var fileNameParts = fileName.Split('_');

                    var logEvents = LoadLogFile(file);

                    var runtimeLogEventData = ScriptableObject.CreateInstance<RuntimeLogEventData>();

                    runtimeLogEventData.logEvents = logEvents;
                    runtimeLogEventData.timeStamp = fileNameParts[0];
                    runtimeLogEventData.userId = fileNameParts[1];
                    runtimeLogEventData.name = fileName;

                    
                    parent.logFiles.Add(runtimeLogEventData);


                    AssetDatabase.AddObjectToAsset(runtimeLogEventData, parent);
                }
                catch (Exception e)
                {
                    failedAssets++;
                    Debug.Log($"{e}. \n Failed to generate data from {file}");
                }
            }
            EditorUtility.ClearProgressBar();

            var studySettings = ScriptableObject.CreateInstance<UserStudySettings>();
            parent.userStudySettings = studySettings;
            studySettings.name = "UserStudyConfiguration";
            AssetDatabase.AddObjectToAsset(studySettings, parent);
            UserStudySettingsEditor.ShowWindow(studySettings);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Failed to create {failedAssets} assets");
        }
        
        private static LogEvent[] LoadLogFile(string path)
        {
            var text = File.ReadAllLines(path);
            var result = new LogEvent[text.Length - 1];

            for (var i = 1; i < text.Length; i++)
            {
                result[i - 1] = new LogEvent
                {
                    timeStamp = ReadTimeStamp(text[i].Split(';')[0]),
                    logData = LogData.LoadLogDataFromCSVLine(text[0], text[i])
                };
            }
            
            return result;
        }
        
        private static TimeSpan ReadTimeStamp(string v)
        {
            if (DateTime.TryParse(v, out var timeStamp))
            {
                return timeStamp.TimeOfDay;
            }

            return DateTime.Now.TimeOfDay;
            //return TimeSpan.Parse(v, "");
        }
        
        [Serializable]
        public struct LogEvent
        {
            public TimeSpan timeStamp;
            public LogData logData;
        }

        [Serializable]
        public struct SerializableLogEvent
        {
            public SerializableTimeSpan timeStamp;
            
        }
    }
}
