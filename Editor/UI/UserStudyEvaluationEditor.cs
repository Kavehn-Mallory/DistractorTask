using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DistractorTask.Logging;
using DistractorTask.UserStudy.Core;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DistractorTask.Editor.UI
{
    public class UserStudyEvaluationEditor : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset visualTreeAsset = default;

        [SerializeField]
        private UserStudyEvaluationSettings settings;

        private RuntimeStudyData _currentlyActiveStudyData;

        public RuntimeStudyData CurrentlyActiveStudyData
        {
            get => _currentlyActiveStudyData;
            set
            {
                _currentlyActiveStudyData = value;
                OnRuntimeStudyDataChanged();
            }
        }

        private RuntimeStudyData _emptyStudyData;

        //private UserStudyEvaluation _userStudyEvaluation;
        private UserStudyEvaluationTextBased _userStudyEvaluationTextBased;

        private ConditionTabFields _conditionTabFields;


        private string[] _paths = Array.Empty<string>();

        

        private const string DefaultSettingsPath = "Assets/DistractorTask/UserStudyEvaluationSettings";
        

        [MenuItem("Window/DistractorTask/UserStudyEvaluation")]
        public static void ShowExample()
        {
            UserStudyEvaluationEditor wnd = GetWindow<UserStudyEvaluationEditor>();
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

            CreateEmptyStudySettings();
            
            
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
            else
            {
                Debug.Log("No tab view found");
            }
            
            

            LoadGeneratedStudyData(settings.lastOpenedPath);
            
        }

        private void CreateEmptyStudySettings()
        {
            _emptyStudyData = ScriptableObject.CreateInstance<RuntimeStudyData>();
            _emptyStudyData.logFiles = new List<RuntimeLogEventData>();
            _emptyStudyData.userIds = Array.Empty<string>();
            _emptyStudyData.userStudySettings = ScriptableObject.CreateInstance<UserStudySettings>();
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
            
            Debug.Log($"Is null {overviewTab == null},{participantTab == null},{conditionTab == null}");

            SetupOverviewTab(ref overviewTab);
            SetupParticipantTab(participantTab);
            SetupConditionTab(conditionTab);


        }
        
        private void SetupOverviewTab(ref Tab overviewTab)
        {
            Debug.Log($"is it null now: {overviewTab == null}");
            var loadedAssetNameLabel = overviewTab.Q<TextField>("LoadedAsset");
            loadedAssetNameLabel.bindingPath = nameof(RuntimeStudyData.userStudySettings);
            var participantCount = overviewTab.Q<TextField>("NoOfParticipants");
            participantCount.bindingPath = nameof(RuntimeStudyData.userIds.Length);
        }
        
        private void SetupParticipantTab(Tab participantTab)
        {
            var participantDropdown = participantTab.Q<DropdownField>("ParticipantSelector");
            participantDropdown.bindingPath = nameof(RuntimeStudyData.userIds);
            participantDropdown.RegisterValueChangedCallback(OnParticipantChanged);
        }

        private void OnParticipantChanged(ChangeEvent<string> evt)
        {
            UpdateStudies(evt.newValue);
        }

        private void UpdateStudies(string userId)
        {
            throw new NotImplementedException();
        }

        private void SetupConditionTab(Tab conditionTab)
        {
            Debug.Log("Setting up conditions graph");
            _conditionTabFields = new ConditionTabFields
            {
                LoadLevelField = conditionTab.Q<EnumField>("LoadLevelSettings"),
                NoiseLevelField = conditionTab.Q<EnumField>("NoiseLevelSettings"),
                ReactionTimeBoxPlot = conditionTab.Q<BoxPlot>("ReactionTimeBoxPlot"),
                AccuracyPieChart = conditionTab.Q<PieChart>("AccuracyPieChart"),
                ReactionTimeOverTimeGraph = conditionTab.Q<Graph2D>("ReactionTimeOverTimeForCondition"),
                ReactionTimeOverTimePerMarkerPoint = conditionTab.Q<Graph2D>("ReactionTimeOverTimePerMarkerPoint")
            };
            
            Debug.Log($"Condition Tab Content exists: {_conditionTabFields.LoadLevelField != null}, {_conditionTabFields.NoiseLevelField != null}, {_conditionTabFields.ReactionTimeBoxPlot != null}, {_conditionTabFields.AccuracyPieChart != null}, {_conditionTabFields.ReactionTimeOverTimeGraph != null}, {_conditionTabFields.ReactionTimeOverTimePerMarkerPoint != null}");
            
            _conditionTabFields.LoadLevelField.RegisterValueChangedCallback(OnConditionChanged);
            _conditionTabFields.NoiseLevelField.RegisterValueChangedCallback(OnConditionChanged);
        }
        
        private void OnConditionChanged(ChangeEvent<Enum> evt)
        {
            var loadLevel = (LoadLevel)_conditionTabFields.LoadLevelField.value;
            var noiseLevel = (NoiseLevel)_conditionTabFields.NoiseLevelField.value;
            
            var reactionTimeData = _userStudyEvaluationTextBased.CalculateReactionTimeForCondition(loadLevel, noiseLevel);

            _conditionTabFields.ReactionTimeOverTimeGraph.ClearGraph();
            _conditionTabFields.ReactionTimeOverTimePerMarkerPoint.ClearGraph();
            
            if (reactionTimeData.Length == 0)
            {
                Debug.Log("No data");
                return;
            }
            Debug.Log($"Found {reactionTimeData.Length} data points");
            var timings = new float[reactionTimeData.Length];
            
            var lastTime = timings[0] - 1;

            var minTime = timings[0];
            var maxTime = minTime;

            var maxDuration = 0f;

            var points = new List<Vector2>();
            for (int i = 0; i < timings.Length; i++)
            {
                var element = reactionTimeData[i];
                minTime = math.min(minTime, element.Time);
                maxTime = math.max(maxTime, element.Time);
                maxDuration = math.max(maxDuration, element.Duration);

                timings[i] = element.Duration;
                
                if (element.Time < lastTime)
                {
                    //todo begin new thing 
                    _conditionTabFields.ReactionTimeOverTimeGraph.AddGraph(points.ToArray());
                    points.Clear();
                }

                lastTime = element.Time;
                
                points.Add(new Vector2(element.Time, element.Duration));
                
                
            }

            _conditionTabFields.ReactionTimeOverTimeGraph.RangeXAxis = new Vector2(minTime, maxTime);
            _conditionTabFields.ReactionTimeOverTimeGraph.RangeYAxis = new Vector2(0, maxDuration);
            _conditionTabFields.ReactionTimeBoxPlot.Values = timings;

        }
        

        


        private void SetupToolbar(Toolbar toolbar)
        {
            var toolbarMenu = toolbar.Q<ToolbarMenu>("File");
            toolbarMenu.menu.AppendAction("Load Study Log Files", OnLoadLogFiles);
            toolbarMenu.menu.AppendAction("Load Study Log Files - Text Only", OnLoadLogFilesOnlyText);
            toolbarMenu.menu.AppendAction("Load Generated Study Data", OnLoadGeneratedStudyData);
        }

        private void OnLoadLogFilesOnlyText(DropdownMenuAction obj)
        {
            var path = EditorUtility.OpenFolderPanel("Select folder that contains the logfiles", Application.persistentDataPath,
                "");
            
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            
            var files = Directory.GetFiles(path);

            _paths = files.Where(file => Path.GetExtension(file) == ".csv").ToArray();

            _userStudyEvaluationTextBased = new UserStudyEvaluationTextBased(_paths);


        }

        private void OnLoadGeneratedStudyData(DropdownMenuAction obj)
        {
            var path = EditorUtility.OpenFilePanel("Select Generated Study Data", "Assets", "asset");
            Debug.Log(path);
            LoadGeneratedStudyData(path);
        }

        private void LoadGeneratedStudyData(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var test = AssetDatabase.LoadAssetAtPath<RuntimeStudyData>("Assets/DistractorTask/GeneratedData/GeneratedUserStudyData.asset");
            Debug.Log(test.logFiles.Count);
            CurrentlyActiveStudyData = test;
            
            
        }
        
        private void OnRuntimeStudyDataChanged()
        {
            rootVisualElement.Unbind();
            
            if (!_currentlyActiveStudyData)
            {
                Debug.Log("Did not find anything");
                _currentlyActiveStudyData = _emptyStudyData;
            }

            
            //SetupOverviewTab(ref this.rootVisualElement.Q<Tab>("Overview"));
            var serializedObject = new SerializedObject(_currentlyActiveStudyData);
            this.rootVisualElement.Bind(serializedObject);

            Debug.Log($"Loading _current study {_currentlyActiveStudyData.logFiles.Count}");
            //_userStudyEvaluation = new UserStudyEvaluation(_currentlyActiveStudyData);
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

            _currentlyActiveStudyData = parent;

            if (settings)
            {
                settings.lastOpenedPath = targetPath;
            }

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
                    logData = LogData.LoadLogDataFromCsvLine(text[0], text[i])
                };
            }
            
            return result;
        }
        
        private static TimeSpan ReadTimeStamp(string v)
        {
            return DateTime.Parse(v).TimeOfDay;
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
            public SerializableTimeSpan timeStamp;
            public LogData logData;
        }

        public class ConditionTabFields
        {
            public EnumField NoiseLevelField;
            public EnumField LoadLevelField;
            public BoxPlot ReactionTimeBoxPlot;
            public PieChart AccuracyPieChart;
            public Graph2D ReactionTimeOverTimeGraph;
            public Graph2D ReactionTimeOverTimePerMarkerPoint;

        }


    }
}
