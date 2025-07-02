using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using TMPro;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

namespace DistractorTask.UI
{
    public class IpInputHandler : MonoBehaviour
    {
        public TMP_InputField inputFieldP0;
        public TMP_InputField inputFieldP1;
        public TMP_InputField inputFieldP2;
        public TMP_InputField inputFieldP3;
        public TMP_InputField inputFieldPort;
        
        
        public TMP_InputField inputFieldLocalEndpointP0;
        public TMP_InputField inputFieldLocalEndpointP1;
        public TMP_InputField inputFieldLocalEndpointP2;
        public TMP_InputField inputFieldLocalEndpointP3;

        public Toggle useLocalEndpoint;


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
                Debug.Log($"Starting to listen on {NetworkExtensions.GetLocalEndpoint(targetPort, true)}");
                NetworkManager.Instance.StartListening(targetPort,
                    OnConnectionEstablished);
                _endpoint = NetworkEndpoint.Parse($"{p0}.{p1}.{p2}.{p3}", port);
                Debug.Log($"Trying to connect to {_endpoint}");
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
            var endpoint = NetworkExtensions.GetLocalEndpoint(targetPort, false);
            if (useLocalEndpoint.isOn && int.TryParse(inputFieldLocalEndpointP0.text, out var p0) && int.TryParse(inputFieldLocalEndpointP1.text, out var p1) &&
                int.TryParse(inputFieldLocalEndpointP2.text, out var p2) && int.TryParse(inputFieldLocalEndpointP3.text, out var p3))
            {
                endpoint = NetworkEndpoint.Parse($"{p0}.{p1}.{p2}.{p3}", targetPort);
            }
            NetworkManager.Instance.UnicastMessage(new IpAddressData
            {
                Endpoint = endpoint,
            }, _endpoint, this.GetInstanceID());
        }
    }
}