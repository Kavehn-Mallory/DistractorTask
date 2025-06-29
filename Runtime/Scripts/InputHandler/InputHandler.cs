using System;
using DistractorTask.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DistractorTask.InputHandler
{
    public class InputHandler : Singleton<InputHandler>
    {

        private MagicLeapOpenXRInput _magicLeapInputs;

        public event Action OnSelectionButtonPressed = delegate { };

        public event Action OnTriggerButtonPressed = delegate { };
        
        private MagicLeapOpenXRInput.ControllerActions _controllerActions;
        private MagicLeapOpenXRInput.EyesActions _eyesActions;
        
        private void Start()
        {
            _magicLeapInputs = new MagicLeapOpenXRInput();
            _magicLeapInputs.Enable();

            _controllerActions = new MagicLeapOpenXRInput.ControllerActions(_magicLeapInputs);

            _controllerActions.Bumper.performed += OnBumperPressed;
            _controllerActions.Trigger.performed += OnTriggerPressed;
            _eyesActions = new MagicLeapOpenXRInput.EyesActions(_magicLeapInputs);

        }

        private void OnTriggerPressed(InputAction.CallbackContext obj)
        {
            OnTriggerButtonPressed.Invoke();
        }

        private void OnBumperPressed(InputAction.CallbackContext obj)
        {
            OnSelectionButtonPressed.Invoke();
        }

        void Update()
        {
            if (_eyesActions.enabled)
            {
                //Debug.Log(_eyesActions.GazePosition.ReadValue<Vector3>());
            }
            //Todo track everything that we need 

        }


        
    }
}