using System;
using System.Collections.Generic;
using DistractorTask.Core;
using Unity.Collections;
using Unity.Networking.Transport;

namespace DistractorTask.Transport
{
    public class NetworkManager : Singleton<NetworkManager>, INetworkManager2
    {

        private List<NetworkConnectionHandler> _handlers;
        private readonly NetworkMessageEventHandler _eventHandler;
        
        public void RegisterCallback<T>(Action<T> callback) where T : ISerializer, new()
        {
            _eventHandler.RegisterCallback(callback);
        }

        public void UnregisterCallback<T>(Action<T> callback) where T : ISerializer, new()
        {
            _eventHandler.UnregisterCallback(callback);
        }

        public bool StartListening(NetworkEndpoint endpoint, Action onConnectionEstablished, ConnectionType connectionType = ConnectionType.Broadcast)
        {
            //for our server it is fine to have two distinct listeners, I assume
            if (TryGetHandler(endpoint, out var handler,false))
            {
                if (handler.ConnectionType == ConnectionType.Unicast)
                {
                    handler.ConnectionType = connectionType;
                }
                return handler.StartListening(endpoint);
            }

            handler = new NetworkConnectionHandler().AsServer(endpoint);
            handler.ConnectionType = connectionType;
            //todo handle events in here 
            _handlers.Add(handler);
            return handler.Driver.Listening;
        }

        public void Connect(NetworkEndpoint endpoint, Action<ConnectionState> onConnectionStateReceived)
        {
            if (TryGetHandler(endpoint, out var handler,true))
            {
                //we already have a handler as a server
                handler.Internal = true;
                return;
            }

            handler = new NetworkConnectionHandler().AsClient(endpoint);
            handler.ConnectionType = ConnectionType.Unicast;
            _handlers.Add(handler);
        }

        public ConnectionState CheckConnectionStatus(NetworkEndpoint endpoint)
        {
            if (TryGetHandler(endpoint, out var handler, true))
            {
                return handler.ConnectionState;
            }

            return ConnectionState.Default;
        }

        public bool BroadcastMessage<T>(T data) where T : ISerializer, new()
        {
            var success = false;
            foreach (var handler in _handlers)
            {
                if (handler.ConnectionType == ConnectionType.Unicast)
                {
                    continue;
                }

                success |= SendMessage(handler, data);
            }

            return success;

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
            if (handler.Internal)
            {
                _eventHandler.TriggerCallback(data);
            }

            return success;

        }

        public bool Multicast<T>(T data, NetworkEndpoint endpoint) where T : ISerializer, new()
        {
            if (TryGetHandler(endpoint, out var handler, true))
            {
                return SendMessage(handler, data);
            }

            return false;
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
}