using System;
using System.Collections.Generic;
using System.Linq;
using DistractorTask.Core;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace DistractorTask.Transport
{
    public class NetworkMessageEventHandler
    {
        
        
        private readonly IInvoker[] _invocationHelper;

        private static readonly Predicate<Type> IsValidType = t =>
            (t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null) && !t.IsAbstract;

        public NetworkMessageEventHandler()
        {
            var serializableData = GetSerializableTypes();
            _invocationHelper = new IInvoker[serializableData.Count];
            Debug.Log(_invocationHelper.Length);
            for (var i = 0; i < serializableData.Count; i++)
            {
                var data = serializableData[i];
                var invoker = typeof(InvocationHelper<>).MakeGenericType(data);
                //Debug.Log($"Type: {data}");
                _invocationHelper[i] = (IInvoker)Activator.CreateInstance(invoker);
            }
        }
        

        public static List<Type> GetSerializableTypes()
        {
#if UNITY_EDITOR
            return TypeCache.GetTypesDerivedFrom<ISerializer>().Where(t => IsValidType.Invoke(t)).ToList();
#else
            return GetSerializableTypesThroughAssemblies();
#endif
            
        }

        private static List<Type> GetSerializableTypesThroughAssemblies()
        {
            var result = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                result.AddRange(assembly.GetTypes().Where(t =>
                    (IsValidType.Invoke(t)) &&
                    typeof(ISerializer).IsAssignableFrom(t)));
            }

            return result;
        }


        
        public bool TriggerCallback(Type type, ISerializer data, int callerId)
        {
            foreach (var invoker in _invocationHelper)
            {
                if (type == invoker.InvocationType)
                {
                    invoker.Invoke(data, callerId);
                    return true;
                }
            }

            return false;
        } 


        public bool TriggerCallback<T>(T data, int callerId) where T : ISerializer, new()
        {
            foreach (var invoker in _invocationHelper)
            {
                if (invoker is InvocationHelper<T> invocationHelper)
                {
                    invocationHelper.Invoke(data, callerId);
                    return true;
                }
            }

            return false;
        }
        
        

        public bool TriggerCallback(Type type, ref DataStreamReader stream)
        {
            foreach (var invoker in _invocationHelper)
            {
                if (type == invoker.InvocationType)
                {
                    invoker.Invoke(ref stream);
                    return true;
                }
            }

            return false;
        }
        
        public bool TriggerCallback(Type type, ref DataStreamReader stream, out ISerializer data)
        {
            foreach (var invoker in _invocationHelper)
            {
                if (type == invoker.InvocationType)
                {
                    data = invoker.Invoke(ref stream);
                    return true;
                }
            }

            data = null;
            return false;
        }

        public void RegisterCallback<T>(Action<T, int> callback) where T : ISerializer, new()
        {
            foreach (var invoker in _invocationHelper)
            {
                if (invoker is InvocationHelper<T> helper)
                {
                    helper.RegisterCallback(callback);
                    return;
                }
            }
        }
        
        public void UnregisterCallback<T>(Action<T, int> callback) where T : ISerializer, new()
        {
            foreach (var invoker in _invocationHelper)
            {
                if (invoker is InvocationHelper<T> helper)
                {
                    helper.UnregisterCallback(callback);
                    return;
                }
            }
        }
        
        
        public interface IInvoker
        {
            public Type InvocationType { get; }
            
            public ISerializer Invoke(ref DataStreamReader stream);

            public void Invoke(ISerializer data, int callerId);
        }
        
        public interface IInvoker<T> : IInvoker where T : ISerializer
        {

            public void RegisterCallback(Action<T, int> callback);

            public void UnregisterCallback(Action<T, int> callback);
        }
        
        public class InvocationHelper<T> : IInvoker<T> where T : ISerializer, new()
        {

            public Type InvocationType => typeof(T);
            
            private Action<T, int> _actionToInvoke = delegate {};
            
            public ISerializer Invoke(ref DataStreamReader stream)
            {
                var data = new T();
                data.Deserialize(ref stream);
                _actionToInvoke.Invoke(data, 0);
                return data;
            }

            public void Invoke(ISerializer data, int callerId)
            {
                if (data is T castedData)
                {
                    Invoke(castedData, callerId);
                }
            }

            public void Invoke(T data, int callerId)
            {
                _actionToInvoke.Invoke(data, callerId);
            }

            public void RegisterCallback(Action<T, int> callback)
            {
                _actionToInvoke += callback;
            }

            public void UnregisterCallback(Action<T, int> callback)
            {
                _actionToInvoke -= callback;
            }
        }
    }
}