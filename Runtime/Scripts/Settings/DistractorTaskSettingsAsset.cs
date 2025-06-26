using System;
using UnityEngine;

namespace DistractorTask.Settings
{
    [CreateAssetMenu(fileName = "DistractorTaskSettingsAsset", menuName = "DistractorTask/Settings")]
    [Serializable]
    public class DistractorTaskSettingsAsset : ScriptableObject
    {
        [SerializeField]
        public bool generateUserId;
        public UserSettings userSettings;
        
        
        public static DistractorTaskSettingsAsset Instance;
        
        
        //todo update user settings in here as well and then make sure to include in build. maybe we can even somehow tie the id thing to the build type 
    }
}