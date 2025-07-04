using System;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;
using UnityEngine;

namespace DistractorTask.Transport
{
    public class NetworkConnectionHandler : IDisposable
    {
        
        //have list with listeners?
        public delegate void ActionRef<T>(ref T item);
        public delegate void ActionRef<T, TS>(ref T item, TS item2);
        
        public NetworkDriver Driver;
        public NetworkPipeline Pipeline;
        public NativeList<NetworkConnection> Connections;
        
        private event Action<ushort, ConnectionState> OnConnectionStateChanged;
        private readonly ActionRef<DataStreamReader, ushort> _onDataReceived;
        private NativeList<ushort> _endpointPorts;
        public ConnectionState ConnectionState;
        private ushort _assignedPort;

        public ushort Port => _assignedPort;

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
        }

        public bool IsCreated => Driver.IsCreated || Connections.IsCreated;
        

        
        public NetworkConnectionHandler(ushort assignedPort, ActionRef<DataStreamReader, ushort> onDataReceived, Action<ushort, ConnectionState> onConnectionStateChanged, int numberOfConnections = 1)
        {
            if (IsCreated)
            {
                Dispose();
            }

            var networkSettings = new NetworkSettings();
            //this enables heartbeat messages -> sent when nothing is received from a peer for some time -> has to be smaller than the reconnection timeout
            networkSettings.WithNetworkConfigParameters(1000, 60, 30000, 10000, 20000);
            Driver = NetworkDriver.Create(networkSettings);
            Pipeline = PipelineCreation.CreatePipeline(ref Driver);
            Connections = new NativeList<NetworkConnection>(numberOfConnections, Allocator.Persistent);
            OnConnectionStateChanged = onConnectionStateChanged;
            this._onDataReceived = onDataReceived;
            _endpointPorts = new NativeList<ushort>(numberOfConnections, Allocator.Persistent);
            _assignedPort = assignedPort;
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
                            _onDataReceived.Invoke(ref stream, _assignedPort);
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
        
        
        public void Connect(NetworkEndpoint endpoint)
        {
            ChangeConnectionState(ConnectionState.ConnectionRequested);
            if (!_endpointPorts.Contains(endpoint.Port))
            {
                _endpointPorts.Add(endpoint.Port);
            }
            Connections.Add(Driver.Connect(endpoint));
        }

        public bool StartListening(NetworkEndpoint endpoint)
        {
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
        Multicast,
        Unicast
    }
}

