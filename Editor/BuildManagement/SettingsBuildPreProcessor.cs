using System.IO;
using System.Linq;
using DistractorTask.Core;
using DistractorTask.Settings;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DistractorTask.Editor.BuildManagement
{
    public class SettingsBuildPreProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;


        
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!Directory.Exists(Constants.RuntimeSettingsPath))
            {
                Directory.CreateDirectory(Constants.RuntimeSettingsPath);
            }

            
            
            var runtimeSettings = ScriptableObject.CreateInstance<RuntimeUserSettings>();
            runtimeSettings.settings = DistractorTaskProjectSettings.instance.Settings;
            //runtimeSettings.hideFlags = HideFlags.DontSaveInEditor;
            
            var assets = PlayerSettings.GetPreloadedAssets().ToList();

            for (var i = assets.Count - 1; i >= 0; i--)
            {
                var asset = assets[i];
                if (asset is RuntimeUserSettings settings)
                {
                    assets.RemoveAt(i);
                }
            }

            
            AssetDatabase.CreateAsset(runtimeSettings, Constants.RuntimeSettingsFullPath);
            
            var assetList = assets.ToList();
            
            assetList.Add(runtimeSettings);
            PlayerSettings.SetPreloadedAssets(assetList.ToArray());
        }
    }
}