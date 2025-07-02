using TMPro;
using UnityEngine;

namespace DistractorTask.Logging
{
    public class GenerateUserIdComponent : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField text;
        
        

        public void SetId()
        {
            text.text = LoggingComponent.Instance.UserId;
        }
        

    }
}