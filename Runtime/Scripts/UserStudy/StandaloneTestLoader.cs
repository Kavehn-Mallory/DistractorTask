using System;
using System.Threading.Tasks;
using DistractorTask.SceneManagement;
using DistractorTask.UI;
using UnityEngine;

namespace DistractorTask.UserStudy
{
    public class StandaloneTestLoader : MonoBehaviour
    {
        public int sceneGroup;

        public SceneLoader sceneLoader;
        public StandaloneMessageBroker broker;

        private void Awake()
        {
            if (broker)
            {
                broker.OnStandaloneSceneLoadingButtonPressed += LoadScene;
            }
        }

        public void LoadScene()
        {
            LoadSceneAdditively();
        }

        [ContextMenu("Load scene")]
        public async Task LoadSceneAdditively()
        {
            if (!sceneLoader)
            {
                return;
            }

            await sceneLoader.LoadSceneGroup(sceneGroup);
            Debug.Log("Loaded additional scene");
        }
    }
}