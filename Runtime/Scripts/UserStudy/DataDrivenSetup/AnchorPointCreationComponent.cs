using DistractorTask.RoomAnalysis;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents;
using TMPro;
using UnityEngine;

namespace DistractorTask.UserStudy.DataDrivenSetup
{
    public class AnchorPointCreationComponent : MonoBehaviour
    {
        //listen for next marker is set 
        //listen for marker count data 
        //have array with marker points 
        
        public TextMeshProUGUI debugText;
        
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float distanceFromWall = 0.1f;
        [SerializeField] private float targetDistance = 2.0f;
        
        [SerializeField] private AreaRaycaster areaRaycaster;
        [SerializeField] private DistractorTaskComponent distractorTaskComponent;

        [SerializeField]
        private DistractorAnchorPointAsset anchorPoints;
        
        
        private Transform _mainCameraTransform;
        private OnMarkerPointActivatedData _activeMarkerPointData = new();


        private void Awake()
        {
            if (!mainCamera)
            {
                mainCamera = Camera.main;
            }
            _mainCameraTransform = mainCamera?.transform;
            if (!_mainCameraTransform)
            {
                Debug.LogError($"No main camera found. Please either add one to {nameof(AnchorPointCreationComponent)} or tag an active camera with \"MainCamera\"");
                enabled = false;
            }
            anchorPoints.Reset();
            NetworkManager.Instance.RegisterCallback<MarkerPointCountData>(OnMarkerPointCountReceived);
            NetworkManager.Instance.RegisterCallback<OnMarkerPointActivatedData>(OnMarkerPointActivated);
            //Register everything in here 
            InputHandler.InputHandler.Instance.OnSelectionButtonPressed += AddPlacementPosition;
            
            
        }

        public void DebugInput()
        {
            AddPlacementPosition();
        }
        

        private void AddPlacementPosition()
        {
            debugText.text = "Input Received";
            //var targetPosition = raycastTarget.transform.position;
            var position = areaRaycaster.RequestAnchorPoint(targetDistance, distanceFromWall);
            
            if (anchorPoints.SetPosition(_activeMarkerPointData.MarkerPointIndex, position))
            {
                debugText.text = "Anchor Point was set";
                NetworkManager.Instance.MulticastRespond(_activeMarkerPointData, NetworkExtensions.DefaultPort, GetInstanceID());
                //reset data 
                _activeMarkerPointData = new OnMarkerPointActivatedData
                {
                    MarkerPointIndex = -1
                };
                
            }
        }

        private void OnMarkerPointActivated(OnMarkerPointActivatedData data, int callerId)
        {
            _activeMarkerPointData = data;
        }


        private void OnMarkerPointCountReceived(MarkerPointCountData data, int callerId)
        {
            debugText.text = "Marker Point Data Received";
            areaRaycaster.InitializeRaycasts(distractorTaskComponent.GetBoundsForDistractorArea());
            anchorPoints.InitializeContainer(data.markerCount);
        }
    }
}