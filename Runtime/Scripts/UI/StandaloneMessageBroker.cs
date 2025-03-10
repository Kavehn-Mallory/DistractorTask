using UnityEngine;
using UnityEngine.Events;

namespace DistractorTask.UI
{
    [CreateAssetMenu(fileName = "MessageBroker", menuName = "Transport/MessageBroker", order = 0)]
    public class StandaloneMessageBroker : ScriptableObject
    {
                
        public UnityAction OnStandaloneSceneLoadingButtonPressed = delegate { };
        public UnityAction OnStandaloneStudyStart = delegate { };

        public void OnStandaloneSceneLoadPressed()
        {
            OnStandaloneSceneLoadingButtonPressed.Invoke();
        }
        
        public void OnStandaloneStudyStartPressed()
        {
            OnStandaloneStudyStart.Invoke();
        }
    }
}