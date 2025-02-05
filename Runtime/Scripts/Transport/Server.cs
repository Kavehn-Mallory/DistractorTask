using System;
using DistractorTask.Core;
using DistractorTask.Logging;
using DistractorTask.Transport.DataContainer;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Serialization;

namespace DistractorTask.Transport
{
    public class Server : Singleton<Server>, INetworkManager
    {
        private NetworkDriver _driver;
        private NativeList<NetworkConnection> _connections;

        public ConnectionDataSettings settings = new()
        {
            port = new ConnectionPortProperty(7777),
            endpointSource = NetworkEndpointSetting.AnyIPv4
        };
        
        public NetworkEndpoint NetworkEndpoint => settings.NetworkEndpoint;
        
        private NetworkMessageEventHandler _eventHandler;
        private NetworkPipeline _pipeline;

        private NetworkConnectionHandler _ipRequestHandler;

        private LogSystem _logSystem;


        [FormerlySerializedAs("ipRequestSettings")] [SerializeField]
        private ConnectionDataSettings ipTransmissionSettings = new()
        {
            port = new ConnectionPortProperty(7500),
            endpointSource = NetworkEndpointSetting.Custom
        };


        protected override void Awake()
        {
            base.Awake();
            Debug.Log("Init");
            _eventHandler = new NetworkMessageEventHandler();
            _connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
            
        }

        private void Start()
        {
            _driver = NetworkDriver.Create();
            _pipeline = PipelineCreation.CreatePipeline(ref _driver);
            

            var endpoint = settings.NetworkEndpoint;
            if (_driver.Bind(endpoint) != 0)
            {
                Debug.LogError($"Failed to bind to port {settings.port}.");
                return;
            }
            _driver.Listen();
        }
        
        public void RegisterCallback<T>(Action<T> callback) where T : ISerializer, new() =>
            _eventHandler.RegisterCallback(callback);

        public void UnregisterCallback<T>(Action<T> callback) where T : ISerializer, new() =>
            _eventHandler.UnregisterCallback(callback);

        private void OnDestroy()
        {
            if (_driver.IsCreated)
            {
                _driver.Dispose();
                _connections.Dispose();
            }

            if (_ipRequestHandler is { IsCreated: true })
            {
                _ipRequestHandler.Dispose();
            }
        }

        private void Update()
        {
            _ipRequestHandler?.UpdateConnectionHandler();


            if (!_driver.IsCreated)
            {
                return;
            }
            
            _driver.ScheduleUpdate().Complete();

            // Clean up connections.
            for (int i = 0; i < _connections.Length; i++)
            {
                if (!_connections[i].IsCreated)
                {
                    _connections.RemoveAtSwapBack(i);
                    i--;
                }
            }

            // Accept new connections.
            NetworkConnection c;
            while ((c = _driver.Accept()) != default)
            {
                _connections.Add(c);
                Debug.Log("Accepted a connection.");
            }

            for (int i = 0; i < _connections.Length; i++)
            {
                NetworkEvent.Type cmd;
                while ((cmd = _driver.PopEventForConnection(_connections[i], out var stream)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        ProcessData(ref stream);
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("Client disconnected from the server.");
                        _connections[i] = default;
                        break;
                    }
                }
            }
        }
        

        private static void UpdateIpRequestBundle(NetworkConnectionHandler handler, NetworkEndpoint endpoint)
        {
            
            NetworkEvent.Type cmd;
            while ((cmd = handler.Connection.PopEvent(handler.Driver, out _)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    handler.Driver.BeginSend(handler.Pipeline, handler.Connection, out var writer);
                    writer.SendMessage(new IpAddressData
                    {
                        Endpoint = endpoint
                    });
                    handler.Driver.EndSend(writer);
                    handler.Driver.Disconnect(handler.Connection);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    handler.Connection = default;
                    handler.Dispose();
                }
            }
        }

        public bool TransmitNetworkMessage(ISerializer data)
        {
            bool success = true;
            foreach (var connection in _connections)
            {
                if (!connection.IsCreated)
                {
                    success = false;
                    continue;
                }
                _driver.BeginSend(_pipeline, connection, out var writer);
                writer.SendMessage(data);
                _driver.EndSend(writer);
            }
            return success;
        }
        
        private void ProcessData(ref DataStreamReader stream)
        {
            var typeIndex = stream.ReadByte();
            var type = DataSerializationIndexer.GetTypeForTypeIndex(typeIndex);

            if (!_eventHandler.TriggerCallback(type, ref stream))
            {
                Debug.LogError($"Type {type} is not handled yet by {nameof(NetworkMessageEventHandler)}. This either means that {type} does not implement {nameof(ISerializer)} or that the type does not have a default constructor");
            }
        }
        
        public event Action OnAutoConnectionEstablished;

        public void TransmitIpAddress(NetworkEndpoint endpoint)
        {
            if (_ipRequestHandler is { IsCreated: true })
            {
                return;
            }
            _ipRequestHandler = new NetworkConnectionHandler();
            _ipRequestHandler.AsClient(endpoint);

            _ipRequestHandler.OnUpdate += (handler) => UpdateIpRequestBundle(handler, settings.NetworkEndpoint);

        }

        [ContextMenu("Transmit IP-Address")]
        public void TransmitIpAddress()
        {
            TransmitIpAddress(ipTransmissionSettings.NetworkEndpoint);
        }
        
    }
}