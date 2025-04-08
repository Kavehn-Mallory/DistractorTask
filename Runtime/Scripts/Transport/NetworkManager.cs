using System;
using System.Collections.Generic;
using DistractorTask.Core;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace DistractorTask.Transport
{
    public class NetworkManager : Singleton<NetworkManager>, INetworkManager
    {

        private NetworkMessageEventHandler _globalHandler = new();
        private Dictionary<ushort, ConnectionObject> _activeConnections = new();
        
        public void RegisterCallback<T>(Action<T, int> callback, ushort port) where T : ISerializer, new()
        {
            if (!_activeConnections.TryGetValue(port, out var connectionObject))
            {
                return;
            }
            connectionObject.EventHandler.RegisterCallback(callback);
        }

        public void RegisterCallbackAllPorts<T>(Action<T, int> callback) where T : ISerializer, new()
        {
            _globalHandler.RegisterCallback(callback);
        }

        public void UnregisterCallback<T>(Action<T, int> callback, ushort port) where T : ISerializer, new()
        {
            if (!_activeConnections.TryGetValue(port, out var connectionObject))
            {
                return;
            }
            connectionObject.EventHandler.UnregisterCallback(callback);
        }
        
        public void UnregisterCallbackAllPorts<T>(Action<T, int> callback) where T : ISerializer, new()
        {
            _globalHandler.UnregisterCallback(callback);
        }

        public bool StartListening(ushort port, Action<ConnectionState> onConnectionStateChanged, ConnectionType connectionType = ConnectionType.Broadcast)
        {
            if (_activeConnections.TryGetValue(port, out var connectionObject))
            {
                //todo update connection type?
                onConnectionStateChanged?.Invoke(connectionObject.ConnectionHandler.ConnectionState);
                RegisterToConnectionStateChange(port, onConnectionStateChanged);
                var endpoint = NetworkExtensions.GetLocalEndpoint(port, true);
                return connectionObject.ConnectionHandler.StartListening(endpoint, connectionType);
            }

            connectionObject = new ConnectionObject
            {
                ConnectionType = connectionType,
                ConnectionHandler = new NetworkConnectionHandler(OnDataReceived, OnConnectionStateChanged),
                EventHandler = new NetworkMessageEventHandler(),
                HasLocalConnection = false,
                OnConnectionStateChange = _ => {}
            };
            _activeConnections.Add(port, connectionObject);
            RegisterToConnectionStateChange(port, onConnectionStateChanged);
            
            //todo do we have the connection type twice?
            
            return connectionObject.ConnectionHandler.StartListening(NetworkExtensions.GetLocalEndpoint(port, true),
                connectionType);
        }

        private void OnConnectionStateChanged(ushort port, ConnectionState connectionState)
        {
            if (_activeConnections.TryGetValue(port, out var connectionObject))
            {
                connectionObject.OnConnectionStateChange?.Invoke(connectionState);
            }
        }

        private void OnDataReceived(ref DataStreamReader stream, ushort port)
        {
            var typeIndex = stream.ReadByte();
            var type = DataSerializationIndexer.GetTypeForTypeIndex(typeIndex);
            
            if (!_globalHandler.TriggerCallback(type, ref stream, out var data))
            {
                Debug.LogError($"Type {type} is not handled yet by {nameof(NetworkMessageEventHandler)}. This either means that {type} does not implement {nameof(ISerializer)} or that the type does not have a default constructor");
                return;
            }

            if (_activeConnections.TryGetValue(port, out var connectionObject))
            {
                connectionObject.EventHandler.TriggerCallback(type, data, 0);
            }
        }

        public void Connect(NetworkEndpoint endpoint, Action<ConnectionState> onConnectionStateChanged, ConnectionType connectionType = ConnectionType.Broadcast)
        {
            var isLocal = endpoint.IsLocalAddress();

            if (_activeConnections.TryGetValue(endpoint.Port, out var connectionObject))
            {
                
                //todo update connection type if necessary 
                if (isLocal)
                {
                    Debug.Log("We are already listening and this is a local thing");
                    connectionObject.HasLocalConnection = true;
                    RegisterToConnectionStateChange(endpoint.Port, onConnectionStateChanged);
                    connectionObject.OnConnectionStateChange.Invoke(ConnectionState.Connected);
                    return;
                }

                if (connectionObject.IsConnectedTo(endpoint))
                {
                    RegisterToConnectionStateChange(endpoint.Port, onConnectionStateChanged);
                    onConnectionStateChanged?.Invoke(ConnectionState.Connected);
                    return;
                }
                
                connectionObject.ConnectionHandler.Connect(endpoint, connectionType);
                return;
            }

            connectionObject = new ConnectionObject()
            {
                ConnectionType = connectionType,
                ConnectionHandler = new NetworkConnectionHandler(OnDataReceived, OnConnectionStateChanged),
                EventHandler = new NetworkMessageEventHandler(),
                HasLocalConnection = isLocal,
                OnConnectionStateChange = _ => {}
            };
            _activeConnections.Add(endpoint.Port, connectionObject);
            RegisterToConnectionStateChange(endpoint.Port, onConnectionStateChanged);

            if (!isLocal)
            {
                connectionObject.ConnectionHandler.Connect(endpoint, connectionType);
            }

        }

        public ConnectionState CheckConnectionStatus(NetworkEndpoint endpoint)
        {
            throw new NotImplementedException();
        }

        public bool BroadcastMessage<T>(T data, int callerId, bool suppressInternalGlobalCall = false) where T : ISerializer, new()
        {
            bool local = false;
            bool success = false;
            foreach (var activeConnectionObject in _activeConnections.Values)
            {
                if (activeConnectionObject.HasLocalConnection)
                {
                    activeConnectionObject.EventHandler.TriggerCallback(data, callerId);
                    local = true;
                }

                success |= SendMessage(activeConnectionObject.ConnectionHandler, data);
            }

            if (local && !suppressInternalGlobalCall)
            {
                _globalHandler.TriggerCallback(data, callerId);
            }

            return success;
        }
        


        public bool MulticastMessage<T>(T data, ushort port, int callerId) where T : ISerializer, new()
        {
            if(_activeConnections.TryGetValue(port, out var activeConnectionObject))
            {
                if (activeConnectionObject.HasLocalConnection)
                {
                    activeConnectionObject.EventHandler.TriggerCallback(data, callerId);
                    _globalHandler.TriggerCallback(data, callerId);
                }

                return SendMessage(activeConnectionObject.ConnectionHandler, data);
            }

            return false;
        }
        
        public bool UnicastMessage<T>(T data, NetworkEndpoint endpoint, int callerId) where T : ISerializer, new()
        {
            if(_activeConnections.TryGetValue(endpoint.Port, out var activeConnectionObject))
            {
                if (activeConnectionObject.HasLocalConnection)
                {
                    activeConnectionObject.EventHandler.TriggerCallback(data, callerId);
                    _globalHandler.TriggerCallback(data, callerId);
                }

                return SendMessage(activeConnectionObject.ConnectionHandler, data, endpoint);
            }

            return false;
        }
        
        private bool SendMessage<T>(NetworkConnectionHandler handler, T data) where T : ISerializer, new()
        {
            var success = false;
            foreach (var connection in handler.Connections)
            {
                var beginSend = handler.Driver.BeginSend(handler.Pipeline, connection, out var writer) == 0;
                writer.SendMessage(data);
                var endSend = handler.Driver.EndSend(writer) >= 0;
                if (beginSend && endSend)
                {
                    success = true;
                }
            }

            return success;

        }
        
        private bool SendMessage<T>(NetworkConnectionHandler handler, T data, NetworkEndpoint targetEndpoint) where T : ISerializer, new()
        {
            var success = false;
            foreach (var connection in handler.Connections)
            {
                if (handler.Driver.GetRemoteEndpoint(connection) != targetEndpoint)
                {
                    continue;
                }
                var beginSend = handler.Driver.BeginSend(handler.Pipeline, connection, out var writer) == 0;
                writer.SendMessage(data);
                var endSend = handler.Driver.EndSend(writer) >= 0;
                if (beginSend && endSend)
                {
                    success = true;
                }
            }

            return success;

        }

        public void RegisterToConnectionStateChange(ushort endpointPort, Action<ConnectionState> onConnectionStateChanged)
        {
            if (onConnectionStateChanged == null)
            {
                return;
            }
            if (_activeConnections.TryGetValue(endpointPort, out var connectionObject))
            {
                connectionObject.OnConnectionStateChange += onConnectionStateChanged;
            }
        }

        public void UnregisterToConnectionStateChange(ushort endpointPort, Action<ConnectionState> onConnectionStateChanged)
        {
            if (onConnectionStateChanged == null)
            {
                return;
            }
            if (_activeConnections.TryGetValue(endpointPort, out var connectionObject))
            {
                connectionObject.OnConnectionStateChange -= onConnectionStateChanged;
            }
        }
    }

    internal class ConnectionObject
    {
        public bool HasLocalConnection;
        public ConnectionType ConnectionType;
        public NetworkConnectionHandler ConnectionHandler;
        public NetworkMessageEventHandler EventHandler;
        public Action<ConnectionState> OnConnectionStateChange;

        public bool IsConnectedTo(NetworkEndpoint endpoint)
        {
            return ConnectionHandler.IsConnectedTo(endpoint);
        }
    }
}