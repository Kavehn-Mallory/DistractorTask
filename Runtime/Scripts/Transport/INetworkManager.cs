﻿using System;
using DistractorTask.Core;
using Unity.Networking.Transport;

namespace DistractorTask.Transport
{
    public interface INetworkManager
    {
        public void RegisterCallback<T>(Action<T, int> callback) where T : ISerializer, new();
        
        public void UnregisterCallback<T>(Action<T, int> callback) where T : ISerializer, new();
        
        public bool StartListening(NetworkEndpoint endpoint, Action<ConnectionState> onConnectionStateChanged, ConnectionType connectionType);

        public void Connect(NetworkEndpoint endpoint, Action<ConnectionState> onConnectionStateChanged, ConnectionType connectionType);

        public ConnectionState CheckConnectionStatus(NetworkEndpoint endpoint);

        public bool BroadcastMessage<T>(T data, int callerId) where T : ISerializer, new();

        public bool MulticastMessage<T>(T data, NetworkEndpoint endpoint, int callerId) where T : ISerializer, new();

        public void RegisterToConnectionStateChange(ushort endpointPort,
            Action<ConnectionState> onConnectionStateChanged);
        
        public void UnregisterToConnectionStateChange(ushort endpointPort,
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