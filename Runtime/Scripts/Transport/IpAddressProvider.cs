using TMPro;
using UnityEngine;

namespace DistractorTask.Transport
{
    public class IpAddressProvider : MonoBehaviour
    {

        public TextMeshProUGUI ipAddressField;
        
        private void Start()
        {
            var ip = NetworkExtensions.GetLocalIPAddress();
            if (ip == null)
            {
                ipAddressField.text = "No IP-Address found.";
                return;
            }
            ipAddressField.text = ip.ToString();
        }
    }
}