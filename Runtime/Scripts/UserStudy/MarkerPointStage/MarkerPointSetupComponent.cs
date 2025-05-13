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
        
        
        private int _currentMarker;

        public int MarkerPointCount = 6;

        public MarkerPointVisualizationController controller;

        
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
            manager.RegisterCallback<ConfirmationData>(OnStartConfirmed, NetworkExtensions.DefaultPort);
        }

        private async void OnStartConfirmed(ConfirmationData obj, int callerId)
        {
            if (callerId == GetInstanceID())
            {
                return;
            }
            Manager.UnregisterCallback<ConfirmationData>(OnStartConfirmed, NetworkExtensions.DefaultPort);
            Manager.RegisterCallback<ConfirmationData>(OnPointSelectionConfirmed, NetworkExtensions.DefaultPort);
            
            await controller.InitializeMarkerPointSetup(MarkerPointCount);
            Manager.BroadcastMessage(new MarkerCountData
            {
                markerCount = MarkerPointCount
            }, GetInstanceID());
        }
        
        private void ActivateMarker()
        {
            _currentMarker++;
        }
        
        private async void OnPointSelectionConfirmed(ConfirmationData data, int callerId)
        {
            if (callerId == GetInstanceID())
            {
                return;
            }
            if (data.confirmationNumber != _currentMarker)
            {
                //todo throw error
            }

            if (_currentMarker >= MarkerPointCount - 1)
            {
                EndMarkerPointSetup();
                return;
            }

            ActivateMarker();
            await controller.TriggerNextPoint(_currentMarker);
            
            Manager.MulticastMessage(new ConfirmationData
            {
                confirmationNumber = _currentMarker
            }, NetworkExtensions.DefaultPort, GetInstanceID());
        }
        
        public void EndMarkerPointSetup()
        {
            Manager.UnregisterCallback<ConfirmationData>(OnPointSelectionConfirmed, NetworkExtensions.DefaultPort); 
            EndStudy(Manager);
        }
    }
}