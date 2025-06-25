using System;
using DistractorTask.Core;
using UnityEngine.InputSystem;

namespace DistractorTask.InputHandler
{
    public class InputHandler : Singleton<InputHandler>
    {
        public event Action OnSelectionButtonPressed = delegate { };

        public event Action<string> OnDeviceAdded = delegate { };
        
        void Update()
        {
            //todo check this and implement it properly: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.11/manual/HID.html
            var clicker = Keyboard.current;
            if (clicker == null)
            {
                return; // No gamepad connected.
            }
            
            if (clicker.escapeKey.wasPressedThisFrame)
            {
                OnSelectionButtonPressed.Invoke();
            }

        }

        private void Start()
        {
            InputSystem.onDeviceChange +=
                (device, change) =>
                {
                    if (change == InputDeviceChange.Added) OnDeviceAdded.Invoke(device.name);
                };
        }
        
    }
}