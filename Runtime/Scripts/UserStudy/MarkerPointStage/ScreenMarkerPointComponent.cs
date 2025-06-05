using System;
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
        
        private void Awake()
        {
            markerPointCanvas.gameObject.SetActive(false);
            _markerPoints = CreateMarkerPoints(marker, markerPointCanvas, zones);
            //todo make persistent
            Manager.RegisterMulticastResponse<MarkerCountData, MarkerPointResponseData>(OnStartConfirmed, _port, GetInstanceID());
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
            markerPointCanvas.gameObject.SetActive(true);
            _currentlyActiveMarker = -1;
        }
        
        private void ActivateMarker()
        {
            Debug.Log("Switching to next marker point");
            if (_currentlyActiveMarker >= 0)
            {
                _markerPoints[_currentMarker].enabled = false;
            }
            _currentlyActiveMarker = _currentMarker;
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

            if (_currentMarker >= _markerPoints.Length - 1)
            {
                EndMarkerPointSetup();
                return;
            }
            ActivateMarker();
        }
        
        public void EndMarkerPointSetup()
        {
            _markerPoints[^1].enabled = false;
            markerPointCanvas.gameObject.SetActive(false);
            Manager.UnregisterCallback<ActivateMarkerPoint>(OnPointSelectionConfirmed, _port); 
        }
    }
    



}