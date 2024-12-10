using TMPro;
using UnityEngine;

namespace DistractorTask.Transport
{
    public class IpAddressProvider : MonoBehaviour
    {

        public TextMeshProUGUI ipAddressField;
        
        private void Start()
        {
            var ip = NetworkConnectionHandler.GetLocalIPAddress();
            ipAddressField.text = ip.ToString();
        }
    }
}