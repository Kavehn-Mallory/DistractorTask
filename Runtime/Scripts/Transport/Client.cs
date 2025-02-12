using System;
using DistractorTask.Core;
using DistractorTask.Transport.DataContainer;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;
using UnityEngine;

namespace DistractorTask.Transport
{
    public class Client : Singleton<Client>, INetworkManager
    {
        [SerializeField]
        private ConnectionDataSettings settings = new()
        {
            endpointSource = NetworkEndpointSetting.LoopbackIPv4,
            port = new ConnectionPortProperty(7777)
        };

        public Action OnConnectionRequested = delegate { };
        public Action OnConnectionDisconnected = delegate { };
        public event Action<bool> OnConnectionEstablished = delegate { };

        public NetworkEndpoint NetworkEndpoint => settings.NetworkEndpoint;

        private NetworkDriver _driver;
        private NetworkConnection _connection;
        private NetworkPipeline _pipeline;

        private NetworkMessageEventHandler _eventHandler;

        private NetworkConnectionHandler _ipRequestHandler;

        public void RegisterCallback<T>(Action<T> callback) where T : ISerializer, new() =>
            _eventHandler.RegisterCallback(callback);

        public void UnregisterCallback<T>(Action<T> callback) where T : ISerializer, new() =>
            _eventHandler.UnregisterCallback(callback);

        protected override void Awake()
        {
            base.Awake();
            _eventHandler = new NetworkMessageEventHandler();
            ListenForIpRequest(7500);

        }

        public void ListenForIpRequest(ushort port)
        {
            var ip = NetworkConnectionHandler.GetLocalIPAddress();
            var endpoint = NetworkEndpoint.Parse(ip.ToString(), port);
#if UNITY_EDITOR
            endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(port);
#endif

            _ipRequestHandler = new NetworkConnectionHandler().AsServer(endpoint);
            Debug.Log($"Opened channel: {endpoint.ToString()}");
            _ipRequestHandler.OnUpdate += OnIpRequestHandlerUpdate;
        }

        private void OnIpRequestHandlerUpdate(NetworkConnectionHandler handler)
        {
            NetworkConnection connection;
            while ((connection = handler.Driver.Accept()) != default)
            {
                handler.Connection = connection;
                Debug.Log("Accepted an IP-request.");
            }

            NetworkEvent.Type cmd;
            while ((cmd = handler.Driver.PopEventForConnection(handler.Connection, out var stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    var typeIndex = stream.ReadByte();
                    var type = DataSerializationIndexer.GetTypeForTypeIndex(typeIndex);
                    if (type == typeof(IpAddressData))
                    {
                        var ipAddressData = new IpAddressData();
                        ipAddressData.Deserialize(ref stream);
                        Debug.Log($"Trying to connect to {ipAddressData.Endpoint.ToString()}");
                        OnConnectionRequested.Invoke();

#if UNITY_EDITOR
                        Connect(NetworkEndpoint.LoopbackIpv4.WithPort(ipAddressData.Endpoint.Port));
#else
                        Connect(ipAddressData.Endpoint);
#endif
                        
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("IP request was ended.");
                    handler.Dispose();
                    break;
                }
            }
        }

        private void OnDestroy()
        {
            _driver.Dispose();
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

            if (!_connection.IsCreated)
            {
                return;
            }

            NetworkEvent.Type cmd;
            while ((cmd = _connection.PopEvent(_driver, out var stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    Debug.Log("We are now connected to the server.");
                    OnConnectionEstablished?.Invoke(true);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    ProcessData(ref stream);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    var reason = (DisconnectReason) stream.ReadByte();
                    if (reason != DisconnectReason.ClosedByRemote && reason != DisconnectReason.Timeout)
                    {
                        //We neither closed the connection manually nor timed out -> error during connection 
                        OnConnectionEstablished.Invoke(false);
                    }
                    else
                    {
                        OnConnectionDisconnected.Invoke();
                    }
                    Debug.Log($"Client got disconnected from server due to {reason}.");
                    _connection = default;
                }
            }
        }
        

        public bool TransmitNetworkMessage(ISerializer data)
        {
            if (!_connection.IsCreated)
            {
                return false;
            }

            _driver.BeginSend(_pipeline, _connection, out var writer);
            writer.SendMessage(data);
            _driver.EndSend(writer);
            return true;
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
        

        public void Connect(NetworkEndpoint endpoint)
        {
            if (_connection.IsCreated)
            {
                return;
            }
            _driver = NetworkDriver.Create();
            _pipeline = PipelineCreation.CreatePipeline(ref _driver);
            _connection = _driver.Connect(endpoint);
            
        }
    }

}