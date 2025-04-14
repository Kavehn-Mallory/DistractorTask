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

        private TaskCompletionSource<TestData> _testTaskThing;

        private ushort _tempPort;

        private int _messageId;

        private MarkerPointVisTester _tester;


        private void Awake()
        {
            
            _tempPort = NetworkExtensions.DefaultPort;
            _tester = new MarkerPointVisTester(this, NetworkExtensions.DefaultPort);
            //this does work because it is a local connection so we do not have to wait for anything to be set up 
            NetworkManager.Instance.StartListening(_tempPort, null, ConnectionType.Multicast);
            NetworkManager.Instance.Connect(NetworkEndpoint.AnyIpv4.WithPort(_tempPort), null, ConnectionType.Multicast);
        }
        
        

        
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
        }


        public async Task InitializeMarkerPointSetup(int markerCount)
        {
            //todo send message with marker point count / positions 
            await SendMessageAndWaitForResponse(new MarkerCountData
            {
                markerCount = markerCount
            }, _tempPort);
        }


        public async Task StartMarkerPointSetup()
        {
            //todo send message that transmits the start data 
            //should activate canvas and first point
            await SendMessageAndWaitForResponse(new MarkerPointStartData(), _tempPort);
        }

        public async Task TriggerPoint(int index)
        {
            await SendMessageAndWaitForResponse(new MarkerPointData(index), _tempPort);
        }

        public async Task EndMarkerPointSetup()
        {
            await SendMessageAndWaitForResponse(new MarkerPointEndData(), _tempPort);
        }

        private void OnResponseReceived()
        {
            _testTaskThing.SetResult(new TestData());
        }
        
        private async Task SendMessageAndWaitForResponse<T>(T data, ushort targetPort) where T : IRespondingSerializer<MarkerPointResponseData>, new()
        {
            var task = ScheduleSendAndReceive<MarkerPointResponseData, T>(data, targetPort, GetInstanceID(), _messageId);
            _messageId++;
            if (waitForResponse)
            {
                await task.Task;
            }

        }

        private static TaskCompletionSource<T> ScheduleSendAndReceive<T, TS>(TS data, ushort targetPort, int senderId,
            int messageId) where T : ISerializer, IResponseIdentifier, new() where TS : IRespondingSerializer<T>, new()
        {
            var returnValue = new TaskCompletionSource<T>();
            var testStruct = new CallbackStruct<T>()
            {
                Source = returnValue,
                ExpectedReturnValue = new T
                {
                    MessageId = messageId,
                    SenderId = senderId
                }
            };

            data.MessageId = messageId;
            data.SenderId = senderId;
            
            
            NetworkManager.Instance.RegisterCallback<T>(testStruct.Callback, targetPort);
            NetworkManager.Instance.MulticastMessage(data, targetPort, senderId);
            
            return testStruct.Source;
        }

        private static void Respond<T>(IRespondingSerializer<T> respondingSerializer, ushort targetPort, int callerId) where T : IResponseIdentifier, ISerializer, new() 
        {
            NetworkManager.Instance.MulticastMessage(respondingSerializer.GenerateResponse(), targetPort, callerId);
        }

        private struct CallbackStruct<T> where T : ISerializer, IResponseIdentifier, new()
        {
            public TaskCompletionSource<T> Source;
            public T ExpectedReturnValue;

            public void Callback(T data, int id)
            {
                if (data.SenderId == ExpectedReturnValue.SenderId && data.MessageId == ExpectedReturnValue.MessageId)
                {
                    Debug.Log("Data received and setting result");
                    NetworkManager.Instance.UnregisterCallback<T>(Callback, NetworkExtensions.DefaultPort);
                    Source.SetResult(data);
                }
                
            }
        }
        

        public struct TestData
        {
            
        }

        public class MarkerPointVisTester
        {
            private readonly MarkerPointVisualizationController _controller;
            private readonly ushort _port;

            public MarkerPointVisTester(MarkerPointVisualizationController controller, ushort port)
            {
                _controller = controller;
                _port = port;
                NetworkManager.Instance.RegisterCallback<MarkerPointStartData>(OnMarkerPointStartDataReceived, _port);
            }

            private async void OnMarkerPointStartDataReceived(MarkerPointStartData arg1, int arg2)
            {
                await Task.Delay(1000);
                NetworkManager.Instance.MulticastMessage(arg1.GenerateResponse(), _port, 0);
            }


            public async void SendMessageAndAwaitResponse()
            {
                Debug.Log("Sending message");
                await _controller.StartMarkerPointSetup();
                Debug.Log("End call");
            }
            
            public void SendMessageWithoutAwaitResponse()
            {
                Debug.Log("Sending message");
                _ = _controller.StartMarkerPointSetup();
                Debug.Log("End call");
            }
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
    
    public class MarkerPointStartData : BaseRespondingData<MarkerPointResponseData>
    {

        public MarkerPointStartData()
        {
            
        }
        
        public MarkerPointStartData(int senderId, int messageId)
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