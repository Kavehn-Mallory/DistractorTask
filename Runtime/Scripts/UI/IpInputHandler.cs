using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using TMPro;
using Unity.Networking.Transport;
using UnityEngine;

namespace DistractorTask.UI
{
    public class IpInputHandler : MonoBehaviour
    {
        public TMP_InputField inputFieldP0;
        public TMP_InputField inputFieldP1;
        public TMP_InputField inputFieldP2;
        public TMP_InputField inputFieldP3;
        public TMP_InputField inputFieldPort;


        public ushort targetPort = NetworkExtensions.DefaultPort;
        private NetworkEndpoint _endpoint;

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
            
            inputFieldPort.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputFieldPort.characterLimit = 5;
            inputFieldPort.text = NetworkExtensions.IpListeningPort.ToString();
        }


        public void EstablishConnection()
        {
            if (int.TryParse(inputFieldP0.text, out var p0) && int.TryParse(inputFieldP1.text, out var p1) &&
                int.TryParse(inputFieldP2.text, out var p2) && int.TryParse(inputFieldP3.text, out var p3) && ushort.TryParse(inputFieldPort.text, out var port))
            {
                Debug.Log($"Starting to listen on {NetworkExtensions.GetLocalEndpointWithDefaultPort(true)}");
                NetworkManager.Instance.StartListening(targetPort,
                    OnConnectionEstablished);
                _endpoint = NetworkEndpoint.Parse($"{p0}.{p1}.{p2}.{p3}", port);
                NetworkManager.Instance.Connect(_endpoint, OnConnectionStateReceived);
            }
            
        }

        private void OnConnectionEstablished(ConnectionState obj)
        {
            if (obj == ConnectionState.Connected)
            {
                Debug.Log("Connection established!");
                return;
            }
            Debug.Log("Something went wrong");
        }

        private void OnConnectionStateReceived(ConnectionState obj)
        {
            Debug.Log("Sending Ip-Address");
            Debug.Log(NetworkManager.Instance.MulticastMessage(new IpAddressData
            {
                Endpoint = NetworkExtensions.GetLocalEndpoint(targetPort,false),
            }, _endpoint.Port, this.GetInstanceID()));
        }
    }
}