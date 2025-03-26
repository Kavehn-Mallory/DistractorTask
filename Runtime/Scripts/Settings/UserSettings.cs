using System;
using UnityEngine;

namespace DistractorTask.Settings
{
    [Serializable]
    public class UserSettings
    {
        [SerializeField]
        private ushort defaultPort = 7777;
        [SerializeField]
        private ushort ipListeningPort = 7500;
        [SerializeField]
        private ushort loggingPort = 7400;
        [SerializeField]
        private ushort videoPlayerPort = 7600;


        public ushort DefaultPort => defaultPort;
        public ushort IPListeningPort => ipListeningPort;
        public ushort LoggingPort => loggingPort;
        public ushort VideoPlayerPort => videoPlayerPort;
        
    }
}