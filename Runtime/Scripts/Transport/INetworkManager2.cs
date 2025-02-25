using System;
using DistractorTask.Core;
using Unity.Networking.Transport;

namespace DistractorTask.Transport
{
    public interface INetworkManager2
    {
        public void RegisterCallback<T>(Action<T> callback) where T : ISerializer, new();
        
        public void UnregisterCallback<T>(Action<T> callback) where T : ISerializer, new();
        
        public bool StartListening(NetworkEndpoint endpoint, Action onConnectionEstablished, ConnectionType connectionType);

        public void Connect(NetworkEndpoint endpoint, Action<ConnectionState> onConnectionStateReceived);

        public ConnectionState CheckConnectionStatus(NetworkEndpoint endpoint);

        public bool BroadcastMessage<T>(T data) where T : ISerializer, new();

        public bool Multicast<T>(T data, NetworkEndpoint endpoint) where T : ISerializer, new();
    }

    public enum ConnectionState : byte
    {
        Default = 0,
        Connected = 1,
        Timeout = 2,
        MaxConnectionAttempts = 3,
        ClosedByRemote = 4,
        AuthenticationFailure = 5,
        ProtocolError = 6,
    }
}