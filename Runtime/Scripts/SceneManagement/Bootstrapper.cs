using System.Threading.Tasks;
using DistractorTask.Core;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace DistractorTask.SceneManagement
{
    public class Bootstrapper : PersistentSingleton<Bootstrapper>
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async void Init()
        {
            AsyncOperation bootstrapperLoadOperation = null;
            Debug.Log("Bootstrapper...");

#if UNITY_EDITOR
            //we just return because the EditorBootstrapper will handle the scene loading for us
            return;
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


        
        
    }
    
}