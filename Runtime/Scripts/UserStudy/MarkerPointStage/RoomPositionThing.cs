using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace DistractorTask.UserStudy.MarkerPointStage
{
    public class RoomPositionThing : MonoBehaviour
    {
        [SerializeField]
        private ARRaycast raycast;


        private void Awake()
        {
            raycast = GetComponent<ARRaycast>();
            
        }
    }
}