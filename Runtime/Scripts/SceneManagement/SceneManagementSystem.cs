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
            NetworkManager.Instance.RegisterCallback<SceneGroupChangeData>(OnSceneGroupChangeRequest);
        }

        private void OnDisable()
        {
            NetworkManager.Instance.UnregisterCallback<SceneGroupChangeData>(OnSceneGroupChangeRequest);
        }

        private void OnSceneGroupChangeRequest(SceneGroupChangeData data, int callerId)
        {
            if (callerId == GetInstanceID())
            {
                return;
            }
            sceneLoader?.LoadSceneGroup(data.index);
        }
        
    }
}