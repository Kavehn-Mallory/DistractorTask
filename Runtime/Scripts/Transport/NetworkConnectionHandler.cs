using System;
using System.Net;
using System.Net.Sockets;
using Unity.Collections;
using Unity.Networking.Transport;
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
        
        public event Action<bool> OnConnectionAccepted = delegate { };
        public ActionRef<DataStreamReader> OnDataReceived = delegate { };
        public ActionRef<DataStreamReader, NetworkConnectionHandler> OnDisconnect = delegate { };
        public bool IsLocal;

        public void Dispose()
        {
            if(Driver.IsCreated)
                Driver.Dispose();
            if (Connections.IsCreated)
                Connections.Dispose();

            IsLocal = false;
        }

        public bool IsCreated => Driver.IsCreated || Connections.IsCreated;

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
                        OnConnectionAccepted.Invoke(true);
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
                            OnConnectionAccepted?.Invoke(true);
                        }
                        if (cmd == NetworkEvent.Type.Data)
                        {
                            OnDataReceived.Invoke(ref stream);
                        }
                        else if (cmd == NetworkEvent.Type.Disconnect)
                        {
                            Connections[i] = default;
                            OnDisconnect.Invoke(ref stream, this);
                            break;
                        }
                    }
                }
            }
        }
        

        public NetworkConnectionHandler AsServer(NetworkEndpoint endpoint)
        {
            if (IsCreated)
            {
                Dispose();
            }
            Driver = NetworkDriver.Create();
            Pipeline = PipelineCreation.CreatePipeline(ref Driver);
            Connections = new NativeList<NetworkConnection>(1, Allocator.Persistent);
            
            if (Driver.Bind(endpoint) != 0)
            {
                Debug.LogError($"Failed to bind to port {endpoint.Port}.");
                return this;
            }
            Driver.Listen();
            return this;
        }

        public NetworkConnectionHandler AsClient(NetworkEndpoint endpoint)
        {
            return AsClient(endpoint, false);
        }

        private NetworkConnectionHandler AsClient(NetworkEndpoint endpoint, bool local)
        {
            if (IsCreated)
            {
                Dispose();
            }
            Driver = NetworkDriver.Create();
            Pipeline = PipelineCreation.CreatePipeline(ref Driver);
            Connections = new NativeList<NetworkConnection>(1, Allocator.Persistent);
            Connections.Add(Driver.Connect(endpoint));
            IsLocal = local;
            return this;
        }

        public NetworkConnectionHandler AsLocalClient(NetworkEndpoint endpoint)
        {
            return AsClient(endpoint, true);
        }
        
        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            return null;
        }
    }
}

