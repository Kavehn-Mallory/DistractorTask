using TMPro;
using UnityEngine;

namespace DistractorTask.Debugging
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class DebuggingComponent : MonoBehaviour
    {
        [SerializeField] private DebuggingScriptableObject debuggingHandler;


        private void Awake()
        {
            debuggingHandler.SetDebugTextField(this.GetComponent<TextMeshProUGUI>());
        }
    }
}