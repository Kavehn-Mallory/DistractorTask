using System.Threading.Tasks;
using DistractorTask.Core;
using DistractorTask.Editor.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DistractorTask.Editor
{
    public static class EditorBootstrapper
    {
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async void LoadBootstrapperSceneOnPlayModeStart()
        {
            AsyncOperation bootstrapperLoadOperation = null;
            Debug.Log("Bootstrapper...");
            
            var test = AssetDatabase.LoadAssetAtPath<SceneAsset>(Constants.EditorBootstrapperScenePath);
            //do nothing and just use the default scene?
            if (DistractorTaskUserSettings.instance.UseBootstrapper && test != null)
            {
                bootstrapperLoadOperation = EditorSceneManager.LoadSceneAsyncInPlayMode(Constants.EditorBootstrapperScenePath, new LoadSceneParameters
                {
                    loadSceneMode = LoadSceneMode.Single,
                    localPhysicsMode = LocalPhysicsMode.None
                });

            }
            if (bootstrapperLoadOperation == null)
            {
                return;
            }
            while (!bootstrapperLoadOperation.isDone)
            {
                await Task.Delay(100);
            }
        }
        
    }
}