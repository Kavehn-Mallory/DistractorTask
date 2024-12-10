using System;
using UnityEngine;

namespace DistractorTask.Transport
{
    [Serializable]
    public struct ConnectionPortProperty
    {
        public ushort Port
        {
            get
            {
                if (data)
                {
                    return data.port;
                }

                return port;
            }
        }

        public ConnectionPortProperty(ushort port)
        {
            this.port = port;
            data = null;
            useReference = false;
        }

        private PortData data;
        [SerializeField]
        private ushort port;

        [HideInInspector]
        public bool useReference;

        public override string ToString()
        {
            return Port.ToString();
        }
    }
}