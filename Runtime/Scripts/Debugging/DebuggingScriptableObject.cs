using System;
using DistractorTask.Transport;
using TMPro;
using UnityEngine;

namespace DistractorTask.Debugging
{
    
    [CreateAssetMenu(menuName = "DistractorTask/Debugging/DebuggingScriptableObject", fileName = "DebuggingScriptableObject", order = 0)]
    public class DebuggingScriptableObject : ScriptableObject
    {
        [SerializeField]
        private TextMeshProUGUI debugText;

        public void AddDebugText(string text)
        {
            if (debugText)
            {
                debugText.text += "\n";
                debugText.text += text;
            }
        }

        public void AddConnectionState(ConnectionState connectionState)
        {
            if (debugText)
            {
                debugText.text += "\n";
                debugText.text += $"State changed to {connectionState.ToString()}";
            }
        }

        public void SetDebugTextField(TextMeshProUGUI textField)
        {
            debugText = textField;
        }

        private void OnDisable()
        {
            debugText = null;
        }
    }
}