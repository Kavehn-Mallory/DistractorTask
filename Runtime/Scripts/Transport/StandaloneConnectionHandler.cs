using System.Collections;
using DistractorTask.Transport.DataContainer;
using Unity.Networking.Transport;
using UnityEngine;

namespace DistractorTask.Transport
{
    public class StandaloneConnectionHandler : MonoBehaviour
    {

        private NetworkEndpoint _endpoint;
        
        public IEnumerator Start()
        {
            NetworkManager.Instance.StartListening(NetworkExtensions.DefaultPort,
                OnConnectionEstablished);
            _endpoint = NetworkEndpoint.AnyIpv4.WithPort(NetworkExtensions.IpListeningPort);
            yield return new WaitForSeconds(2f);
            NetworkManager.Instance.Connect(_endpoint, OnConnectionStateReceived);
        }

        private void OnConnectionStateReceived(ConnectionState obj) =>
            NetworkManager.Instance.MulticastMessage(new IpAddressData
            {
                Endpoint = NetworkExtensions.GetLocalEndpointWithDefaultPort(false),
            }, _endpoint.Port, this.GetInstanceID());
        
        
        private void OnConnectionEstablished(ConnectionState obj)
        {
            if (obj == ConnectionState.Connected)
            {
                Debug.Log("Connection established!");
                return;
            }
            Debug.Log("Something went wrong");
        }
    }
}