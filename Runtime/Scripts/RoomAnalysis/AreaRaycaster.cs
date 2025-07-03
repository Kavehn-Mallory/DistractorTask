using System;
using System.Collections.Generic;
using DistractorTask.Debugging;
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
        
        public DebuggingScriptableObject debugText;
        
        private Transform _mainCameraTransform;

        private Vector2[] _raycastStartPositions;

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
                debugText.AddDebugText("Raycasting is possible");
                _isRaycastingEnabled = true;
            }
            else
            {
                Debug.Log("Raycasting is not possible");
                debugText.AddDebugText("Raycasting is not possible");
                _isRaycastingEnabled = false;
            }
        }

        private void Update()
        {
            if (_raycastStartPositions == null || _raycastStartPositions.Length == 0 || !_isRaycastingEnabled)
            {
                return;
            }
            target?.SetPositionAndRotation(RequestAnchorPoint(_debugDistance, _debugDistanceFromWall).position, Quaternion.identity);
        }

        public void InitializeRaycasts(Vector2 distanceFromCenter)
        {
            var center = _mainCameraTransform.position + _mainCameraTransform.forward * 0.1f;

            var bottomLeft = center - _mainCameraTransform.right * distanceFromCenter.x - _mainCameraTransform.up * distanceFromCenter.y;
            var topRight = center + _mainCameraTransform.right * distanceFromCenter.x + _mainCameraTransform.up * distanceFromCenter.y;
            
            var centerPointScreenSpace = mainCamera.WorldToScreenPoint(center, mainCamera.stereoActiveEye).XY();
            var bottomLeftScreenSpace = mainCamera.WorldToScreenPoint(bottomLeft, mainCamera.stereoActiveEye).XY();
            var topRightScreenSpace = mainCamera.WorldToScreenPoint(topRight, mainCamera.stereoActiveEye).XY();
            
            _raycastStartPositions = new Vector2[5 + raycastCountPerEdge * 4];
            _raycastStartPositions[0] = centerPointScreenSpace;
            _raycastStartPositions[1] = bottomLeftScreenSpace;
            _raycastStartPositions[2] = new Vector2(bottomLeftScreenSpace.x, topRightScreenSpace.y);
            _raycastStartPositions[3] = topRightScreenSpace;
            _raycastStartPositions[4] = new Vector2(topRightScreenSpace.x, bottomLeftScreenSpace.y);


            var movePerCast = new Vector2(topRightScreenSpace.x - bottomLeftScreenSpace.x,
                topRightScreenSpace.y - bottomLeftScreenSpace.y) / (raycastCountPerEdge + 1f);

            var offset = 5;

            var startPosition = bottomLeftScreenSpace;
            //left
            for (int i = 0; i < raycastCountPerEdge; i++)
            {
                _raycastStartPositions[i + offset] = startPosition + (new Vector2(0, movePerCast.y) * i);
            }

            offset += raycastCountPerEdge;
            startPosition = new Vector2(bottomLeftScreenSpace.x, topRightScreenSpace.y);
            //top
            for (int i = 0; i < raycastCountPerEdge; i++)
            {
                _raycastStartPositions[i + offset] = startPosition + (new Vector2(movePerCast.x, 0) * i);
            }
            offset += raycastCountPerEdge;
            startPosition = topRightScreenSpace;
            //right
            for (int i = 0; i < raycastCountPerEdge; i++)
            {
                _raycastStartPositions[i + offset] = startPosition + (new Vector2(0, -movePerCast.y) * i);
            }
            offset += raycastCountPerEdge;
            startPosition = new Vector2(topRightScreenSpace.x, bottomLeftScreenSpace.y);
            //bottom
            for (int i = 0; i < raycastCountPerEdge; i++)
            {
                _raycastStartPositions[i + offset] = startPosition + (new Vector2(-movePerCast.x, 0) * i);
            }


        }

        public AnchorPoint RequestAnchorPoint(float targetDistance, float minDistanceFromWall)
        {
            if (!_isRaycastingEnabled || _raycastStartPositions == null || _raycastStartPositions.Length == 0)
            {
                return new AnchorPoint
                {
                    position = _mainCameraTransform.position + _mainCameraTransform.forward * targetDistance,
                    distanceFromWall = minDistanceFromWall,
                    directionTowardsWall = _mainCameraTransform.forward
                };
            }
            var minDistance = targetDistance + minDistanceFromWall;
            var hits = new List<ARRaycastHit>();

            var centerHitResults = new List<ARRaycastHit>();

            var centerDistance = minDistance;
            
            if(manager.Raycast(_raycastStartPositions[0], centerHitResults))
            {
                centerDistance = centerHitResults[0].distance;
                minDistance = math.min(centerDistance, minDistance);
                
            }
            
            for (var i = 1; i < _raycastStartPositions.Length; i++)
            {
                var raycast = _raycastStartPositions[i];
                hits.Clear();
                if (manager.Raycast(raycast, hits))
                {
                    minDistance = math.min(hits[0].distance, minDistance);
                }
            }

            minDistance -= minDistanceFromWall;
            var raycastPosition = _mainCameraTransform.position + _mainCameraTransform.forward * minDistance;
            

            return new AnchorPoint
            {
                position = raycastPosition,
                distanceFromWall = centerDistance - minDistance,
                directionTowardsWall = _mainCameraTransform.forward
            };
        }
        
        
        
        
    }

    [Serializable]
    public struct AnchorPoint
    {
        public Vector3 position;
        public float distanceFromWall;
        public Vector3 directionTowardsWall;

        public Vector3 GetPosition()
        {
            return position;
        }

        public Vector3 GetPositionInsideWall(float additionalOffset = 0.05f)
        {
            return position + directionTowardsWall * (additionalOffset + distanceFromWall);
        }
    }
}