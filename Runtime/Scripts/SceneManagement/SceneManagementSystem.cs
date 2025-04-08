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
            NetworkManager.Instance.RegisterCallbackAllPorts<SceneGroupChangeData>(OnSceneGroupChangeRequest);
        }

        private void OnDisable()
        {
            NetworkManager.Instance.UnregisterCallbackAllPorts<SceneGroupChangeData>(OnSceneGroupChangeRequest);
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