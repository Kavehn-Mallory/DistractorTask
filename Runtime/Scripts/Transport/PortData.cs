using UnityEngine;

namespace DistractorTask.Transport
{
    [CreateAssetMenu(fileName = "Port", menuName = "Transport/ConnectionData/Port", order = 0)]
    public class PortData : ScriptableObject
    {
        public ushort port;
    }
}