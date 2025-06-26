using System;
using DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace DistractorTask.RoomAnalysis
{
    public class AreaRaycaster : MonoBehaviour
    {
        [SerializeField] private ARRaycastManager manager;
        [SerializeField] private Transform target;
        [SerializeField] private Camera mainCamera;
        [Tooltip("Number of raycasts per edge of the specified area. One raycast is placed at each corner by default + one raycast in the center of the area")]
        [SerializeField] private int raycastCountPerEdge = 1;

        [SerializeField] private float maxRaycastDistance = 5f;
        
        public TextMeshProUGUI debugText;
        
        private Transform _mainCameraTransform;

        private ARRaycast[] _raycasts;

        private float _debugDistance;
        private float _debugDistanceFromWall;
        private bool _isRaycastingEnabled;

        private void Awake()
        {
            if (!mainCamera)
            {
                mainCamera = Camera.main;
            }
            _mainCameraTransform = mainCamera?.transform;
            if (!_mainCameraTransform)
            {
                Debug.LogError($"No main camera found. Please either add one to {nameof(AreaRaycaster)} or tag an active camera with \"MainCamera\"");
                enabled = false;
            }
            

        }

        private void Start()
        {
            if (LoaderUtility
                    .GetActiveLoader()?
                    .GetLoadedSubsystem<XRRaycastSubsystem>() != null)
            {
                // XRPlaneSubsystem was loaded. The platform supports plane detection.
                Debug.Log("Raycasting is possible");
                debugText.text = "Raycasting is possible";
                _isRaycastingEnabled = true;
            }
            else
            {
                Debug.Log("Raycasting is not possible");
                debugText.text = "Raycasting is not possible";
                _isRaycastingEnabled = false;
            }
        }

        private void Update()
        {
            if (_raycasts == null || _raycasts.Length == 0 || !_isRaycastingEnabled)
            {
                return;
            }
            target?.SetPositionAndRotation(RequestAnchorPoint(_debugDistance, _debugDistanceFromWall).position, Quaternion.identity);
        }

        public void InitializeRaycasts(Vector2 distanceFromCenter, float targetDistance, float minDistanceFromWall)
        {

            targetDistance += minDistanceFromWall;
            var center = _mainCameraTransform.position + _mainCameraTransform.forward * 0.1f;

            var bottomLeft = center - _mainCameraTransform.right * distanceFromCenter.x - _mainCameraTransform.up * distanceFromCenter.y;
            var topRight = center + _mainCameraTransform.right * distanceFromCenter.x + _mainCameraTransform.up * distanceFromCenter.y;
            
            var centerPointScreenSpace = mainCamera.WorldToScreenPoint(center, mainCamera.stereoActiveEye).XY();
            var bottomLeftScreenSpace = mainCamera.WorldToScreenPoint(bottomLeft, mainCamera.stereoActiveEye).XY();
            var topRightScreenSpace = mainCamera.WorldToScreenPoint(topRight, mainCamera.stereoActiveEye).XY();
            
            

            _raycasts = new ARRaycast[5 + raycastCountPerEdge * 4];
            _raycasts[0] = manager.AddRaycast(centerPointScreenSpace, maxRaycastDistance);
            _raycasts[1] = manager.AddRaycast(bottomLeftScreenSpace, targetDistance);
            _raycasts[2] = manager.AddRaycast(new Vector2(bottomLeftScreenSpace.x, topRightScreenSpace.y), targetDistance);
            _raycasts[3] = manager.AddRaycast(topRightScreenSpace, targetDistance);
            _raycasts[4] = manager.AddRaycast(new Vector2(topRightScreenSpace.x, bottomLeftScreenSpace.y), targetDistance);


            var movePerCast = new Vector2(topRightScreenSpace.x - bottomLeftScreenSpace.x,
                topRightScreenSpace.y - bottomLeftScreenSpace.y) / (raycastCountPerEdge + 1f);

            var offset = 5;

            var startPosition = bottomLeftScreenSpace;
            //left
            for (int i = 0; i < raycastCountPerEdge; i++)
            {
                var position = startPosition + (new Vector2(0, movePerCast.y) * i);
                _raycasts[i + offset] = manager.AddRaycast(position, targetDistance);
            }

            offset += raycastCountPerEdge;
            startPosition = new Vector2(bottomLeftScreenSpace.x, topRightScreenSpace.y);
            //top
            for (int i = 0; i < raycastCountPerEdge; i++)
            {
                var position = startPosition + (new Vector2(movePerCast.x, 0) * i);
                _raycasts[i + offset] = manager.AddRaycast(position, targetDistance);
            }
            offset += raycastCountPerEdge;
            startPosition = topRightScreenSpace;
            //right
            for (int i = 0; i < raycastCountPerEdge; i++)
            {
                var position = startPosition + (new Vector2(0, -movePerCast.y) * i);
                _raycasts[i + offset] = manager.AddRaycast(position, targetDistance);
            }
            offset += raycastCountPerEdge;
            startPosition = new Vector2(topRightScreenSpace.x, bottomLeftScreenSpace.y);
            //bottom
            for (int i = 0; i < raycastCountPerEdge; i++)
            {
                var position = startPosition + (new Vector2(-movePerCast.x, 0) * i);
                _raycasts[i + offset] = manager.AddRaycast(position, targetDistance);
            }


        }

        public AnchorPoint RequestAnchorPoint(float targetDistance, float minDistanceFromWall)
        {
            var counter = 0;
            foreach (var raycast in _raycasts)
            {
                if (!raycast)
                {
                    counter++;
                }
                
            }

            debugText.text = counter.ToString();
            if (!_isRaycastingEnabled || counter == _raycasts.Length)
            {
                return new AnchorPoint
                {
                    position = _mainCameraTransform.position + _mainCameraTransform.forward * targetDistance,
                    distanceFromWall = minDistanceFromWall
                };
            }
            var minDistance = targetDistance + minDistanceFromWall;
            foreach (var raycast in _raycasts)
            {
                minDistance = math.min(raycast.distance, minDistance);
            }
            
            minDistance -= minDistanceFromWall;
            var raycastPosition = _mainCameraTransform.position + _mainCameraTransform.forward * minDistance;

            return new AnchorPoint
            {
                position = raycastPosition,
                distanceFromWall = _raycasts[0].distance - minDistance
            };
        }
        
        
        
        
    }

    [Serializable]
    public struct AnchorPoint
    {
        public Vector3 position;
        public float distanceFromWall;
    }
}