using UnityEngine;

namespace DistractorTask.Settings
{

    public class RuntimeUserSettings : ScriptableObject
    {
        public UserSettings settings;

        public static RuntimeUserSettings Instance;

        private void OnEnable()
        {
            Instance = this;
        }
    }
}