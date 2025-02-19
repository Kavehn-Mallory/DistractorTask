using System;
using DistractorTask.Core;
using Unity.Networking.Transport;
using UnityEngine;

namespace DistractorTask.Transport
{
    public class NetworkConnectionManager : Singleton<NetworkConnectionManager>, INetworkManager
    {
        
        [SerializeField]
        private ConnectionDataSettings settings = new()
        {
            endpointSource = NetworkEndpointSetting.LoopbackIPv4,
            port = new ConnectionPortProperty(7777)
        };
        
        public NetworkEndpoint NetworkEndpoint { get; }

        private NetworkConnectionHandler _ipRequestHandler;

        private NetworkConnectionHandler _dataConnectionHandler;
        
        public void RegisterCallback<T>(Action<T> callback) where T : ISerializer, new()
        {
            throw new NotImplementedException();
        }

        public void UnregisterCallback<T>(Action<T> callback) where T : ISerializer, new()
        {
            throw new NotImplementedException();
        }

        public bool TransmitNetworkMessage(ISerializer data)
        {
            throw new NotImplementedException();
        }

        public event Action<bool> OnConnectionEstablished;
        
    }
}