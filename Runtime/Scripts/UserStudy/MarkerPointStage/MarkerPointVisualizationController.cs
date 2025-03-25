using System;
using System.Threading.Tasks;
using DistractorTask.Core;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
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
        
        private async Task SendMessageAndWaitForResponse<T>(T data, ushort targetPort) where T : ISerializer, new()
        {
            var task = ScheduleSendAndReceive<MarkerPointResponseData, T>(data, targetPort);

            if (waitForResponse)
            {
                await task.Task;
            }

        }

        private TaskCompletionSource<T> ScheduleSendAndReceive<T, TS>(TS data, ushort targetPort) where T : ISerializer, new() where TS : ISerializer, new()
        {
            var returnValue = new TaskCompletionSource<T>();
            var testStruct = new CallbackStruct<T>()
            {
                Source = returnValue
            };
            NetworkManager.Instance.RegisterCallback<T>(testStruct.Callback);
            NetworkManager.Instance.MulticastMessage(data, NetworkEndpoint.AnyIpv4.WithPort(targetPort),
                GetInstanceID());
            
            return testStruct.Source;
        }

        private struct CallbackStruct<T> where T : ISerializer, new()
        {
            public TaskCompletionSource<T> Source;

            public void Callback(T data, int id)
            {
                NetworkManager.Instance.UnregisterCallback<T>(Callback);
                Source.SetResult(data);
            }
        }
        

        public struct TestData
        {
            
        }

    }

    internal struct MarkerPointResponseData : ISerializer
    {
        public void Serialize(ref DataStreamWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public struct MarkerPointEndData : ISerializer
    {
        public void Serialize(ref DataStreamWriter writer)
        {
            
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            
        }
    }
    
    public struct MarkerPointStartData : ISerializer
    {
        public void Serialize(ref DataStreamWriter writer)
        {
            
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            
        }
    }
    

    [Serializable]
    public struct MarkerPointData : ISerializer
    {

        public int index;
        
        public MarkerPointData(int index)
        {
            this.index = index;
        }

        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteInt(index);
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            index = reader.ReadInt();
        }
    }
}