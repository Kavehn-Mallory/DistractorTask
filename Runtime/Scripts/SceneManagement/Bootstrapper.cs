using System.Linq;
using System.Threading.Tasks;
using DistractorTask.Core;
using DistractorTask.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistractorTask.SceneManagement
{
    public class Bootstrapper : PersistentSingleton<Bootstrapper>
    {
        private const string ScenePath = "Packages/com.janwittke.distractortask/Runtime/Scenes/Bootstrapper/Editor_Bootstrapper.unity";
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async void Init()
        {
            AsyncOperation bootstrapperLoadOperation = null;
            Debug.Log("Bootstrapper...");

#if UNITY_EDITOR
            var test = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            //do nothing and just use the default scene?
            if (DistractorTaskUserSettings.instance.UseBootstrapper && test != null)
            {
                Debug.Log("Enabling scene");
                AddOrEnableScene(test);
                bootstrapperLoadOperation = SceneManager.LoadSceneAsync(test.name, LoadSceneMode.Single);
            }
#elif UNITY_ANDROID
//todo put custom scene for VR/AR
            bootstrapperLoadOperation = SceneManager.LoadSceneAsync("Android_Bootstrapper", LoadSceneMode.Single);

#else
//it's currently a little bit convoluted, but this way we can hopefully switch to the correct scene.
             bootstrapperLoadOperation = SceneManager.LoadSceneAsync("Win_Bootstrapper", LoadSceneMode.Single);
#endif
            /*if (RuntimePlatform.Android == Application.platform)
            {

            }
            else
            {

            }*/

            
            if (bootstrapperLoadOperation == null)
            {
                return;
            }
            while (!bootstrapperLoadOperation.isDone)
            {
                await Task.Delay(100);
            }
        }

#if UNITY_EDITOR

        private static void AddOrEnableScene(SceneAsset sceneAsset)
        {
            var scenes = EditorBuildSettings.scenes;

            var path = AssetDatabase.GetAssetOrScenePath(sceneAsset);
            
            for (var index = 0; index < scenes.Length; index++)
            {
                var scene = scenes[index];
                if (scene.path.Equals(path))
                {
                    scenes[index].enabled = true;
                    EditorBuildSettings.scenes = scenes;
                    return;
                }
                
            }
            
            var sceneList = scenes.ToList();
            sceneList.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = sceneList.ToArray();
        }
#endif
        
        
    }
    
}