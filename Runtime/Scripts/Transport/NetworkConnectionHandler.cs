using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;
using UnityEngine;

namespace DistractorTask.Transport
{
    public class NetworkConnectionHandler : IDisposable
    {
        
        //Todo change this even further -> have a thing that has a driver, pipeline and connection(s) package 
        //have list with listeners?
        public delegate void ActionRef<T>(ref T item);
        public delegate void ActionRef<T, TS>(ref T item, TS item2);
        
        public NetworkDriver Driver;
        public NetworkPipeline Pipeline;
        public NativeList<NetworkConnection> Connections;
        
        private event Action<ushort, ConnectionState> OnConnectionStateChanged;
        private readonly ActionRef<DataStreamReader, ushort> _onDataReceived;
        public bool Internal;
        private NativeList<ushort> _endpointPorts;
        [Obsolete]
        public ConnectionType ConnectionType;
        public ConnectionState ConnectionState;

        public void Dispose()
        {
            if(Driver.IsCreated)
                Driver.Dispose();
            if (Connections.IsCreated)
                Connections.Dispose();
            if (_endpointPorts.IsCreated)
            {
                _endpointPorts.Dispose();
            }

            Internal = false;
        }

        public bool IsCreated => Driver.IsCreated || Connections.IsCreated;
        

        
        public NetworkConnectionHandler(ActionRef<DataStreamReader, ushort> onDataReceived, Action<ushort, ConnectionState> onConnectionStateChanged, int numberOfConnections = 1)
        {
            if (IsCreated)
            {
                Dispose();
            }
            Driver = NetworkDriver.Create();
            Pipeline = PipelineCreation.CreatePipeline(ref Driver);
            Connections = new NativeList<NetworkConnection>(numberOfConnections, Allocator.Persistent);
            OnConnectionStateChanged = onConnectionStateChanged;
            this._onDataReceived = onDataReceived;
            _endpointPorts = new NativeList<ushort>(numberOfConnections, Allocator.Persistent);

        }




        public void UpdateConnectionHandler()
        {
            if (IsCreated)
            {
                Driver.ScheduleUpdate().Complete();
                
                // Clean up connections.
                for (int i = 0; i < Connections.Length; i++)
                {
                    if (!Connections[i].IsCreated)
                    {
                        Connections.RemoveAtSwapBack(i);
                        i--;
                    }
                }

                if (Driver.Listening)
                {
                    // Accept new connections.
                    NetworkConnection c;
                    while ((c = Driver.Accept()) != default)
                    {
                        Connections.Add(c);
                        Debug.Log($"Accepted connection for {Driver.GetLocalEndpoint()}");
                        ChangeConnectionState(ConnectionState.Connected);
                    }
                }
                

                
                //handle active connections
                for (int i = 0; i < Connections.Length; i++)
                {
                    NetworkEvent.Type cmd;
                    while ((cmd = Driver.PopEventForConnection(Connections[i], out var stream)) !=
                           NetworkEvent.Type.Empty)
                    {
                        if (cmd == NetworkEvent.Type.Connect)
                        {
                            Debug.Log("We are now connected to the server.");
                            ChangeConnectionState(ConnectionState.Connected);
                        }
                        if (cmd == NetworkEvent.Type.Data)
                        {
                            Debug.Log($"Data received {Driver.GetLocalEndpoint()}");
                            _onDataReceived.Invoke(ref stream, Driver.GetLocalEndpoint().Port);
                        }
                        else if (cmd == NetworkEvent.Type.Disconnect)
                        {
                            Connections[i] = default;
                            var reason = (DisconnectReason) stream.ReadByte();
                            ChangeConnectionState(NetworkExtensions.CastDisconnectReasonToConnectionState(reason));
                            Debug.Log($"We got disconnected from server due to {reason}.");
                            break;
                        }
                    }
                }
            }
        }

        private void ChangeConnectionState(ConnectionState state)
        {
            if (state == ConnectionState)
            {
                return;
            }
            ConnectionState = state;
            foreach (var endpoint in _endpointPorts)
            {
                OnConnectionStateChanged?.Invoke(endpoint, state);
            }
            
            
            
        }
        
        
        public void Connect(NetworkEndpoint endpoint, ConnectionType connectionType)
        {
            ConnectionType = connectionType;
            ChangeConnectionState(ConnectionState.ConnectionRequested);
            if (!_endpointPorts.Contains(endpoint.Port))
            {
                _endpointPorts.Add(endpoint.Port);
            }
            Connections.Add(Driver.Connect(endpoint));
        }

        public bool StartListening(NetworkEndpoint endpoint, ConnectionType connectionType)
        {
            ConnectionType = connectionType;
            if (Driver.Listening)
            {
                return true;
            }
            if (Driver.Bind(endpoint) != 0)
            {
                return false;
            }

            if (!_endpointPorts.Contains(endpoint.Port))
            {
                _endpointPorts.Add(endpoint.Port);
            }
            
            Driver.Listen();
            return true;
        }


        public void EstablishInternalConnection()
        {
            Internal = true;
            if (ConnectionState != ConnectionState.Connected)
            {
                ChangeConnectionState(ConnectionState.Connected);
            }
        }

        public bool ContainsEndpoint(ushort endpointPort)
        {
            foreach (var networkEndpoint in _endpointPorts)
            {
                if (networkEndpoint.Equals(endpointPort))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsConnectedTo(NetworkEndpoint endpoint)
        {
            foreach (var connection in Connections)
            {
                if (Driver.GetRemoteEndpoint(connection).Equals(endpoint))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public enum ConnectionType
    {
        Broadcast,
        Multicast
    }
}

