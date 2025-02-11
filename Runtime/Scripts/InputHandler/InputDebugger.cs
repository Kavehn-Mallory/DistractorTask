using TMPro;
using UnityEngine;

namespace DistractorTask.InputHandler
{
    public class InputDebugger : MonoBehaviour
    {
        public TextMeshProUGUI textField;


        private void Start()
        {
            InputHandler.Instance.OnDeviceAdded += OnDeviceAdded;
        }

        private void OnDeviceAdded(string obj)
        {
            textField.text = obj;
        }
    }
}