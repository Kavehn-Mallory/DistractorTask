using TMPro;
using UnityEngine;

namespace DistractorTask.Debugging
{
    
    [CreateAssetMenu(menuName = "DistractorTask/Debugging/DebuggingScriptableObject", fileName = "DebuggingScriptableObject", order = 0)]
    public class DebuggingScriptableObject : ScriptableObject
    {
        [SerializeField]
        private TextMeshProUGUI debugText;

        public void SetDebugText(string text)
        {
            if (debugText)
            {
                debugText.text = text;
            }
        }
    }
}