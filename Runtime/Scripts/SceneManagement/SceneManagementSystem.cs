using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using UnityEngine;

namespace DistractorTask.SceneManagement
{
    public class SceneManagementSystem : MonoBehaviour
    {
        [SerializeField]
        private SceneLoader sceneLoader;
        private void OnEnable()
        {
            sceneLoader = FindObjectOfType<SceneLoader>();
            NetworkConnectionManager.Instance.RegisterCallback<SceneGroupChangeData>(OnSceneGroupChangeRequest);
        }

        private void OnDisable()
        {
            NetworkConnectionManager.Instance.UnregisterCallback<SceneGroupChangeData>(OnSceneGroupChangeRequest);
        }

        private void OnSceneGroupChangeRequest(SceneGroupChangeData data)
        {
            sceneLoader?.LoadSceneGroup(data.index);
        }
        
    }
}