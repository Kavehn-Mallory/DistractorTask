using System;
using System.Net;
using System.Net.Sockets;
using Unity.Networking.Transport;
using UnityEngine;

namespace DistractorTask.Transport
{
    public class NetworkConnectionHandler : IDisposable
    {
        public NetworkDriver Driver;
        public NetworkPipeline Pipeline;
        public NetworkConnection Connection;

        public event Action<NetworkConnectionHandler> OnUpdate = delegate { };

        public void Dispose()
        {
            Driver.Dispose();
            Connection = default;
        }

        public bool IsCreated => Driver.IsCreated;

        public void UpdateConnectionHandler()
        {
            if (IsCreated)
            {
                Driver.ScheduleUpdate().Complete();
                OnUpdate.Invoke(this);
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
            if (IsCreated)
            {
                Dispose();
            }
            Driver = NetworkDriver.Create();
            Pipeline = PipelineCreation.CreatePipeline(ref Driver);
            Connection = Driver.Connect(endpoint);
            return this;
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