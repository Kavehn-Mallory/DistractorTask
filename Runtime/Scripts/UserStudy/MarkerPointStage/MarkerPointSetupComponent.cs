using System;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using DistractorTask.UserStudy.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DistractorTask.UserStudy.MarkerPointStage
{
    public class MarkerPointSetupComponent : SendingStudyStageComponent<MarkerPointStageEvent>
    {
        
        [SerializeField] private Canvas markerPointCanvas;
        [SerializeField] private Vector2Int zones;
        [SerializeField] private Image marker;

        private Image[] _markerPoints = Array.Empty<Image>();
        private int _currentMarker;
        
        public int MarkerPointCount => _markerPoints.Length;
        
        private void Awake()
        {
            markerPointCanvas.gameObject.SetActive(false);
            _markerPoints = CreateMarkerPoints(marker, markerPointCanvas, zones);

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

        public override void StartStudy(INetworkManager manager)
        {
            manager.RegisterCallback<ConfirmationData>(OnStartConfirmed);
            base.StartStudy(manager);
        }

        private void OnStartConfirmed(ConfirmationData obj)
        {
            Manager.UnregisterCallback<ConfirmationData>(OnStartConfirmed);
            Manager.RegisterCallback<ConfirmationData>(OnPointSelectionConfirmed);
            _markerPoints[0].enabled = true;
            markerPointCanvas.gameObject.SetActive(true);
            Manager.TransmitNetworkMessage(new MarkerCountData
            {
                markerCount = MarkerPointCount
            });
        }
        
        private void ActivateMarker()
        {
            _markerPoints[_currentMarker].enabled = false;
            _currentMarker++;
            _markerPoints[_currentMarker].enabled = true;
        }
        
        private void OnPointSelectionConfirmed(ConfirmationData data)
        {
            if (data.confirmationNumber != _currentMarker)
            {
                //todo throw error
            }

            if (_currentMarker >= _markerPoints.Length - 1)
            {
                EndMarkerPointSetup();
                return;
            }
            ActivateMarker();
            Manager.TransmitNetworkMessage(new ConfirmationData
            {
                confirmationNumber = _currentMarker
            });
        }
        
        public void EndMarkerPointSetup()
        {
            _markerPoints[^1].enabled = false;
            markerPointCanvas.gameObject.SetActive(false);
            Manager.UnregisterCallback<ConfirmationData>(OnPointSelectionConfirmed); 
            EndStudy(Manager);
        }
    }
}