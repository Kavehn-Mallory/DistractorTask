using System;
using System.Collections.Generic;
using System.Linq;
using DistractorTask.Core;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace DistractorTask.Transport
{
    public class NetworkMessageEventHandler
    {
        
        
        private readonly IInvoker[] _invocationHelper;

        public NetworkMessageEventHandler()
        {
            var serializableData = GetSerializableTypes();
            _invocationHelper = new IInvoker[serializableData.Count];
            Debug.Log(_invocationHelper.Length);
            for (var i = 0; i < serializableData.Count; i++)
            {
                var data = serializableData[i];
                var invoker = typeof(InvocationHelper<>).MakeGenericType(data);

                _invocationHelper[i] = (IInvoker)Activator.CreateInstance(invoker);
                
            }
        }

        public static List<Type> GetSerializableTypes()
        {
            
#if UNITY_EDITOR
            return TypeCache.GetTypesDerivedFrom<ISerializer>().Where(s => s.IsValueType || s.GetConstructor(Type.EmptyTypes) != null).ToList();
#else
            var result = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                result.AddRange(assembly.GetTypes().Where(t =>
                    (t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null) &&
                    typeof(ISerializer).IsAssignableFrom(t)));
            }

            return result;
#endif
            
        }




        public bool TriggerCallback<T>(T data) where T : ISerializer, new()
        {
            foreach (var invoker in _invocationHelper)
            {
                if (invoker is InvocationHelper<T> invocationHelper)
                {
                    Debug.Log("Found a local invoker");
                    invocationHelper.Invoke(data);
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

        public void RegisterCallback<T>(Action<T> test) where T : ISerializer, new()
        {
            foreach (var invoker in _invocationHelper)
            {
                if (invoker is InvocationHelper<T> helper)
                {
                    helper.RegisterCallback(test);
                    return;
                }
            }
        }
        
        public void UnregisterCallback<T>(Action<T> test) where T : ISerializer, new()
        {
            foreach (var invoker in _invocationHelper)
            {
                if (invoker is InvocationHelper<T> helper)
                {
                    helper.UnregisterCallback(test);
                    return;
                }
            }
        }
        
        
        public interface IInvoker
        {
            public Type InvocationType { get; }
            
            public void Invoke(ref DataStreamReader stream);
        }
        
        public interface IInvoker<T> : IInvoker where T : ISerializer
        {

            public void RegisterCallback(Action<T> callback);

            public void UnregisterCallback(Action<T> callback);
        }
        
        public class InvocationHelper<T> : IInvoker<T> where T : ISerializer, new()
        {

            public Type InvocationType => typeof(T);
            
            private Action<T> _actionToInvoke = delegate {};

            public void Invoke(ref DataStreamReader stream)
            {
                var data = new T();
                data.Deserialize(ref stream);
                _actionToInvoke.Invoke(data);
            }

            public void Invoke(T data)
            {
                _actionToInvoke.Invoke(data);
            }

            public void RegisterCallback(Action<T> callback)
            {
                _actionToInvoke += callback;
            }

            public void UnregisterCallback(Action<T> callback)
            {
                _actionToInvoke -= callback;
            }
        }
    }
}