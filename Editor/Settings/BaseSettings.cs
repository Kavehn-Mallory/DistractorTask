
#if UNITY_EDITOR

using System.IO;
using UnityEditor;

namespace DistractorTask.Editor.Settings
{
    public abstract class BaseSettings<T> : ScriptableSingleton<T> where T : BaseSettings<T>
    {
        
        public void Save() => this.Save(true);

        public static SerializedObject GetSerializedSettings()
        {
            var settings = instance;
            if (!File.Exists(GetFilePath()))
            {
                settings.Save();
            }
            return new SerializedObject(settings);
        }
        private void OnDisable()
        {
            Save();
        }

        private void OnDestroy()
        {
            Save();
        }
    }
}

#endif
