using System.Threading.Tasks;
using DistractorTask.Core;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace DistractorTask.SceneManagement
{
    public class Bootstrapper : PersistentSingleton<Bootstrapper>
    {

        private const string WindowsBootstrapper = "Win_Bootstrapper";
        private const string HololensBootstrapper = "Android_Bootstrapper";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async void Init()
        {
            AsyncOperation bootstrapperLoadOperation = null;
            Debug.Log("Bootstrapper...");
            if (IsSceneInBuild(HololensBootstrapper))
            {
                bootstrapperLoadOperation = SceneManager.LoadSceneAsync(HololensBootstrapper, LoadSceneMode.Single);
            }
#if UNITY_EDITOR
            //we just return because the EditorBootstrapper will handle the scene loading for us
            return;
#elif UNITY_ANDROID
//todo put custom scene for VR/AR
            if (IsSceneInBuild(HololensBootstrapper))
            {
                bootstrapperLoadOperation = SceneManager.LoadSceneAsync(HololensBootstrapper, LoadSceneMode.Single);
            }
#else
//it's currently a little bit convoluted, but this way we can hopefully switch to the correct scene.
            if (IsSceneInBuild(WindowsBootstrapper))
            {
                bootstrapperLoadOperation = SceneManager.LoadSceneAsync(WindowsBootstrapper, LoadSceneMode.Single);
            }
#endif
            
            
            if (bootstrapperLoadOperation == null)
            {
                return;
            }
            while (!bootstrapperLoadOperation.isDone)
            {
                await Task.Delay(100);
            }
        }
        
        public static bool IsSceneInBuild(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var sceneNameInBuildSetting = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                if (sceneNameInBuildSetting.Equals(sceneName))
                {
                    return true;
                }
            }
            return false;
        }


        
        
    }
    
}