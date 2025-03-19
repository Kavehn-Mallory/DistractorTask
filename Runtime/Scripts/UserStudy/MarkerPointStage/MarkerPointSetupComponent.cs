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
        }

        private void OnStartConfirmed(ConfirmationData obj, int callerId)
        {
            if (callerId == GetInstanceID())
            {
                return;
            }
            Manager.UnregisterCallback<ConfirmationData>(OnStartConfirmed);
            Manager.RegisterCallback<ConfirmationData>(OnPointSelectionConfirmed);

            Manager.BroadcastMessage(new MarkerCountData
            {
                markerCount = MarkerPointCount
            }, GetInstanceID());
        }
        
        private void ActivateMarker()
        {
            _currentMarker++;
        }
        
        private void OnPointSelectionConfirmed(ConfirmationData data, int callerId)
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

            Manager.BroadcastMessage(new ActivateMarkerPoint
            {
                currentMarkerIndex = _currentMarker
            }, GetInstanceID());
            ActivateMarker();
            Manager.BroadcastMessage(new ConfirmationData
            {
                confirmationNumber = _currentMarker
            }, GetInstanceID());
        }
        
        public void EndMarkerPointSetup()
        {
            Manager.UnregisterCallback<ConfirmationData>(OnPointSelectionConfirmed); 
            EndStudy(Manager);
        }
    }
}