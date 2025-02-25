using System.Collections.Generic;
using DistractorTask.Transport.DataContainer;
using DistractorTask.UserStudy.Core;
using MixedReality.Toolkit.Input;
using UnityEngine;

namespace DistractorTask.UserStudy.MarkerPointStage
{
    public class DistractorPlacementSetupComponent : ReceivingStudyStageComponent<MarkerPointStageEvent>
    {
        
        [SerializeField] private Camera mainCamera;
        [SerializeField] private FuzzyGazeInteractor gazeInteractor;
    
        private readonly List<Vector3> _distractorPlacementPositions = new();
        private Transform _mainCameraTransform;

        public List<Vector3> DistractorPlacementPositions
        {
            get
            {
                if(_distractorPlacementPositions.Count > 0)
                    return _distractorPlacementPositions;
                return new List<Vector3>
                {
                    _mainCameraTransform.position + _mainCameraTransform.forward
                };
            }
        }

        private int _currentMarkerPoint;
        private int _markerPointCount;
        private bool _acceptInput;
        
        private void Awake()
        {
            if (!mainCamera)
            {
                mainCamera = Camera.main;
            }
            _mainCameraTransform = mainCamera?.transform;
            if (!_mainCameraTransform)
            {
                Debug.LogError($"No main camera found. Please either add one to {nameof(DistractorPlacementSetupComponent)} or tag an active camera with \"MainCamera\"");
                enabled = false;
            }
        }
        


        protected sealed override void OnStudyStageStart(MarkerPointStageEvent studyEvent)
        {
            Manager.RegisterCallback<MarkerCountData>(OnMarkerCountDataReceived);
            Manager.BroadcastMessage(new ConfirmationData());
            InputHandler.InputHandler.Instance.OnSelectionButtonPressed += AddPlacementPosition;
        }

        private void OnMarkerCountDataReceived(MarkerCountData markerCountData)
        {
            _markerPointCount = markerCountData.markerCount;
            Manager.UnregisterCallback<MarkerCountData>(OnMarkerCountDataReceived);
            Manager.RegisterCallback<ConfirmationData>(OnConfirmationDataReceived);
            StartMarkerPlacement();
        }
        
        private void StartMarkerPlacement()
        {
            _acceptInput = true;
            _currentMarkerPoint = 0;
            //todo display counter and tell the user what to do 
        }
        
        private void OnConfirmationDataReceived(ConfirmationData confirmationData)
        {
            _currentMarkerPoint = confirmationData.confirmationNumber;
            _acceptInput = true;
        }

        protected sealed override void OnStudyStageEnd(MarkerPointStageEvent studyEvent)
        {
            Manager.UnregisterCallback<ConfirmationData>(OnConfirmationDataReceived);
            _acceptInput = false;
            InputHandler.InputHandler.Instance.OnSelectionButtonPressed -= AddPlacementPosition;
        }
        
        [ContextMenu("Add Position")]
        public void AddPlacementPosition()
        {
            if (!_acceptInput)
            {
                return;
            }

            _acceptInput = false;
            var position = _mainCameraTransform.position + _mainCameraTransform.forward;
        
            _distractorPlacementPositions.Add(position);
            Manager.BroadcastMessage(new ConfirmationData
            {
                confirmationNumber = _currentMarkerPoint
            });
        }
        
    }
}