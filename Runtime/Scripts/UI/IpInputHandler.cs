using DistractorTask.Transport;
using TMPro;
using UnityEngine;

namespace DistractorTask.UI
{
    public class IpInputHandler : MonoBehaviour
    {
        public TMP_InputField inputFieldP0;
        public TMP_InputField inputFieldP1;
        public TMP_InputField inputFieldP2;
        public TMP_InputField inputFieldP3;


        private void Awake()
        {
            inputFieldP0.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputFieldP0.characterLimit = 3;
            
            inputFieldP1.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputFieldP1.characterLimit = 3;
            
            inputFieldP2.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputFieldP2.characterLimit = 3;
            
            inputFieldP3.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputFieldP3.characterLimit = 3;
        }


        public void InputIpAddress()
        {
            if (int.TryParse(inputFieldP0.text, out var p0) && int.TryParse(inputFieldP1.text, out var p1) &&
                int.TryParse(inputFieldP2.text, out var p2) && int.TryParse(inputFieldP3.text, out var p3))
            {
                Server.Instance.SetIpTransmissionSettings(p0, p1, p2, p3);
            }
            
        }
    }
}