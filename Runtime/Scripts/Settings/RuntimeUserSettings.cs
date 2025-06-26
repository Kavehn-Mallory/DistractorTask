using System;
using UnityEngine;

namespace DistractorTask.Settings
{

    [Obsolete]
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