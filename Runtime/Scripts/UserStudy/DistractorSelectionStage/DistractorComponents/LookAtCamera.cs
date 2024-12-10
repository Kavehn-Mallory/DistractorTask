using UnityEngine;

namespace DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents
{
    public class LookAtCamera : MonoBehaviour
    {

        private Transform _cameraTransform;
    
        private void Awake()
        {
            if (!Camera.main)
            {
                Debug.LogError("No main camera found. Is required for correct positioning of target", this);
                enabled = false;
                return;
            }
            _cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            this.transform.SetPositionAndRotation(transform.position, _cameraTransform.rotation);
        }
    }
}