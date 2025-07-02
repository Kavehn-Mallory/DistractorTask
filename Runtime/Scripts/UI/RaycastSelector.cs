using System;
using DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DistractorTask.UI
{
    public class RaycastSelector : MonoBehaviour
    {
        public Transform raycastReticle;

        private Transform _mainCameraTransform;

        [SerializeField]
        private LayerMask layerMask;

        [SerializeField]
        private UnityEvent<DistractorComponent> onHoverEnter;

        [SerializeField]
        private UnityEvent onHoverExit;

        private void Start()
        {
            _mainCameraTransform = Camera.main.transform;
        }


        private void Update()
        {
            if(Physics.Raycast(_mainCameraTransform.position, _mainCameraTransform.forward, out var hitInfo, 10f, layerMask))
            {
                if (hitInfo.transform.TryGetComponent<DistractorComponent>(out var distractorComponent))
                {
                    Debug.Log("Hit");
                    onHoverEnter.Invoke(distractorComponent);

                    raycastReticle.SetPositionAndRotation(hitInfo.point, Quaternion.identity);
                    return;
                }
            }
            raycastReticle.SetPositionAndRotation(_mainCameraTransform.position + _mainCameraTransform.forward * 0.5f, Quaternion.identity);
            onHoverExit.Invoke();
            
        }
    }
}