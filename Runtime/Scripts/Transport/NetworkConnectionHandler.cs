using System;
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
        
        private event Action<NetworkEndpoint, ConnectionState> OnConnectionStateChanged;
        private readonly ActionRef<DataStreamReader> _onDataReceived;
        public bool Internal;
        public NetworkEndpoint Endpoint => Driver.GetLocalEndpoint();
        public ConnectionType ConnectionType;
        public ConnectionState ConnectionState;

        public void Dispose()
        {
            if(Driver.IsCreated)
                Driver.Dispose();
            if (Connections.IsCreated)
                Connections.Dispose();

            Internal = false;
        }

        public bool IsCreated => Driver.IsCreated || Connections.IsCreated;
        

        
        public NetworkConnectionHandler(ActionRef<DataStreamReader> onDataReceived, Action<NetworkEndpoint, ConnectionState> onConnectionStateChanged, int numberOfConnections = 1)
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
                            Debug.Log("Data received");
                            _onDataReceived.Invoke(ref stream);
                        }
                        else if (cmd == NetworkEvent.Type.Disconnect)
                        {
                            Connections[i] = default;
                            var reason = (DisconnectReason) stream.ReadByte();
                            ChangeConnectionState(NetworkHelper.CastDisconnectReasonToConnectionState(reason));
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
            OnConnectionStateChanged?.Invoke(Endpoint, state);
        }
        
        
        public void Connect(NetworkEndpoint endpoint, ConnectionType connectionType)
        {
            ConnectionType = connectionType;
            ChangeConnectionState(ConnectionState.ConnectionRequested);
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
    }

    public enum ConnectionType
    {
        Broadcast,
        Multicast
    }
}

