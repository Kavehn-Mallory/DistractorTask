using UnityEngine;
using UnityEngine.InputSystem;

namespace DistractorTask.InputHandler
{
    public class InputHandler : MonoBehaviour
    {
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
                // 'Use' code here
                
            }

        }
    }
}