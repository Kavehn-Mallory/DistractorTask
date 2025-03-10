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
            NetworkManager.Instance.StartListening(NetworkHelper.GetLocalEndpointWithDefaultPort(true),
                OnConnectionEstablished);
            _endpoint = NetworkEndpoint.AnyIpv4.WithPort(NetworkHelper.IpListeningPort);
            yield return new WaitForSeconds(2f);
            NetworkManager.Instance.Connect(_endpoint, OnConnectionStateReceived, ConnectionType.Multicast);
        }

        private void OnConnectionStateReceived(ConnectionState obj) =>
            NetworkManager.Instance.MulticastMessage(new IpAddressData
            {
                Endpoint = NetworkHelper.GetLocalEndpointWithDefaultPort(false),
            }, _endpoint, this.GetInstanceID());
        
        
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