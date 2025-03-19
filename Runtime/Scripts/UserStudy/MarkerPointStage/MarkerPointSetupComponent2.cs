using System;
using DistractorTask.Core;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using DistractorTask.UserStudy.Core;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DistractorTask.UserStudy.MarkerPointStage
{
    public class MarkerPointSetupComponent2 : MonoBehaviour
    {
        
        [SerializeField] private Canvas markerPointCanvas;
        [SerializeField] private Vector2Int zones;
        [SerializeField] private Image marker;

        private Image[] _markerPoints = Array.Empty<Image>();
        private int _currentMarker;
        
        public int MarkerPointCount => _markerPoints.Length;

        public NetworkManager Manager => NetworkManager.Instance;
        
        private void Awake()
        {
            markerPointCanvas.gameObject.SetActive(false);
            _markerPoints = CreateMarkerPoints(marker, markerPointCanvas, zones);
            Manager.RegisterCallback<MarkerCountData>(OnStartConfirmed);

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
            Manager.UnregisterCallback<MarkerCountData>(OnStartConfirmed);
            Manager.RegisterCallback<ActivateMarkerPoint>(OnPointSelectionConfirmed);
            _markerPoints[0].enabled = true;
            markerPointCanvas.gameObject.SetActive(true);
        }
        
        private void ActivateMarker()
        {
            _markerPoints[_currentMarker].enabled = false;
            _currentMarker++;
            _markerPoints[_currentMarker].enabled = true;
        }
        
        private void OnPointSelectionConfirmed(ActivateMarkerPoint data, int callerId)
        {
            
            if (callerId == GetInstanceID())
            {
                return;
            }
            if (data.currentMarkerIndex != _currentMarker)
            {
                //todo throw error
            }

            if (_currentMarker >= _markerPoints.Length - 1)
            {
                EndMarkerPointSetup();
                return;
            }
            ActivateMarker();
            Manager.BroadcastMessage(new ConfirmationData
            {
                confirmationNumber = _currentMarker
            }, GetInstanceID());
        }
        
        public void EndMarkerPointSetup()
        {
            _markerPoints[^1].enabled = false;
            markerPointCanvas.gameObject.SetActive(false);
            Manager.UnregisterCallback<ActivateMarkerPoint>(OnPointSelectionConfirmed); 
        }
    }

    [Serializable]
    internal struct ActivateMarkerPoint : ISerializer
    {

        [FormerlySerializedAs("nextMarkerIndex")] public int currentMarkerIndex;
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteInt(currentMarkerIndex);
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            currentMarkerIndex = reader.ReadInt();
        }
    }
}