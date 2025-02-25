using System;
using System.Collections.Generic;
using System.Linq;
using DistractorTask.Core;
using DistractorTask.Transport.DataContainer;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;
using UnityEngine;

namespace DistractorTask.Transport
{
    public class NetworkConnectionManager : Singleton<NetworkConnectionManager>, INetworkManager
    {
        //todo we need to check what happens when we try to connect to our own thing
        //todo potentially we still need to accept that there are listening and receiving connections
        //todo potentially creates a problem where we handle data twice if on own device?
        [SerializeField]
        private ConnectionDataSettings settings = new()
        {
            endpointSource = NetworkEndpointSetting.LoopbackIPv4,
            port = new ConnectionPortProperty(7777)
        };
        
        public NetworkEndpoint NetworkEndpoint => settings.NetworkEndpoint;
        
        public Action OnConnectionRequested = delegate { };
        
        public event Action<NetworkEndpoint, DisconnectReason> OnConnectionDisconnected = delegate { };
        
        public event Action<bool> OnConnectionEstablished = delegate { };

        private readonly List<NetworkConnectionHandler> _handlers = new();
        
        private readonly NetworkMessageEventHandler _eventHandler = new();


        private void OnDestroy()
        {
            foreach (var handler in _handlers)
            {
                handler.Dispose();
            }
        }


        public void ListenForRequest(NetworkEndpoint endpoint, NetworkConnectionHandler.ActionRef<DataStreamReader> onDataReceived = null)
        {

            NetworkConnectionHandler.ActionRef<DataStreamReader> combinedDataReceivedEvent = OnDataReceived;
            if (onDataReceived != null)
            {
                combinedDataReceivedEvent += onDataReceived;
            }
            
            var handler = ListenForRequestInternal(_handlers, endpoint, combinedDataReceivedEvent, OnConnectionEstablished, OnDisconnect);
            if (handler != null)
            {
                _handlers.Add(handler);
            }
        }
        
        private static NetworkConnectionHandler ListenForRequestInternal(List<NetworkConnectionHandler> handlers, NetworkEndpoint endpoint, NetworkConnectionHandler.ActionRef<DataStreamReader> onDataReceived, Action<bool> onConnectionEstablished, NetworkConnectionHandler.ActionRef<DataStreamReader, NetworkConnectionHandler> onDisconnect)
        {

            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                if (handler.Internal)
                {
                    //we can skip local instances. They represent clients only and cannot be used as servers 
                    continue;
                }
                if (handler.Driver.GetLocalEndpoint() == endpoint)
                {
                    //we are not listening yet, probably should do that right?
                    if (!handler.Driver.Listening)
                    {
                        if (!handler.Driver.Bound && handler.Driver.Bind(endpoint) != 0)
                        {
                            Debug.LogError($"Failed to bind to port {endpoint.Port}.");
                        }
                        handler.Driver.Listen();
                        
                    }
                    //todo this probably does not work yet because we could have chained more than one delegate together
                    if (onDataReceived != null && !handler.OnDataReceived.GetInvocationList().Contains(onDataReceived))
                    {
                        handler.OnDataReceived += onDataReceived;
                    }
                    return null;
                }
            }

            var server = new NetworkConnectionHandler().AsServer(endpoint);


            Debug.Log($"Creating new handler for connection {endpoint.ToString()}");
            server.OnDisconnect += onDisconnect;
            server.OnConnectionAccepted += onConnectionEstablished;
            server.OnDataReceived += onDataReceived;
            //handlers.Add(server);
            return server;
        }

        
        public void ListenForIpRequest(ushort port, NetworkConnectionHandler.ActionRef<DataStreamReader> onIpDataReceived = null)
        {
            var ip = NetworkConnectionHandler.GetLocalIPAddress();
            var endpoint = NetworkEndpoint.Parse(ip.ToString(), port);
            //if we are listening for ip in the editor we are doing it just locally?
#if UNITY_EDITOR
            endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(port);
#endif


            NetworkConnectionHandler.ActionRef<DataStreamReader> combinedAction = OnIpDataReceived;
            if (onIpDataReceived != null)
            {
                combinedAction += onIpDataReceived;
            }

            var handler = ListenForRequestInternal(_handlers, endpoint, combinedAction, null, null);
            if (handler != null)
            {
                _handlers.Add(handler);
            }
        }
        


        private void OnDisconnect(ref DataStreamReader stream, NetworkConnectionHandler handler)
        {
            var reason = (DisconnectReason) stream.ReadByte();
            OnConnectionDisconnected.Invoke(handler.Driver.GetLocalEndpoint(), reason);
            Debug.Log($"We got disconnected from server due to {reason}.");
        }

        public static bool DidConnectionThrowError(DisconnectReason reason)
        {
            return reason != DisconnectReason.ClosedByRemote && reason != DisconnectReason.Timeout;
        }

        public void Connect(NetworkEndpoint endpoint, NetworkConnectionHandler.ActionRef<DataStreamReader> onDataReceived = null)
        {
            var localConnection = endpoint.IsLoopback;
            Debug.Log(_handlers.Count);
            var localServer = false;
            
            for (int i = 0; i < _handlers.Count; i++)
            {
                var handler = _handlers[i];

                if (handler.Driver.GetLocalEndpoint().Port != endpoint.Port)
                {
                    continue;
                }
                if (localConnection)
                {
                    Debug.Log("Local connection");
                    if (handler.Internal)
                    {
                        Debug.Log("Local connection already established");
                        //we already have a connection 
                        return;
                    }

                    if (localServer)
                    {
                        continue;
                    }

                    
                    if (handler.Driver.GetLocalEndpoint().IsLoopback || handler.Driver.GetLocalEndpoint() ==
                        NetworkEndpoint.AnyIpv4.WithPort(endpoint.Port))
                    {
                        Debug.Log("Local server already exists");
                        //We already have a server, but need to check if we have a connection
                        localServer = true;
                    }
                }
                
                
                for (int connectionIndex = 0; connectionIndex < handler.Connections.Length; connectionIndex++)
                {
                    if (handler.Driver.GetRemoteEndpoint(handler.Connections[connectionIndex]).Equals(endpoint))
                    {
                        //we are already connected 
                        Debug.Log("Already connected");
                        if (onDataReceived != null && !handler.OnDataReceived.GetInvocationList().Contains(onDataReceived))
                        {
                            handler.OnDataReceived += onDataReceived;
                        }
                        return;
                    }
                    
                }
            }
            
            Debug.Log($"Creating client for connection as local? {localServer}");
            _handlers.Add(CreateClient(endpoint, onDataReceived, localServer));


        }

        private NetworkConnectionHandler CreateClient(NetworkEndpoint endpoint,
            NetworkConnectionHandler.ActionRef<DataStreamReader> onDataReceived, bool localClient)
        {
            if (localClient)
            {
                var client =  new NetworkConnectionHandler().AsLocalClient(endpoint);
                client.OnConnectionAccepted += OnConnectionEstablished;
                return client;
            }
            NetworkConnectionHandler.ActionRef<DataStreamReader> combinedAction = OnDataReceived;
            if (onDataReceived != null)
            {
                combinedAction += onDataReceived;
            }

            var newClient = new NetworkConnectionHandler().AsClient(endpoint);
            newClient.OnDisconnect += OnDisconnect;
            newClient.OnConnectionAccepted += OnConnectionEstablished;
            newClient.OnDataReceived += combinedAction;
            return newClient;
        }
        
        public void RegisterCallback<T>(Action<T> callback) where T : ISerializer, new() =>
            _eventHandler.RegisterCallback(callback);

        public void UnregisterCallback<T>(Action<T> callback) where T : ISerializer, new() =>
            _eventHandler.UnregisterCallback(callback);

        public bool TransmitNetworkMessage(ISerializer data)
        {
            Debug.Log($"Connection count: {_handlers.Count}");
            var successful = false;
            foreach (var handler in _handlers)
            {
                foreach (var connection in handler.Connections)
                {
                    if (!connection.IsCreated)
                    {
                        continue;
                    }

                    Debug.Log($"Sending message to {handler.Driver.GetRemoteEndpoint(connection)}");
                    successful = true;
                    handler.Driver.BeginSend(handler.Pipeline, connection, out var writer);
                    writer.SendMessage(data);
                    handler.Driver.EndSend(writer);
                }
            }

            return successful;
        }
        
        public bool TransmitNetworkMessage(ISerializer data, NetworkEndpoint endpoint)
        {
            var successful = false;
            foreach (var handler in _handlers)
            {
                foreach (var connection in handler.Connections)
                {
                    
                    if (!connection.IsCreated)
                    {
                        continue;
                    }
                    if (handler.Driver.GetRemoteEndpoint(connection).Equals(endpoint))
                    {
                        Debug.Log("Sending message");
                        successful = true;
                        handler.Driver.BeginSend(handler.Pipeline, connection, out var writer);
                        writer.SendMessage(data);
                        handler.Driver.EndSend(writer);
                    }
                }
            }

            return successful;
        }
        


        private void Update()
        {
            for (var index = _handlers.Count - 1; index >= 0; index--)
            {
                var handler = _handlers[index];
                handler.UpdateConnectionHandler();
                //todo remove empty handlers (but check that they disconnected and aren't waiting for a connection
            }
        }
        
        private void OnDataReceived(ref DataStreamReader stream)
        {
            var typeIndex = stream.ReadByte();
            var type = DataSerializationIndexer.GetTypeForTypeIndex(typeIndex);

            Debug.Log($"Message of type {type} received");
            if (!_eventHandler.TriggerCallback(type, ref stream))
            {
                Debug.LogError($"Type {type} is not handled yet by {nameof(NetworkMessageEventHandler)}. This either means that {type} does not implement {nameof(ISerializer)} or that the type does not have a default constructor");
            }
        }


        private void OnIpDataReceived(ref DataStreamReader stream)
        {
            var typeIndex = stream.ReadByte();
            var type = DataSerializationIndexer.GetTypeForTypeIndex(typeIndex);
            if (type == typeof(IpAddressData))
            {
                var ipAddressData = new IpAddressData();
                ipAddressData.Deserialize(ref stream);
                Debug.Log($"Trying to connect to {ipAddressData.Endpoint.ToString()}", this);
                OnConnectionRequested.Invoke();

#if UNITY_EDITOR
                Connect(NetworkEndpoint.LoopbackIpv4.WithPort(ipAddressData.Endpoint.Port));
#else
                Connect(ipAddressData.Endpoint);
#endif
                        
            }
        }



       
        
    }
}