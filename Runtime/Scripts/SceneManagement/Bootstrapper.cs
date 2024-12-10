using System.Threading.Tasks;
using DistractorTask.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
#endif

namespace DistractorTask.SceneManagement
{
    public class Bootstrapper : PersistentSingleton<Bootstrapper>
    {
        private static bool _loadBootstrapper;

#if UNITY_EDITOR
        [MenuItem("Tools/Toggle Bootstrapper", true)]
        private static bool SetToggleState()
        {
            Menu.SetChecked("Tools/Toggle Bootstrapper", _loadBootstrapper);
            return true;
        }

        [MenuItem("Tools/Toggle Bootstrapper")]
        public static void ToggleBootstrapper()
        {
            _loadBootstrapper = !_loadBootstrapper;
        }
#endif    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static async void Init()
        {
            AsyncOperation bootstrapperLoadOperation = null;
            Debug.Log("Bootstrapper...");

#if UNITY_EDITOR
            //do nothing and just use the default scene?
            if (_loadBootstrapper)
            {
                bootstrapperLoadOperation = SceneManager.LoadSceneAsync("Editor_Bootstrapper", LoadSceneMode.Single);
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
    }
}