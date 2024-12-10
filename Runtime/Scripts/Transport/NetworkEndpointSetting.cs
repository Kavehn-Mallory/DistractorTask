using System;

namespace DistractorTask.Transport
{
    [Serializable]
    public enum NetworkEndpointSetting
    {
        AnyIPv4,
        AnyIPv6,
        LoopbackIPv4,
        LoopbackIPv6,
        Custom
    }
}