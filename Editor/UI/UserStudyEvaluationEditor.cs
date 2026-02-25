using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DistractorTask.Editor.UI.CustomUI;
using DistractorTask.Editor.UI.DuplicatePropertyDrawer;
using DistractorTask.Logging;
using DistractorTask.UserStudy;
using DistractorTask.UserStudy.Core;
using DistractorTask.UserStudy.DataDrivenSetup;
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
        
        private UserStudyEvaluationTextBased _userStudyEvaluationTextBased;

        private ConditionTabFields _conditionTabFields;


        private string[] _paths = Array.Empty<string>();

        private TextField _loadedAssetField;
        private TextField _participantCount;
        private DropdownField _participantDropdownField;
        private VisualElement _warningArea;

        [SerializeField]
        private UserStudySettings userStudySettings;


        private List<UserStudyEvaluationTextBased.Duplicate> _duplicates;

        

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

            if (!userStudySettings)
            {
                Debug.Log("Creating settings");
                userStudySettings = AssetDatabase.LoadAssetAtPath<UserStudySettings>(
                    $"{DefaultSettingsPath}/UserStudySettings.asset") ?? ScriptableObject.CreateInstance<UserStudySettings>();

            }
            
            if (!EditorUtility.IsPersistent(settings))
            {
                Directory.CreateDirectory(DefaultSettingsPath);
                AssetDatabase.CreateAsset(settings, $"{DefaultSettingsPath}/UserStudyEvaluationSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (!EditorUtility.IsPersistent(userStudySettings))
            {
                Directory.CreateDirectory(DefaultSettingsPath);
                AssetDatabase.CreateAsset(userStudySettings, $"{DefaultSettingsPath}/UserStudySettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            CreateStudyEvaluationContainer();
            
            
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
            
            

            LoadStudyFiles(settings.lastOpenedPath);
            
        }

        private void CreateStudyEvaluationContainer()
        {
            _userStudyEvaluationTextBased = ScriptableObject.CreateInstance<UserStudyEvaluationTextBased>();
            rootVisualElement.Unbind();
            
            //SetupOverviewTab(ref this.rootVisualElement.Q<Tab>("Overview"));
            var serializedObject = new SerializedObject(_userStudyEvaluationTextBased);
            this.rootVisualElement.Bind(serializedObject);
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
            var settingsTab = tabView.Q<Tab>("Settings");
            
            Debug.Log($"Is null {overviewTab == null},{participantTab == null},{conditionTab == null}");

            SetupOverviewTab(ref overviewTab);
            SetupParticipantTab(participantTab);
            SetupConditionTab(conditionTab);
            SetupSettingsTab(settingsTab);


        }

        private void SetupSettingsTab(Tab settingsTab)
        {
            var propertyField = settingsTab.Q<PropertyField>();
            propertyField.dataSource = userStudySettings;
        }

        private void SetupOverviewTab(ref Tab overviewTab)
        {
            //todo binding here

            var so = new SerializedObject(_userStudyEvaluationTextBased);
            _loadedAssetField = overviewTab.Q<TextField>("LoadedAsset");
            //_loadedAssetField.value = _userStudyEvaluationTextBased.DateRange;
            _loadedAssetField.TrackPropertyValue(so.FindProperty(UserStudyEvaluationTextBased.DateRangeFieldName), ev => _loadedAssetField.value = _userStudyEvaluationTextBased.DateRange);
            //_loadedAssetField.bindingPath = UserStudyEvaluationTextBased.DateRangeFieldName;
            _participantCount = overviewTab.Q<TextField>("NoOfParticipants");
            var property = so.FindProperty(UserStudyEvaluationTextBased
                .UserIdsArrayFieldName);
            _participantCount.TrackPropertyValue(property, OnFilePathsChanged);
            //participantCount.bindingPath = nameof(RuntimeStudyData.userIds.Length);


            _warningArea = overviewTab.Q<VisualElement>("WarningArea");
        }

        private void OnFilePathsChanged(SerializedProperty obj)
        {
            _participantCount.value = obj.arraySize.ToString();
            var currentIndex = _participantDropdownField.index;
            _participantDropdownField.choices = _userStudyEvaluationTextBased.UserIds;
            _participantDropdownField.index = math.clamp(currentIndex, 0, _participantDropdownField.choices.Count);
            //_loadedAssetField.value = _userStudyEvaluationTextBased.DateRange;

            _duplicates = _userStudyEvaluationTextBased.FindDuplicates();

            if (_duplicates.Count > 0)
            {
                _warningArea.Add(new Label($"There are {_duplicates.Count} duplicates in the data set"));
                var listView = new ListView(_duplicates);
                listView.makeItem = () => new DuplicateField();
                listView.bindItem = BindItem;
                _warningArea.Add(listView);
                _warningArea.Add(new VisualElement
                {
                    style = { height = 20}
                });

            }

            var trialCount =
                _userStudyEvaluationTextBased.CheckTrialCountDataForStudies(_userStudyEvaluationTextBased.UserIds
                    .ToArray(),userStudySettings.studies.Length);
                
                
            var headerElements = new string[trialCount.Length + 1];
            headerElements[0] = "Default";

            for (int i = 0; i < trialCount.Length; i++)
            {
                headerElements[i + 1] = trialCount[i].Id;
            }

                
            var header = new RowElement(headerElements);
                
            _warningArea.Add(header);

            for (int i = 0; i < userStudySettings.studies.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Calculating Trial Count per Study", $"{i} / {userStudySettings.studies.Length}", (float)i / userStudySettings.studies.Length);
                for (int duplicate = 0; duplicate < trialCount.Length; duplicate++)
                {
                    var numberOfTrials = trialCount[duplicate].Trials.Length > i
                        ? trialCount[duplicate].Trials[i].ToString()
                        : "0";
                    headerElements[duplicate + 1] = numberOfTrials;
                }

                var trialCountTarget = PermutationGenerator.CalculateTrialCount(userStudySettings.studies[i])
                    .ToString();
                headerElements[0] = trialCountTarget;
                _warningArea.Add(new RowElement(headerElements));
            }
            
            EditorUtility.ClearProgressBar();
            
        }


        private void BindItem(VisualElement element, int i)
        {
            var item = _duplicates[i];
            (element as DuplicateField)?.SetDuplicate(item);
        }

        private void SetupParticipantTab(Tab participantTab)
        {
            _participantDropdownField = participantTab.Q<DropdownField>("ParticipantSelector");
            _participantDropdownField.bindingPath = UserStudyEvaluationTextBased.UserIdsArrayFieldName;
            _participantDropdownField.RegisterValueChangedCallback(OnParticipantChanged);
        }

        private void OnParticipantChanged(ChangeEvent<string> evt)
        {
            UpdateStudies(evt.newValue);
        }

        private void UpdateStudies(string userId)
        {
            //throw new NotImplementedException();
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
            toolbarMenu.menu.AppendAction("Load Study Log Files - Text Only", OnLoadLogFilesOnlyText);
            toolbarMenu.menu.AppendAction("Generate Python Files", GeneratePythonFiles);
            toolbarMenu.menu.AppendAction("Generate Error Rate Python File", GenerateErrorRatePythonFile);
            toolbarMenu.menu.AppendAction("Generate Task Performance over Time Python File", GenerateTaskPerformanceOverTimeFile);
            toolbarMenu.menu.AppendAction("Generate Python File for each unique condition", GenerateFilesForStudyTrialConditionCases);
        }

        private void GenerateErrorRatePythonFile(DropdownMenuAction obj)
        {
            NormalizedCsvGenerator.GenerateErrorRateCSVFiles(_userStudyEvaluationTextBased.FilePaths);
        }

        private void GenerateTaskPerformanceOverTimeFile(DropdownMenuAction obj)
        {
            NormalizedCsvGenerator.GenerateTaskPerformanceOverTimeCSVFile(_userStudyEvaluationTextBased.FilePaths);
        }

        private void GenerateFilesForStudyTrialConditionCases(DropdownMenuAction obj)
        {
            NormalizedCsvGenerator.GeneratePerStudyConditionCSVFiles(_userStudyEvaluationTextBased.FilePaths);
        }

        private void GeneratePythonFiles(DropdownMenuAction obj)
        {
            NormalizedCsvGenerator.GenerateCSVFilesForPython(_userStudyEvaluationTextBased.FilePaths);
            NormalizedCsvGenerator.GenerateErrorRateCSVFiles(_userStudyEvaluationTextBased.FilePaths);
            NormalizedCsvGenerator.GenerateTaskPerformanceOverTimeCSVFile(_userStudyEvaluationTextBased.FilePaths);
        }

        private void OnLoadLogFilesOnlyText(DropdownMenuAction obj)
        {
            var path = EditorUtility.OpenFolderPanel("Select folder that contains the logfiles", Application.persistentDataPath,
                "");
            
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            settings.lastOpenedPath = path;
            
            
            LoadStudyFiles(path);

        }

        private void LoadStudyFiles(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return;
            }
            var files = Directory.GetFiles(folderPath);

            _paths = files.Where(file => Path.GetExtension(file) == ".csv").ToArray();

            _userStudyEvaluationTextBased.SetFilePaths(_paths, userStudySettings.validIds);
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
