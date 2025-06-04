using MixedReality.Toolkit.Input;
using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.UI;

namespace DistractorTask.InputHandler
{
    public class CustomRayReticleVisualizer : BaseReticleVisual
    {
        [SerializeField]
        [Tooltip("The interactor which this visual represents.")]
        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;

        [SerializeField]
        [Tooltip("The GameObject which holds the proximity light for the reticle")]
        private GameObject proximityLight;

        /*[SerializeField]
        [Tooltip("Should a reticle appear on all surfaces hit by the interactor or interactables only?")]
        private MRTKRayReticleVisual.ReticleVisibilitySettings visibilitySettings;*/
        
        [SerializeField]
        private Transform headGazeVisualizer;
        
        [SerializeField]
        private float distanceFromCamera = 0.1f;

        [SerializeField]
        private Canvas targetCanvas;

        private Transform _mainCameraTransform;

        private float DistanceFromCamera()
        {
            if (targetCanvas && _mainCameraTransform)
            {
                return Vector3.Distance(targetCanvas.transform.position, _mainCameraTransform.transform.position);
            }

            return distanceFromCamera;
        }

        private bool _onUIObject;

        private void Start()
        {
            _mainCameraTransform = Camera.main?.transform;
            rayInteractor.uiHoverEntered.AddListener(OnHoverEnter);
            rayInteractor.uiHoverExited.AddListener(OnHoverExit);
        }

        private void OnHoverExit(UIHoverEventArgs uiHoverExit)
        {
            Reticle.SetActive(false);
            _onUIObject = false;
            if (proximityLight)
            {
                proximityLight.SetActive(false);
            }
            headGazeVisualizer.gameObject.SetActive(true);
        }

        private void OnHoverEnter(UIHoverEventArgs uiHoverEnter)
        {
            var uiPosition = uiHoverEnter.uiObject.transform.position;
            _onUIObject = true;
            ReticleRoot.position = uiPosition;
            Reticle.SetActive(true);
            if (proximityLight)
            {
                proximityLight.SetActive(true);
            }

            headGazeVisualizer.gameObject.SetActive(false);

        }

        private void Update()
        {
            if (!_onUIObject)
            {
                headGazeVisualizer.position = rayInteractor.rayOriginTransform.position +
                                              rayInteractor.rayOriginTransform.forward * DistanceFromCamera();
            }
            
        }
    }
}