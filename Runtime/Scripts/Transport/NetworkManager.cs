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
                RegisterToConnectionStateChange(endpoint, onConnectionStateChanged);
                return handler.StartListening(endpoint, connectionType);
            }

            handler = new NetworkConnectionHandler(OnDataReceived, OnConnectionStateChanged);
            RegisterToConnectionStateChange(endpoint, onConnectionStateChanged);
            handler.StartListening(endpoint, connectionType);
            
            _handlers.Add(handler);
            return handler.Driver.Listening;
        }

        private void OnDataReceived(ref DataStreamReader stream)
        {
            var typeIndex = stream.ReadByte();
            var type = DataSerializationIndexer.GetTypeForTypeIndex(typeIndex);
            
            if (type == typeof(UserStudyBeginData))
            {
                Debug.Log("Sending message");
            }

            Debug.Log($"Message of type {type} received");
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

                handler.EstablishInternalConnection();
                onConnectionStateChanged?.Invoke(handler.ConnectionState);
                RegisterToConnectionStateChange(endpoint, onConnectionStateChanged);
                return;
            }
            Debug.Log("No handler exists yet");
            RegisterToConnectionStateChange(endpoint, onConnectionStateChanged);
            handler = new NetworkConnectionHandler(OnDataReceived, OnConnectionStateChanged, 1);
            handler.Connect(endpoint, connectionType);
            
            
            _handlers.Add(handler);
        }

        private void OnConnectionStateChanged(NetworkEndpoint endpoint, ConnectionState connectionState)
        {
            if (!endpoint.IsValid)
            {
                return;
            }
            Debug.Log("Looking for state-change-object", this);
            for (var i = 0; i < _onConnectionStateChange.Count; i++)
            {
                var changedData = _onConnectionStateChange[i];
                if (OnConnectionChangedData.AreEqual(changedData.Endpoint, endpoint))
                {
                    Debug.Log("Found a connection state object");
                    changedData.OnStateChange.Invoke(connectionState);
                    continue;
                }

                Debug.Log($"Thats not it: {changedData.Endpoint} vs {endpoint}");
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
            NetworkConnectionHandler localHandler = null;
            foreach (var handler in _handlers)
            {
                if (handler.Internal)
                {
                    localHandler = handler;
                }
                if (handler.ConnectionType == ConnectionType.Multicast)
                {
                    continue;
                }
                success |= SendMessage(handler, data);
            }

            if (localHandler != null)
            {
                SendMessageLocally(localHandler, data, callerId);
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

        private void SendMessageLocally<T>(NetworkConnectionHandler handler, T data, int callerId)
            where T : ISerializer, new()
        {
            _eventHandler.TriggerCallback(data, callerId);
        }

        public bool MulticastMessage<T>(T data, NetworkEndpoint endpoint, int callerId) where T : ISerializer, new()
        {
            if (TryGetHandler(endpoint, out var handler, true))
            {
                var result = SendMessage(handler, data);

                if (handler.Internal)
                {
                    SendMessageLocally(handler, data, callerId);
                }
                return result;
            }

            return false;
        }

        public void RegisterToConnectionStateChange(NetworkEndpoint endpoint, Action<ConnectionState> onConnectionStateChanged)
        {
            if (onConnectionStateChanged == null)
            {
                return;
            }
            for (var i = 0; i < _onConnectionStateChange.Count; i++)
            {
                var connectionChangedData = _onConnectionStateChange[i];
                if (OnConnectionChangedData.AreEqual(connectionChangedData.Endpoint, endpoint))
                {
                    connectionChangedData.OnStateChange += onConnectionStateChanged;
                    _onConnectionStateChange[i] = connectionChangedData;
                    return;
                }
            }
            
            _onConnectionStateChange.Add(new OnConnectionChangedData(endpoint, onConnectionStateChanged));

        }

        public void UnregisterToConnectionStateChange(NetworkEndpoint endpoint, Action<ConnectionState> onConnectionStateChanged)
        {
            if (onConnectionStateChanged == null)
            {
                return;
            }
            for (var i = 0; i < _onConnectionStateChange.Count; i++)
            {
                var connectionChangedData = _onConnectionStateChange[i];
                if (OnConnectionChangedData.AreEqual(connectionChangedData.Endpoint, endpoint))
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

        private bool TryGetHandlerInternal(NetworkEndpoint endpoint, out NetworkConnectionHandler existingHandler)
        {
            existingHandler = default;
            foreach (var handler in _handlers)
            {
                if (handler.Endpoint.Equals(endpoint))
                {
                    existingHandler = handler;
                    return true;
                }
            }

            return false;
        }
        
        private bool TryGetHandler(NetworkEndpoint endpoint, out NetworkConnectionHandler existingHandler, bool loopbackCheck)
        {
            var isLocalAddress = endpoint.IsLoopback;
            if (!loopbackCheck || !isLocalAddress)
            {
                return TryGetHandlerInternal(endpoint, out existingHandler);
            }
            
            
            foreach (var handler in _handlers)
            {
                if (handler.Endpoint.Equals(endpoint))
                {
                    existingHandler = handler;
                    return true;
                }
                //if(handler.Driver.)
                var localEndpoint = handler.Driver.GetLocalEndpoint();
                if (localEndpoint.Port != endpoint.Port)
                {
                    continue;
                }

                if ((localEndpoint.IsLoopback || localEndpoint == NetworkEndpoint.AnyIpv4.WithPort(endpoint.Port)))
                {
                    existingHandler = handler;
                    return true;
                }
            }

            existingHandler = default;
            return false;
        }
    }

    public struct OnConnectionChangedData
    {
        public NetworkEndpoint Endpoint;
        public Action<ConnectionState> OnStateChange;

        public OnConnectionChangedData(NetworkEndpoint endpoint, Action<ConnectionState> onStateChange)
        {
            Endpoint = endpoint;
            OnStateChange = onStateChange;
        }

        public static bool AreEqual(NetworkEndpoint endpoint, NetworkEndpoint otherEndpoint)
        {
            if (endpoint.Equals(otherEndpoint))
            {
                return true;
            }
            Debug.Log($"{endpoint} vs {otherEndpoint}");
            if (otherEndpoint.Port != endpoint.Port)
            {
                return false;
            }
            
            return (otherEndpoint.IsLoopback || otherEndpoint == NetworkEndpoint.AnyIpv4.WithPort(endpoint.Port)) && (endpoint.IsLoopback || endpoint == NetworkEndpoint.AnyIpv4.WithPort(endpoint.Port));
        }
    }
}