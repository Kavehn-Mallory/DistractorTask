using System;
using DistractorTask.Core;
using DistractorTask.Logging;
using Unity.Networking.Transport;

namespace DistractorTask.Transport
{
    public interface INetworkManager
    {
        public void RegisterCallback<T>(Action<T> callback) where T : ISerializer, new();

        public void UnregisterCallback<T>(Action<T> callback) where T : ISerializer, new();

        public bool TransmitNetworkMessage(ISerializer data);

        public event Action OnAutoConnectionEstablished;
        
        public NetworkEndpoint NetworkEndpoint { get; }
    }
}