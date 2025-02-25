using System;
using System.Collections.Generic;
using DistractorTask.Core;
using DistractorTask.Transport.DataContainer;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace DistractorTask.Transport
{
    public class NetworkManager : Singleton<NetworkManager>, INetworkManager
    {


        public Action<string> DebugAction = delegate { };
        
        private readonly List<NetworkConnectionHandler> _handlers = new();
        private readonly NetworkMessageEventHandler _eventHandler = new();

        private readonly List<OnConnectionChangedData> _onConnectionStateChange = new();
        
        public void RegisterCallback<T>(Action<T, int> callback) where T : ISerializer, new()
        {
            if (typeof(T) == typeof(UserStudyBeginData))
            {
                Debug.Log("Registered");
            }
            _eventHandler.RegisterCallback(callback);
        }

        public void UnregisterCallback<T>(Action<T, int> callback) where T : ISerializer, new()
        {
            _eventHandler.UnregisterCallback(callback);
        }

        public bool StartListening(NetworkEndpoint endpoint, Action<ConnectionState> onConnectionStateChanged, ConnectionType connectionType = ConnectionType.Broadcast)
        {
            //for our server it is fine to have two distinct listeners, I assume
            if (TryGetHandler(endpoint, out var handler,false))
            {
                if (handler.ConnectionType == ConnectionType.Multicast)
                {
                    handler.ConnectionType = connectionType;
                }
                onConnectionStateChanged?.Invoke(handler.ConnectionState);
                RegisterToConnectionStateChange(endpoint.Port, onConnectionStateChanged);
                return handler.StartListening(endpoint, connectionType);
            }

            handler = new NetworkConnectionHandler(OnDataReceived, OnConnectionStateChanged);
            RegisterToConnectionStateChange(endpoint.Port, onConnectionStateChanged);
            handler.StartListening(endpoint, connectionType);
            
            _handlers.Add(handler);
            return handler.Driver.Listening;
        }

        private void OnDataReceived(ref DataStreamReader stream)
        {
            var typeIndex = stream.ReadByte();
            var type = DataSerializationIndexer.GetTypeForTypeIndex(typeIndex);
            
            DebugAction.Invoke($"Received {type}");
            if (!_eventHandler.TriggerCallback(type, ref stream))
            {
                Debug.LogError($"Type {type} is not handled yet by {nameof(NetworkMessageEventHandler)}. This either means that {type} does not implement {nameof(ISerializer)} or that the type does not have a default constructor");
            }
        }

        public void Connect(NetworkEndpoint endpoint, Action<ConnectionState> onConnectionStateChanged, ConnectionType connectionType = ConnectionType.Broadcast)
        {
            if (TryGetHandler(endpoint, out var handler,true))
            {
                Debug.Log("Found a handler");
                //we already have a handler as a server
                
                if (handler.ConnectionType == ConnectionType.Multicast)
                {
                    handler.ConnectionType = connectionType;
                }
                
                DebugAction.Invoke("Local");
                
                if (NetworkHelper.IsLocalAddress(endpoint))
                {
                    handler.EstablishInternalConnection();
                }
                else if(!handler.ContainsEndpoint(endpoint.Port))
                {
                    handler.Connect(endpoint, handler.ConnectionType);
                }
                    
                onConnectionStateChanged?.Invoke(handler.ConnectionState);
                RegisterToConnectionStateChange(endpoint.Port, onConnectionStateChanged);
                
                
                return;
            }
            DebugAction.Invoke("Global");
            Debug.Log("No handler exists yet");
            RegisterToConnectionStateChange(endpoint.Port, onConnectionStateChanged);
            handler = new NetworkConnectionHandler(OnDataReceived, OnConnectionStateChanged, 1);
            handler.Connect(endpoint, connectionType);
            
            
            _handlers.Add(handler);
        }

        private void OnConnectionStateChanged(ushort endpoint, ConnectionState connectionState)
        {
            Debug.Log("Looking for state-change-object", this);
            for (var i = 0; i < _onConnectionStateChange.Count; i++)
            {
                var changedData = _onConnectionStateChange[i];
                if (changedData.EndpointPort == endpoint)
                {
                    Debug.Log("Found a connection state object");
                    changedData.OnStateChange.Invoke(connectionState);
                    continue;
                }

                Debug.Log($"Thats not it: {changedData.EndpointPort} vs {endpoint}");
            }
        }

        public ConnectionState CheckConnectionStatus(NetworkEndpoint endpoint)
        {
            if (TryGetHandler(endpoint, out var handler, true))
            {
                return handler.ConnectionState;
            }

            return ConnectionState.Default;
        }

        public bool BroadcastMessage<T>(T data, int callerId) where T : ISerializer, new()
        {
            var success = false;
            foreach (var handler in _handlers)
            {
                if (handler.ConnectionType == ConnectionType.Multicast)
                {
                    continue;
                }
                success |= SendMessage(handler, data);
            }

            if (callerId != 0)
            {
                SendMessageLocally(data, callerId);
            }
            
            return success;

        }

        private bool SendMessage<T>(NetworkConnectionHandler handler, T data) where T : ISerializer, new()
        {
            var success = false;
            foreach (var connection in handler.Connections)
            {
                var beginSend = handler.Driver.BeginSend(handler.Pipeline, connection, out var writer) == 0;
                Debug.Log(beginSend);
                writer.SendMessage(data);
                var endSend = handler.Driver.EndSend(writer) >= 0;
                if (beginSend && endSend)
                {
                    success = true;
                }
            }

            return success;

        }

        private void SendMessageLocally<T>(T data, int callerId)
            where T : ISerializer, new()
        {
            _eventHandler.TriggerCallback(data, callerId);
        }

        public bool MulticastMessage<T>(T data, NetworkEndpoint endpoint, int callerId) where T : ISerializer, new()
        {
            if (TryGetHandler(endpoint, out var handler, true))
            {
                var result = SendMessage(handler, data);

                if (callerId != 0)
                {
                    Debug.Log($"Local send: {typeof(T)}");
                    SendMessageLocally(data, callerId);
                }
                return result;
            }

            return false;
        }

        public void RegisterToConnectionStateChange(ushort endpointPort, Action<ConnectionState> onConnectionStateChanged)
        {
            if (onConnectionStateChanged == null)
            {
                return;
            }
            for (var i = 0; i < _onConnectionStateChange.Count; i++)
            {
                var connectionChangedData = _onConnectionStateChange[i];
                if (connectionChangedData.EndpointPort == endpointPort)
                {
                    connectionChangedData.OnStateChange += onConnectionStateChanged;
                    _onConnectionStateChange[i] = connectionChangedData;
                    return;
                }
            }
            
            _onConnectionStateChange.Add(new OnConnectionChangedData(endpointPort, onConnectionStateChanged));

        }

        public void UnregisterToConnectionStateChange(ushort endpointPort, Action<ConnectionState> onConnectionStateChanged)
        {
            if (onConnectionStateChanged == null)
            {
                return;
            }
            for (var i = 0; i < _onConnectionStateChange.Count; i++)
            {
                var connectionChangedData = _onConnectionStateChange[i];
                if (connectionChangedData.EndpointPort == endpointPort)
                {
                    connectionChangedData.OnStateChange -= onConnectionStateChanged;
                    _onConnectionStateChange[i] = connectionChangedData;
                    return;
                }
            }
        }
        
        private void Update()
        {
            for (var index = _handlers.Count - 1; index >= 0; index--)
            {
                var handler = _handlers[index];
                handler.UpdateConnectionHandler();
            }
        }

        private void OnDestroy()
        {
            foreach (var handler in _handlers)
            {
                handler.Dispose();
            }
            
        }
        
        
        private bool TryGetHandler(NetworkEndpoint endpoint, out NetworkConnectionHandler existingHandler, bool loopbackCheck)
        {
            existingHandler = default;
            foreach (var handler in _handlers)
            {
                if (handler.ContainsEndpoint(endpoint.Port))
                {
                    existingHandler = handler;
                    return true;
                }
            }

            return false;
        }
    }

    public struct OnConnectionChangedData
    {
        public readonly ushort EndpointPort;
        public Action<ConnectionState> OnStateChange;

        public OnConnectionChangedData(ushort endpointPort, Action<ConnectionState> onStateChange)
        {
            EndpointPort = endpointPort;
            OnStateChange = onStateChange;
        }
        
    }
}