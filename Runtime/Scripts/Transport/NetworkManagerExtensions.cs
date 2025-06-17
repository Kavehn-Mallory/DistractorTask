using System;
using System.Threading.Tasks;
using DistractorTask.Core;
using Unity.Networking.Transport;
using UnityEngine;

namespace DistractorTask.Transport
{
    public static class NetworkManagerExtensions
    {

        public static async Task BroadcastMessageAndAwaitResponse<T, TResponse>(this INetworkManager networkManager, T data, int callerId, int messageId, bool suppressLocalBroadcast = false) where T : IRespondingSerializer<TResponse>, new() where TResponse : IResponseIdentifier, ISerializer, new()
        {
            var task = networkManager.ScheduleSendAndReceive<TResponse, T>(data, NetworkEndpoint.AnyIpv4.WithPort(0), callerId, messageId, ConnectionType.Broadcast, suppressLocalBroadcast);
            await task.Task;
        }

        public static async Task MulticastMessageAndAwaitResponse<T, TResponse>(this INetworkManager networkManager, T data, ushort targetPort, int callerId, int messageId)
            where T : IRespondingSerializer<TResponse>, new() where TResponse : IResponseIdentifier, ISerializer, new()
        {
            var task = networkManager.ScheduleSendAndReceive<TResponse, T>(data, NetworkEndpoint.AnyIpv4.WithPort(targetPort),
                callerId, messageId, ConnectionType.Multicast);
            await task.Task;
        }
        
        public static async Task UnicastMessageAndAwaitResponse<T, TResponse>(this INetworkManager networkManager, T data, NetworkEndpoint endpoint, int callerId, int messageId)
            where T : IRespondingSerializer<TResponse>, new() where TResponse : IResponseIdentifier, ISerializer, new()
        {
            var task = networkManager.ScheduleSendAndReceive<TResponse, T>(data, endpoint,
                callerId, messageId, ConnectionType.Unicast);
            await task.Task;
        }
        
        private static TaskCompletionSource<T> ScheduleSendAndReceive<T, TS>(this INetworkManager networkManager, TS data, NetworkEndpoint endpoint, int callerId,
            int messageId, ConnectionType connectionType, bool suppressLocalBroadcast = false) where T : ISerializer, IResponseIdentifier, new() where TS : IRespondingSerializer<T>, new()
        {
            var returnValue = new TaskCompletionSource<T>();

            var testStruct = networkManager.CreateCallback(returnValue, new T
            {
                MessageId = messageId,
                SenderId = callerId
            });


            data.MessageId = messageId;
            data.SenderId = callerId;
            
            
            switch (connectionType)
            {
                case ConnectionType.Broadcast:
                    networkManager.RegisterCallbackAllPorts<T>(testStruct.Callback);
                    networkManager.BroadcastMessage(data, callerId, suppressLocalBroadcast);
                    break;
                case ConnectionType.Multicast:
                    networkManager.RegisterCallback<T>(testStruct.Callback, endpoint.Port);
                    networkManager.MulticastMessage(data, endpoint.Port, callerId);
                    break;
                case ConnectionType.Unicast:
                    networkManager.RegisterCallback<T>(testStruct.Callback, endpoint.Port);
                    networkManager.UnicastMessage(data, endpoint, callerId);
                    break;
                default:
                    networkManager.RegisterCallback<T>(testStruct.Callback, endpoint.Port);
                    networkManager.MulticastMessage(data, endpoint.Port, callerId);
                    break;
            }
            
            return testStruct.Source;
        }

        /// <summary>
        /// Expects a broadcast message of type <see cref="T"/>, triggering the given action and afterward responds with a message of type <see cref="TResponse"/>.
        /// </summary>
        /// <param name="networkManager">Network Manager to use</param>
        /// <param name="actionToPerformBeforeResponse">Action to perform before sending out the response</param>
        /// <param name="callerId">Caller id of the responder</param>
        /// <param name="suppressLocalBroadcast">Determines whether a broadcast message should be propagated locally.</param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns>Awaitable task </returns>
        public static Task AwaitBroadcastMessageAndRespond<T, TResponse>(this INetworkManager networkManager, Action<T, int> actionToPerformBeforeResponse, int callerId, bool suppressLocalBroadcast = false) where T : IRespondingSerializer<TResponse>, new() where TResponse : IResponseIdentifier, ISerializer, new()
        {
            return networkManager.AwaitMessageAndRespond<T, TResponse>(actionToPerformBeforeResponse, ConnectionType.Broadcast, 0, ConnectionType.Broadcast, NetworkEndpoint.AnyIpv4.WithPort(0), callerId, suppressLocalBroadcast);
        }

        /// <summary>
        /// Expects a multicast message of type <see cref="T"/>, triggering the given action and afterward responds with a message of type <see cref="TResponse"/>.
        /// </summary>
        /// <param name="networkManager">Network Manager to use</param>
        /// <param name="actionToPerformBeforeResponse">Action to perform before sending out the response</param>
        /// <param name="targetPort">Port where arrival of message <see cref="T"/> is awaited</param>
        /// <param name="callerId">Caller id of the responder</param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        public static Task AwaitMulticastMessageAndRespond<T, TResponse>(this INetworkManager networkManager, Action<T, int> actionToPerformBeforeResponse, ushort targetPort, int callerId) where T : IRespondingSerializer<TResponse>, new() where TResponse : IResponseIdentifier, ISerializer, new()
        {
            return networkManager.AwaitMessageAndRespond<T, TResponse>(actionToPerformBeforeResponse, ConnectionType.Multicast, targetPort, ConnectionType.Multicast, NetworkEndpoint.AnyIpv4.WithPort(targetPort), callerId);
        }
        

        /// <summary>
        /// Expects a unicast message of type <see cref="T"/>, triggering the given action and afterward responds with a message of type <see cref="TResponse"/>.
        /// </summary>
        /// <param name="networkManager">Network Manager to use</param>
        /// <param name="actionToPerformBeforeResponse">Action to perform before sending out the response</param>
        /// <param name="targetEndpoint">Endpoint defines port where arrival of message <see cref="T"/> is awaited. Endpoint is also used to respond.</param>
        /// <param name="callerId">Caller id of the responder</param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        public static Task AwaitUnicastMessageAndRespond<T, TResponse>(this INetworkManager networkManager, Action<T, int> actionToPerformBeforeResponse, NetworkEndpoint targetEndpoint, int callerId) where T : IRespondingSerializer<TResponse>, new() where TResponse : IResponseIdentifier, ISerializer, new()
        {
            return networkManager.AwaitMessageAndRespond<T, TResponse>(actionToPerformBeforeResponse, ConnectionType.Multicast, targetEndpoint.Port, ConnectionType.Unicast, targetEndpoint, callerId);
        }


        internal static Task AwaitMessageAndRespond<T, TResponse>(this INetworkManager networkManager,
            Action<T, int> actionToPerformBeforeResponse, ConnectionType receivingMessageType, ushort receivingPort,
            ConnectionType responseMessageType, NetworkEndpoint responseEndpoint, int callerId,
            bool suppressLocalBroadcast = false, bool persistent = false)
            where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            var responseMessage =
                networkManager.GetResponseMethod<T, TResponse>(responseMessageType, new TResponse(), responseEndpoint, callerId, suppressLocalBroadcast);

            //Todo this is not restartable yet? 

            var awaitableResponse = new AwaitMessageWrapper<T>(networkManager, actionToPerformBeforeResponse,
                responseMessage, receivingMessageType, receivingPort, persistent);
            return awaitableResponse.AwaitMessage();
        }

        
        public static Action RegisterResponse<T, TResponse>(this INetworkManager networkManager, Action<T, int> actionToPerformBeforeResponse, ConnectionType receivingMessageType, ushort receivingPort,
            ConnectionType responseMessageType, NetworkEndpoint responseEndpoint, int callerId,
            bool suppressLocalBroadcast = false, bool persistent = false) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            
            var responseMessage =
                networkManager.GetResponseMethod<T, TResponse>(responseMessageType, new TResponse(), responseEndpoint, callerId, suppressLocalBroadcast);


            actionToPerformBeforeResponse ??= delegate { };

            actionToPerformBeforeResponse += responseMessage;
            var oneTimeUse = networkManager.CreateOneTimeActionWrapper(receivingMessageType, actionToPerformBeforeResponse, responseEndpoint.Port, persistent);
            

            RegisterCallback<T>(networkManager, receivingMessageType, receivingPort, oneTimeUse.TriggerEvent);

            return oneTimeUse.Unregister;

        }

        private static void RegisterCallback<T>(INetworkManager networkManager, ConnectionType receivingMessageType,
            ushort receivingPort, Action<T, int> callback) where T : ISerializer, new()
        {
            switch (receivingMessageType)
            {
                case ConnectionType.Broadcast:
                    networkManager.RegisterCallbackAllPorts(callback);
                    
                    break;
                case ConnectionType.Multicast:
                    networkManager.RegisterCallback(callback, receivingPort);
                    break;
                case ConnectionType.Unicast:
                    networkManager.RegisterCallback(callback, receivingPort);
                    break;
                default:
                    networkManager.RegisterCallback(callback, receivingPort);
                    break;
            }
        }
        
        private static void UnregisterCallback<T>(INetworkManager networkManager, ConnectionType receivingMessageType,
            ushort receivingPort, Action<T, int> callback) where T : ISerializer, new()
        {
            switch (receivingMessageType)
            {
                case ConnectionType.Broadcast:
                    networkManager.UnregisterCallbackAllPorts(callback);
                    
                    break;
                case ConnectionType.Multicast:
                    networkManager.UnregisterCallback(callback, receivingPort);
                    break;
                case ConnectionType.Unicast:
                    networkManager.UnregisterCallback(callback, receivingPort);
                    break;
                default:
                    networkManager.UnregisterCallback(callback, receivingPort);
                    break;
            }
        }

        /*public static Action RegisterResponse<T, TResponse>(Func<(T, int), TResponse> actionToPerformBeforeResponse, ConnectionType receivingMessageType, ushort receivingPort,
            ConnectionType responseMessageType, NetworkEndpoint responseEndpoint, int callerId,
            bool suppressLocalBroadcast = false, bool persistent = false) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            
            var responseMessage =
                GetResponseMethod<T, TResponse>(responseMessageType, new TResponse(), responseEndpoint, callerId, suppressLocalBroadcast);


            actionToPerformBeforeResponse ??= delegate { };

            actionToPerformBeforeResponse += responseMessage;
            var oneTimeUse = new OneTimeActionWrapper<T>(actionToPerformBeforeResponse, responseEndpoint.Port, persistent);
            

            switch (receivingMessageType)
            {
                case ConnectionType.Broadcast:
                    networkManager.RegisterCallbackAllPorts<T>(oneTimeUse.TriggerEvent);
                    
                    break;
                case ConnectionType.Multicast:
                    networkManager.RegisterCallback<T>(oneTimeUse.TriggerEvent, receivingPort);
                    break;
                case ConnectionType.Unicast:
                    networkManager.RegisterCallback<T>(oneTimeUse.TriggerEvent, receivingPort);
                    break;
                default:
                    networkManager.RegisterCallback<T>(oneTimeUse.TriggerEvent, receivingPort);
                    break;
            }

            return oneTimeUse.Unregister;

        }*/

        /// <summary>
        /// Allows for the receiving and responding messages to be sent via different calls.
        /// </summary>
        /// <param name="networkManager"></param>
        /// <param name="receivingMessageType">What message type is used for the receiving message</param>
        /// <param name="receivingPort">If <paramref name="receivingMessageType"/> is not Broadcast -> determines which port should be listened at</param>
        /// <param name="responseMessageType">What message type should be used to send the message</param>
        /// <param name="responseEndpoint">If <paramref name="responseMessageType"/> is not Broadcast -> determines which NetworkEndpoint (Unicast) or port (Multicast) should be used for sending</param>
        /// <param name="callerId">Caller id of the method caller</param>
        /// <param name="suppressLocalBroadcast">Used if <paramref name="responseMessageType"/> is broadcast to determine if local broadcast messages should be sent (if local connections exist</param>
        /// <typeparam name="T">IRespondingSerializer data that the response targets</typeparam>
        /// <typeparam name="TResponse">Response to T</typeparam>
        public static Action RegisterResponse<T, TResponse>(this INetworkManager networkManager, ConnectionType receivingMessageType, ushort receivingPort,
            ConnectionType responseMessageType, NetworkEndpoint responseEndpoint, int callerId,
            bool suppressLocalBroadcast = false, bool persistent = false) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            return networkManager.RegisterResponse<T, TResponse>(null, receivingMessageType, receivingPort,
                responseMessageType, responseEndpoint, callerId,
                suppressLocalBroadcast, persistent);
            
        }

        private static Action<T, int> GetResponseMethod<T, TResponse>(this INetworkManager networkManager, ConnectionType responseMessageType, TResponse response, NetworkEndpoint responseEndpoint, int callerId, bool suppressLocalBroadcast) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            switch (responseMessageType)
            {
                case ConnectionType.Broadcast:
                    return (T data, int _) => networkManager.BroadcastRespond(data, response, callerId, suppressLocalBroadcast);
                case ConnectionType.Multicast:
                    return (T data, int _) => networkManager.MulticastRespond(data, response, responseEndpoint.Port, callerId);
                case ConnectionType.Unicast:
                    return (T data, int _) => networkManager.UnicastRespond(data, response, responseEndpoint, callerId);
                default:
                    return (T data, int _) => networkManager.MulticastRespond(data, response, responseEndpoint.Port, callerId);
            }
        }
        
        /*private static Action<T, int> GetResponseMethod<T, TResponse>(ConnectionType responseMessageType, NetworkEndpoint responseEndpoint, int callerId, bool suppressLocalBroadcast) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            switch (responseMessageType)
            {
                case ConnectionType.Broadcast:
                    return (T data, int _) => BroadcastRespond(data, callerId, suppressLocalBroadcast);
                case ConnectionType.Multicast:
                    return (T data, int _) => MulticastRespond(data, responseEndpoint.Port, callerId);
                case ConnectionType.Unicast:
                    return (T data, int _) => UnicastRespond(data, responseEndpoint, callerId);
                default:
                    return (T data, int _) => MulticastRespond(data, responseEndpoint.Port, callerId);
            }
        }*/
        
        public static Action RegisterPersistentBroadcastResponse<T, TResponse>(this INetworkManager networkManager, Action<T, int> actionToPerformBeforeResponse, int callerId, bool suppressLocalBroadcast) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            return networkManager.RegisterResponse<T, TResponse>(actionToPerformBeforeResponse, ConnectionType.Broadcast, default, ConnectionType.Broadcast, default, callerId, suppressLocalBroadcast, true);
        }

        public static Action RegisterPersistentBroadcastResponse<T, TResponse>(this INetworkManager networkManager, int callerId, bool suppressLocalBroadcast) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            return networkManager.RegisterResponse<T, TResponse>(ConnectionType.Broadcast, default, ConnectionType.Broadcast, default, callerId, suppressLocalBroadcast, true);
        }
        
        public static Action RegisterPersistentMulticastResponse<T, TResponse>(this INetworkManager networkManager, Action<T, int> actionToPerformBeforeResponse, ushort port,
            int callerId) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            return networkManager.RegisterResponse<T, TResponse>(actionToPerformBeforeResponse, ConnectionType.Multicast, port, ConnectionType.Multicast, NetworkEndpoint.AnyIpv4.WithPort(port), callerId, persistent: true);
        }
        
        public static Action RegisterPersistentMulticastResponse<T, TResponse>(this INetworkManager networkManager, ushort port,
            int callerId) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            return networkManager.RegisterResponse<T, TResponse>(ConnectionType.Multicast, port, ConnectionType.Multicast, NetworkEndpoint.AnyIpv4.WithPort(port), callerId, persistent: true);
        }
        
        public static Action RegisterPersistentUnicastResponse<T, TResponse>(this INetworkManager networkManager, Action<T, int> actionToPerformBeforeResponse, NetworkEndpoint endpoint,
            int callerId) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            return networkManager.RegisterResponse<T, TResponse>(actionToPerformBeforeResponse, ConnectionType.Unicast,endpoint.Port, ConnectionType.Unicast, endpoint, callerId, persistent: true);

        }
        
        public static Action RegisterPersistentUnicastResponse<T, TResponse>(this INetworkManager networkManager, NetworkEndpoint endpoint,
            int callerId) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            return networkManager.RegisterResponse<T, TResponse>(ConnectionType.Unicast,endpoint.Port, ConnectionType.Unicast, endpoint, callerId, persistent: true);

        }
        
        
        
        public static void RegisterBroadcastResponse<T, TResponse>(this INetworkManager networkManager, Action<T, int> actionToPerformBeforeResponse, int callerId, bool suppressLocalBroadcast) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            networkManager.RegisterResponse<T, TResponse>(actionToPerformBeforeResponse, ConnectionType.Broadcast, default, ConnectionType.Broadcast, default, callerId, suppressLocalBroadcast);
        }

        public static void RegisterBroadcastResponse<T, TResponse>(this INetworkManager networkManager, int callerId, bool suppressLocalBroadcast) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            networkManager.RegisterResponse<T, TResponse>(ConnectionType.Broadcast, default, ConnectionType.Broadcast, default, callerId, suppressLocalBroadcast);
        }
        
        public static void RegisterMulticastResponse<T, TResponse>(this INetworkManager networkManager, Action<T, int> actionToPerformBeforeResponse, ushort port,
            int callerId) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            networkManager.RegisterResponse<T, TResponse>(actionToPerformBeforeResponse, ConnectionType.Multicast, port, ConnectionType.Multicast, NetworkEndpoint.AnyIpv4.WithPort(port), callerId);
        }
        
        public static void RegisterMulticastResponse<T, TResponse>(this INetworkManager networkManager, ushort port,
            int callerId) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            networkManager.RegisterResponse<T, TResponse>(ConnectionType.Multicast, port, ConnectionType.Multicast, NetworkEndpoint.AnyIpv4.WithPort(port), callerId);
        }
        
        public static void RegisterUnicastResponse<T, TResponse>(this INetworkManager networkManager, Action<T, int> actionToPerformBeforeResponse, NetworkEndpoint endpoint,
            int callerId) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            networkManager.RegisterResponse<T, TResponse>(actionToPerformBeforeResponse, ConnectionType.Unicast,endpoint.Port, ConnectionType.Unicast, endpoint, callerId);

        }
        
        public static void RegisterUnicastResponse<T, TResponse>(this INetworkManager networkManager, NetworkEndpoint endpoint,
            int callerId) where T : IRespondingSerializer<TResponse>, new()
            where TResponse : IResponseIdentifier, ISerializer, new()
        {
            networkManager.RegisterResponse<T, TResponse>(ConnectionType.Unicast,endpoint.Port, ConnectionType.Unicast, endpoint, callerId);

        }
        
        
        public static void BroadcastRespond<T>(this INetworkManager networkManager, IRespondingSerializer<T> respondingSerializer, T response,
            int callerId, bool suppressLocalBroadcast = false)
            where T : IResponseIdentifier, ISerializer, new()
        {
            networkManager.Respond(respondingSerializer, response, default, callerId, ConnectionType.Broadcast, suppressLocalBroadcast);
        }
        
        public static void MulticastRespond<T>(this INetworkManager networkManager, IRespondingSerializer<T> respondingSerializer, T response, ushort targetPort,
            int callerId)
            where T : IResponseIdentifier, ISerializer, new()
        {
            networkManager.Respond(respondingSerializer, response, NetworkEndpoint.AnyIpv4.WithPort(targetPort), callerId, ConnectionType.Multicast);
        }
        
        public static void UnicastRespond<T>(this INetworkManager networkManager, IRespondingSerializer<T> respondingSerializer, T response, NetworkEndpoint endpoint,
            int callerId)
            where T : IResponseIdentifier, ISerializer, new()
        {
            networkManager.Respond(respondingSerializer, response, endpoint, callerId, ConnectionType.Unicast);
        }
        
        public static void BroadcastRespond<T>(this INetworkManager networkManager, IRespondingSerializer<T> respondingSerializer,
            int callerId, bool suppressLocalBroadcast = false)
            where T : IResponseIdentifier, ISerializer, new()
        {
            networkManager.Respond(respondingSerializer, default, callerId, ConnectionType.Broadcast, suppressLocalBroadcast);
        }
        
        public static void MulticastRespond<T>(this INetworkManager networkManager, IRespondingSerializer<T> respondingSerializer, ushort targetPort,
            int callerId)
            where T : IResponseIdentifier, ISerializer, new()
        {
            networkManager.Respond(respondingSerializer, NetworkEndpoint.AnyIpv4.WithPort(targetPort), callerId, ConnectionType.Multicast);
        }
        
        public static void UnicastRespond<T>(this INetworkManager networkManager, IRespondingSerializer<T> respondingSerializer, NetworkEndpoint endpoint,
            int callerId)
            where T : IResponseIdentifier, ISerializer, new()
        {
            networkManager.Respond(respondingSerializer, endpoint, callerId, ConnectionType.Unicast);
        }
        
        
        private static void Respond<T>(this INetworkManager networkManager, IRespondingSerializer<T> respondingSerializer, T response, NetworkEndpoint endpoint,
            int callerId, ConnectionType connectionType, bool suppressLocalBroadcast = false)
            where T : IResponseIdentifier, ISerializer, new()
        {
            networkManager.Respond(respondingSerializer.GenerateResponse(response), endpoint, callerId, connectionType, suppressLocalBroadcast);
        }
        
        private static void Respond<T>(this INetworkManager networkManager, IRespondingSerializer<T> respondingSerializer, NetworkEndpoint endpoint,
            int callerId, ConnectionType connectionType, bool suppressLocalBroadcast = false)
            where T : IResponseIdentifier, ISerializer, new() 
        {
            networkManager.Respond(respondingSerializer.GenerateResponse(), endpoint, callerId, connectionType, suppressLocalBroadcast);
        }
        
        private static void Respond<T>(this INetworkManager networkManager, T response, NetworkEndpoint endpoint,
            int callerId, ConnectionType connectionType, bool suppressLocalBroadcast = false)
            where T : IResponseIdentifier, ISerializer, new() 
        {
            switch (connectionType)
            {
                case ConnectionType.Broadcast:
                    networkManager.BroadcastMessage(response, callerId,
                        suppressLocalBroadcast);
                    break;
                case ConnectionType.Multicast:
                    networkManager.MulticastMessage(response, endpoint.Port, callerId);
                    break;
                case ConnectionType.Unicast:
                    networkManager.UnicastMessage(response, endpoint, callerId);
                    break;
                default:
                    networkManager.MulticastMessage(response, endpoint.Port, callerId);
                    break;
            }
            
            
        }


        internal static OneTimeActionWrapper<T> CreateOneTimeActionWrapper<T>(this INetworkManager networkManager, ConnectionType receivingMessageType,
            Action<T, int> oneTimeAction, ushort port, bool persistent) where T : ISerializer, new()
        {
            return new OneTimeActionWrapper<T>(networkManager, receivingMessageType, oneTimeAction, port, persistent);
        }

        private static CallbackStruct<T> CreateCallback<T>(this INetworkManager networkManager,
            TaskCompletionSource<T> source, T expectedReturnValue) where T : ISerializer, IResponseIdentifier, new()
        {
            return new CallbackStruct<T>
            {
                NetworkManager = networkManager,
                ExpectedReturnValue = expectedReturnValue,
                Source = source
            };
        }
        
        
        private struct CallbackStruct<T> where T : ISerializer, IResponseIdentifier, new()
        {
            public TaskCompletionSource<T> Source;
            public T ExpectedReturnValue;
            public INetworkManager NetworkManager;

            public void Callback(T data, int id)
            {
                if (data.SenderId == ExpectedReturnValue.SenderId && data.MessageId == ExpectedReturnValue.MessageId)
                {
                    Debug.Log("Data received and setting result");
                    NetworkManager.UnregisterCallback<T>(Callback, NetworkExtensions.DefaultPort);
                    Source.SetResult(data);
                }
                
            }
        }

        internal class AwaitMessageWrapper<T> where T : ISerializer, new()
        {
            private TaskCompletionSource<T> _task;
            private readonly Action<T, int> _actionBeforeResponse;
            private readonly Action<T, int> _responseAction;
            private readonly ushort _port;
            private readonly bool _persistent;
            private readonly INetworkManager _networkManager;
            private readonly ConnectionType _connectionType;

            public Task AwaitMessage()
            {
                if (_task.Task.IsCompleted)
                {
                    CreateTask();
                }

                return _task.Task;
            }

            private void CreateTask()
            {
                _task = new TaskCompletionSource<T>();
            }


            public AwaitMessageWrapper(INetworkManager networkManager, Action<T, int> actionBeforeResponse, Action<T, int> response, ConnectionType connectionType, ushort port, bool persistent)
            {
                _networkManager = networkManager;
                RegisterCallback<T>(_networkManager, _connectionType, _port, TriggerEvent);
                _actionBeforeResponse = actionBeforeResponse;
                _responseAction = response;
                _port = port;
                _persistent = persistent;
                _connectionType = connectionType;
                CreateTask();

            }

            public void TriggerEvent(T data, int id)
            {
                _actionBeforeResponse.Invoke(data, id);
                if (!_persistent)
                {
                    Unregister();
                }

                _responseAction.Invoke(data, id);
                _task.SetResult(data);
            }
            public void Unregister()
            {
                
                UnregisterCallback<T>(_networkManager, _connectionType, _port, TriggerEvent);
                if (!_task.Task.IsCompleted)
                {
                    _task.SetCanceled();
                }
            }
        }
        
        internal class OneTimeActionWrapper<T> where T : ISerializer,new()
        {
            private readonly Action<T, int> _oneTimeAction;
            private readonly ushort _port;
            private readonly bool _persistent;
            private readonly INetworkManager _networkManager;
            private readonly ConnectionType _receivingMessageType;

            internal OneTimeActionWrapper(INetworkManager networkManager, ConnectionType receivingMessageType, Action<T, int> oneTimeAction, ushort port, bool persistent)
            {
                _oneTimeAction = oneTimeAction;
                _port = port;
                _persistent = persistent;
                _networkManager = networkManager;
                _receivingMessageType = receivingMessageType;

            }

            public void TriggerEvent(T data, int callerId)
            {
                _oneTimeAction.Invoke(data, callerId);
                if (_persistent)
                {
                    return;
                }

                
                Unregister();
            }

            public void Unregister() => UnregisterCallback<T>(_networkManager, _receivingMessageType, _port, TriggerEvent);

        }
        
    }




}