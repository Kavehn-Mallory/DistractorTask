using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace DistractorTask.InputHandler
{
    public class HeadGazeTracker : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The interactor which this visual represents.")]
        private XRRayInteractor rayInteractor;

        [SerializeField]
        private float distanceFromCamera = 0.1f;

        [SerializeField]
        private Transform headGazeVisualizer;

        public Transform start;

        public Transform end;
        
        private void Awake()
        {
            if (!rayInteractor)
            {
                Debug.LogError("No ray interactor specified", this);
                enabled = false;
            }
        }

        private void Update()
        {
            start.position = rayInteractor.rayOriginTransform.position;
            end.position = rayInteractor.rayOriginTransform.position + rayInteractor.rayOriginTransform.forward;
            var direction = rayInteractor.rayOriginTransform.forward;
            
            headGazeVisualizer.position = rayInteractor.rayOriginTransform.position + direction * distanceFromCamera;
        }
    }
}