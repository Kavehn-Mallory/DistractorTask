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

        /// <summary>
        /// Called when a connection message is received or the connection times out or fails in other ways 
        /// </summary>
        public event Action<bool> OnConnectionEstablished;
        
        public NetworkEndpoint NetworkEndpoint { get; }
    }
}