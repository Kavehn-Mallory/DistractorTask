using System;
using System.Threading.Tasks;
using DistractorTask.Core;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using DistractorTask.Transport.DataContainer.GenericClasses;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace DistractorTask.UserStudy.MarkerPointStage
{
    /// <summary>
    /// Responsible to  send messages to the client responsible for displaying them
    /// </summary>
    public class MarkerPointVisualizationController : MonoBehaviour
    {

        /// <summary>
        /// If set to true, the controller will await a response from the connected client before returning the control to the study. Otherwise, it will immediately return after sending the message 
        /// </summary>
        [SerializeField]
        private bool waitForResponse;

        private NetworkEndpoint _clientEndpoint;
        

        private ushort _tempPort;

        private int _messageId;

       // private MarkerPointVisTester _tester;


        private void Awake()
        {
            
            _tempPort = NetworkExtensions.DisplayWallControlPort;
            //_tester = new MarkerPointVisTester(this, NetworkExtensions.DisplayWallControlPort);
            //this does work because it is a local connection so we do not have to wait for anything to be set up 
            /*NetworkManager.Instance.StartListening(_tempPort, null, ConnectionType.Multicast);
            NetworkManager.Instance.Connect(NetworkEndpoint.AnyIpv4.WithPort(_tempPort), null, ConnectionType.Multicast);*/
        }
        
        

        
        /*
        [ContextMenu("Async Test")]
        public void TestAsyncWithAwaitCall()
        {
            Debug.Log("Test Case: Wait for response");
            _tester.SendMessageAndAwaitResponse();
        }
        
        [ContextMenu("Non-async Test")]
        public void TestAsyncWithoutAwaitCall()
        {
            Debug.Log("Test Case: Do not wait for response");
            _tester.SendMessageWithoutAwaitResponse();
        }*/


        public async Task InitializeMarkerPointSetup(int markerCount)
        {
            //todo send message with marker point count / positions 
            await NetworkManager.Instance.MulticastMessageAndAwaitResponse<MarkerCountData, MarkerPointResponseData>(new MarkerCountData
            {
                markerCount = markerCount
            }, _tempPort, GetInstanceID(), _messageId);
            _messageId++;
        }
        
        public async Task TriggerNextPoint(int index)
        {
            await NetworkManager.Instance.MulticastMessageAndAwaitResponse<ActivateMarkerPoint, OnMarkerPointActivatedData>(new ActivateMarkerPoint(index), _tempPort, GetInstanceID(), _messageId);
            _messageId++;
        }

        public async Task EndMarkerPointSetup()
        {
            await NetworkManager.Instance.MulticastMessageAndAwaitResponse<MarkerPointEndData, MarkerPointResponseData>(new MarkerPointEndData(), _tempPort, GetInstanceID(), _messageId);
            _messageId++;
        }
        

        

    }

    public class MarkerPointResponseData : BaseResponseData
    {
        
        

        public MarkerPointResponseData(int senderId, int messageId)
        {
            this.SenderId = senderId;
            this.MessageId = messageId;
        }

        public MarkerPointResponseData()
        {

        }
        

        
    }

    [Serializable]
    public class MarkerPointEndData : BaseRespondingData<MarkerPointResponseData>
    {
        public MarkerPointEndData()
        {
        }

        public MarkerPointEndData(int senderId, int messageId)
        {
            SenderId = senderId;
            MessageId = messageId;
        }
        
        public override MarkerPointResponseData GenerateResponse() => new MarkerPointResponseData(SenderId, MessageId);
        
    }
    

    [Serializable]
    public class MarkerPointData : BaseRespondingData<MarkerPointResponseData>
    {

        public int index;

        public MarkerPointData()
        {
        }

        public MarkerPointData(int index, int senderId, int messageId)
        {
            this.index = index;
            this.SenderId = senderId;
            this.MessageId = messageId;
        }
        
        public MarkerPointData(int index)
        {
            this.index = index;
            this.SenderId = 0;
            this.MessageId = 0;
        }
        
        public override MarkerPointResponseData GenerateResponse() => new MarkerPointResponseData(SenderId, MessageId);

        public override void Serialize(ref DataStreamWriter writer)
        {
            base.Serialize(ref writer);
            writer.WriteInt(index);
        }

        public override void Deserialize(ref DataStreamReader reader)
        {
            base.Deserialize(ref reader);
            index = reader.ReadInt();
        }
    }
}