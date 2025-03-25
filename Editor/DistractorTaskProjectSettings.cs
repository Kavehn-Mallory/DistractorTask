using System.IO;
using DistractorTask.Core;
using DistractorTask.Settings;
using UnityEditor;
using UnityEngine;

namespace DistractorTask.Editor
{
    
    [FilePath("ProjectSettings/Packages/com.janwittke.distractortask/DistractorTaskSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class DistractorTaskProjectSettings : BaseSettings<DistractorTaskProjectSettings>
    {

        [SerializeField]
        private ushort defaultPort = 7777;
        [SerializeField]
        private ushort ipListeningPort = 7500;
        [SerializeField]
        private ushort loggingPort = 7400;
        [SerializeField]
        private ushort videoPlayerPort = 7600;
    }
}