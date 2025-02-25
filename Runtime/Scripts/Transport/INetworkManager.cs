using System;
using DistractorTask.Core;
using Unity.Networking.Transport;

namespace DistractorTask.Transport
{
    public interface INetworkManager
    {
        public void RegisterCallback<T>(Action<T> callback) where T : ISerializer, new();
        
        public void UnregisterCallback<T>(Action<T> callback) where T : ISerializer, new();
        
        public bool StartListening(NetworkEndpoint endpoint, Action<ConnectionState> onConnectionStateChanged, ConnectionType connectionType);

        public void Connect(NetworkEndpoint endpoint, Action<ConnectionState> onConnectionStateChanged, ConnectionType connectionType);

        public ConnectionState CheckConnectionStatus(NetworkEndpoint endpoint);

        public bool BroadcastMessage<T>(T data) where T : ISerializer, new();

        public bool Multicast<T>(T data, NetworkEndpoint endpoint) where T : ISerializer, new();

        public void RegisterToConnectionStateChange(NetworkEndpoint endpoint,
            Action<ConnectionState> onConnectionStateChanged);
        
        public void UnregisterToConnectionStateChange(NetworkEndpoint endpoint,
            Action<ConnectionState> onConnectionStateChanged);
    }

    public enum ConnectionState : byte
    {
        Default = 0,
        ConnectionRequested = 1,
        Connected = 2,
        Timeout = 3,
        MaxConnectionAttempts = 4,
        ClosedByRemote = 5,
        AuthenticationFailure = 6,
        ProtocolError = 7,
    }
}