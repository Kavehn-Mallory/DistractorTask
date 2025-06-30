using DistractorTask.Core;
using DistractorTask.Logging.Components;
using DistractorTask.Settings;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using DistractorTask.UserStudy.DataDrivenSetup;
using DistractorTask.UserStudy.DistractorSelectionStage;
using DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents;
using DistractorTask.VideoPlayer;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DistractorTask.Logging
{
    public class LoggingComponent : MonoBehaviour
    {

        [SerializeField]
        private bool isServer;
        

        private ControlPanel _controlPanel;
        private DistractorSelectionComponent _selectionComponent;
        private VideoPlayerController _videoPlayerController;

        [SerializeField, ReadOnly]
        private string userId;

        private const int IdLength = 8;


        [ContextMenu("Generate new UserId")]
        private void GenerateNewId()
        {
            userId = GenerateUserId();
        }
        
        [ContextMenu("Clear UserId")]
        private void ClearUserId()
        {
            userId = "";
        }

        private string GenerateUserId()
        {
            string characters = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz1234567890";
            string generatedString = "";

            for(int i = 0; i < IdLength; i++)
                generatedString += characters[Random.Range(0, characters.Length)];

            return generatedString;
        }


        private void Start()
        {
            if (DistractorTaskSettingsAsset.Instance && DistractorTaskSettingsAsset.Instance.generateUserId && userId == string.Empty)
            {
                GenerateNewId();
            }
            
            if (isServer)
            {
                StudyLog.UserId = userId;
                NetworkManager.Instance.StartListening(NetworkExtensions.LoggingPort, OnConnectionStateChanged);
                NetworkManager.Instance.RegisterCallback<LogFileData>(OnLogFileDataReceived, NetworkExtensions.LoggingPort);

            }
            else
            {
                var portListeners = FindObjectsByType<ConnectionPortListener>(FindObjectsSortMode.None);

                foreach (var portListener in portListeners)
                {
                    portListener.OnDataReceived += OnIpDataReceived;
                }
            }
            StudyLog.RegisterLog<MarkerPointCountData>();
            StudyLog.RegisterLog<StudyConditionVideoInfoData>();
            StudyLog.RegisterLog<LogfileDataOld>();
            
            StudyLog.LogCustomKeyframe(Transport.DataContainer.LogCategoryOld.Default, "LogFile Start");
            
            RegisterStudyComponentEvents();
        }

        private void OnLogFileDataReceived(LogFileData logFileData, int arg2)
        {
            
        }

        private void OnHeadPositionDataReceived(HeadPositionData arg1, int arg2)
        {
            throw new System.NotImplementedException();
        }
        
        private void OnEyetrackingDataReceived(EyetrackingLookDirection arg1, int arg2)
        {
            throw new System.NotImplementedException();
        }

        private void OnDisable()
        {
            StudyLog.DisposeLogSystem();
        }


        private void RegisterStudyComponentEvents()
        {
            _controlPanel = FindAnyObjectByType<ControlPanel>();
            _selectionComponent = FindAnyObjectByType<DistractorSelectionComponent>();
            _videoPlayerController = FindAnyObjectByType<VideoPlayerController>();
            if (_controlPanel)
            {
                _controlPanel.OnStudyPhaseStart += OnStudyPhaseStart;
                _controlPanel.OnNextIteration += OnNextIteration;
                _controlPanel.OnStudyPhaseEnd += OnStudyPhaseEnd;
                _controlPanel.OnStudyLog += OnStudyLog;
                _controlPanel.OnStudyCompleted += OnStudyCompleted;
            }

            if (_selectionComponent)
            {
                _selectionComponent.OnDistractorSelection += OnDistractorSelected;
            }

            if (_videoPlayerController)
            {
                _videoPlayerController.OnVideoClipSelected += OnVideoClipSelected;
                _videoPlayerController.OnVideoClipReset += OnVideoClipReset;
            }
            
            NetworkManager.Instance.RegisterCallbackAllPorts<HeadPositionData>(OnHeadPositionDataReceived);
            NetworkManager.Instance.RegisterCallbackAllPorts<EyetrackingLookDirection>(OnEyetrackingDataReceived);
        }

        private static void OnVideoClipReset()
        {
            StudyLog.LogCustomKeyframe(Transport.DataContainer.LogCategoryOld.VideoPlayer, $"Video Clip Reset");
        }

        private static void OnVideoClipSelected(string videoClipName, string audioClipName)
        {
            StudyLog.LogCustomKeyframe(Transport.DataContainer.LogCategoryOld.VideoPlayer, $"New Video Clip selected: {videoClipName} with audio {audioClipName}");
        }

        private static void OnDistractorSelected(DistractorTaskComponent.DistractorSelectionResult result)
        {
            StudyLog.LogCustomKeyframe(Transport.DataContainer.LogCategoryOld.UserStudy, GenerateDistractorSelectionResponse(result));
        }

        private static string GenerateDistractorSelectionResponse(DistractorTaskComponent.DistractorSelectionResult result)
        {
            if (result.WasSuccessful())
            {
                return $"Correct Distractor {result.selectedDistractor} was selected";
            }

            if (result.selectedDistractor == -1)
            {
                return $"No Distractor was selected instead of {result.targetDistractor}";
            }
            return $"Incorrect Distractor {result.selectedDistractor} was selected instead of {result.targetDistractor}";
        }

        private void OnStudyCompleted()
        {
            StudyLog.LogCustomKeyframe(Transport.DataContainer.LogCategoryOld.UserStudy, $"Study was completed for user {userId}");
        }

        private void OnStudyLog(Transport.DataContainer.LogCategoryOld studyCategoryOld, string message)
        {
            StudyLog.LogCustomKeyframe(studyCategoryOld, message);
        }

        private void OnStudyPhaseEnd(string studyPhaseName)
        {
            StudyLog.LogCustomKeyframe(Transport.DataContainer.LogCategoryOld.UserStudy, $"Ending {studyPhaseName}");
        }

        private static void OnNextIteration(string studyStage, int markerPointIndex, int markerPointCount)
        {
            StudyLog.LogCustomKeyframe(Transport.DataContainer.LogCategoryOld.UserStudy, $"Starting {studyStage} {markerPointIndex} of {markerPointCount}");
        }

        private static void OnStudyPhaseStart(string studyPhaseName)
        {
            StudyLog.LogCustomKeyframe(Transport.DataContainer.LogCategoryOld.UserStudy, $"Starting {studyPhaseName}");
        }

        private void OnIpDataReceived(IpAddressData ipAddressData)
        {
            var loggingEndpoint = ipAddressData.Endpoint.WithPort(NetworkExtensions.LoggingPort);
            NetworkManager.Instance.Connect(loggingEndpoint, OnConnectionEstablished);
        }

        private void OnConnectionEstablished(ConnectionState obj)
        {
            Debug.Log("Sending log data to server from now on");
        }

        private void OnConnectionStateChanged(ConnectionState obj)
        {
            if (obj == ConnectionState.Connected)
            {
                Debug.Log("Connection established!");
                return;
            }
            Debug.Log("Something went wrong");
        }
        
    }
}