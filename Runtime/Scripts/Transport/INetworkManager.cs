using System;
using DistractorTask.Core;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;

namespace DistractorTask.Transport
{
    public interface INetworkManager
    {
        public void RegisterCallback<T>(Action<T> callback) where T : ISerializer, new();

        public void UnregisterCallback<T>(Action<T> callback) where T : ISerializer, new();

        public bool TransmitNetworkMessage(ISerializer data);

        /// <summary>
        /// Called when a connection message is received
        /// </summary>
        public event Action<bool> OnConnectionEstablished;
        

        /// <summary>
        /// Called when a connection is terminated 
        /// </summary>
        public event Action<NetworkEndpoint, DisconnectReason> OnConnectionDisconnected;
        
        public NetworkEndpoint NetworkEndpoint { get; }

        
    }
}