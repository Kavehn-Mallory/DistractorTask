using System;
using DistractorTask.Core;
using UnityEngine;

namespace DistractorTask.Transport
{
    public static class DataSerializationIndexer
    {
        private static Type[] _typeIndices = Array.Empty<Type>();


        [RuntimeInitializeOnLoadMethod]
        public static void CalculateTypeIndices()
        {
            var types = NetworkMessageEventHandler.GetSerializableTypes();
            types.Sort(SortByName);

            _typeIndices = new Type[types.Count + 1];

            for (int i = 0; i < types.Count; i++)
            {
                _typeIndices[i + 1] = types[i];
            }
            _typeIndices[0] = null;
        }

        private static int SortByName(Type x, Type y)
        {
            return string.Compare(x.AssemblyQualifiedName, y.AssemblyQualifiedName, StringComparison.Ordinal);
        }

        public static byte GetTypeIndex(Type type)
        {
            for (int i = 0; i < _typeIndices.Length; i++)
            {
                if (_typeIndices[i] == type)
                {
                    return (byte)i;
                }
            }
            //zero is associated with "null" instead of a type, indicating that the type does not exist
            return 0;
        }

        public static byte GetTypeIndex<T>() where T : ISerializer
        {
            return GetTypeIndex(typeof(T));
        }

        public static Type GetTypeForTypeIndex(byte index)
        {
            return _typeIndices[index];
        }
    }
}