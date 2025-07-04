using System;
using DistractorTask.Logging;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using UnityEngine;
using UnityEngine.UI;

namespace DistractorTask.UserStudy.MarkerPointStage
{
    public class ScreenMarkerPointComponent : MonoBehaviour
    {
        
        [SerializeField] private Canvas markerPointCanvas;
        [SerializeField] private Vector2Int zones;
        [SerializeField] private Image marker;

        private Image[] _markerPoints = Array.Empty<Image>();
        private int _currentMarker;
        private int _currentlyActiveMarker;

        private ushort _port = NetworkExtensions.DisplayWallControlPort;
        
        public int MarkerPointCount => _markerPoints.Length;

        public NetworkManager Manager => NetworkManager.Instance;

        private Action _activateMarkerPointsRegisterCallback;
        private Action _unregisterMarkerCountDataResponse;
        
        private void Awake()
        {
            markerPointCanvas.gameObject.SetActive(false);
            _markerPoints = CreateMarkerPoints(marker, markerPointCanvas, zones);
            _unregisterMarkerCountDataResponse = Manager.RegisterPersistentMulticastResponse<MarkerCountData, MarkerPointResponseData>(OnStartConfirmed, _port, GetInstanceID());
        }

        private void OnDisable()
        {
            _unregisterMarkerCountDataResponse?.Invoke();
        }

        private static Image[] CreateMarkerPoints(Image image, Canvas markerPointCanvas, Vector2Int markerZones)
        {
            var result = new Image[markerZones.x * markerZones.y];

            var xStep = Screen.width / markerZones.x;
            var yStep = Screen.height / markerZones.y;
            
            for (int y = 0; y < markerZones.y; y++)
            {
                for (int x = 0; x < markerZones.x; x++)
                {
                    var markerInstance = Instantiate(image, new Vector3(0.5f * xStep + xStep * x, 0.5f * yStep + yStep * y), Quaternion.identity, markerPointCanvas.transform);
                    result[y * markerZones.x + x] = markerInstance;
                    markerInstance.enabled = false;
                }
            }

            return result;
        }
        

        private void OnStartConfirmed(MarkerCountData obj, int callerId)
        {
            if (callerId == GetInstanceID())
            {
                return;
            }
            Debug.Log("Marker Point Start Received");
            //Manager.RegisterCallback<ActivateMarkerPoint>(OnPointSelectionConfirmed, _port);
            _activateMarkerPointsRegisterCallback = Manager.RegisterPersistentMulticastResponse<ActivateMarkerPoint, OnMarkerPointActivatedData>(
                OnPointSelectionConfirmed, _port, GetInstanceID());
            Manager.RegisterMulticastResponse<MarkerPointEndData, MarkerPointResponseData>(EndMarkerPointSetup, NetworkExtensions.DisplayWallControlPort, GetInstanceID());
            markerPointCanvas.gameObject.SetActive(true);
            _currentlyActiveMarker = -1;
        }



        private void ActivateMarker()
        {
            Debug.Log("Switching to next marker point");
            if (_currentlyActiveMarker >= 0)
            {
                _markerPoints[_currentlyActiveMarker].enabled = false;
            }

            if (_currentMarker < 0 || _currentMarker >= _markerPoints.Length)
            {
                _currentlyActiveMarker = -1;
                return;
            }
            _currentlyActiveMarker = _currentMarker;
            LoggingComponent.Log(LogData.CreateMarkerPointActivatedLogData(_currentMarker));
            _markerPoints[_currentMarker].enabled = true;
        }
        
        private void OnPointSelectionConfirmed(ActivateMarkerPoint data, int callerId)
        {
            Debug.Log("Activate marker point data received");
            if (callerId == GetInstanceID())
            {
                return;
            }
            
            _currentMarker = data.currentMarkerIndex;
            ActivateMarker();
        }
        
        private void EndMarkerPointSetup(MarkerPointEndData arg1, int callerId)
        {
            if(_currentlyActiveMarker >= 0)
                _markerPoints[_currentlyActiveMarker].enabled = false;
            markerPointCanvas.gameObject.SetActive(false);
            _activateMarkerPointsRegisterCallback?.Invoke();
            _activateMarkerPointsRegisterCallback = null;
        }
    }
    



}