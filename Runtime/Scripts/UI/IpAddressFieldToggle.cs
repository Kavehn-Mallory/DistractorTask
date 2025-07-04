using Unity.Networking.Transport;
using UnityEngine;
using DistractorTask.Transport;

namespace DistractorTask.UI
{
    public class IpAddressFieldToggle : MonoBehaviour
    {

        public GameObject targetObject;

        public void OnConnectionStateChanged(NetworkEndpoint endpoint, ConnectionState connectionState)
        {
            if (connectionState == ConnectionState.Connected)
            {
                targetObject.SetActive(false);
                return;
            }
            targetObject.SetActive(true);
        }
    }
}