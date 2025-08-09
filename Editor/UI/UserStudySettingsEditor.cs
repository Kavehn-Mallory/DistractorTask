using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DistractorTask.Editor.UI
{
    public class UserStudySettingsEditor : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        
        public static void ShowWindow(UserStudySettings settings)
        {
            UserStudySettingsEditor wnd = GetWindow<UserStudySettingsEditor>();
            wnd.titleContent = new GUIContent("UserStudySettingsEditor");
            wnd.SetSettingsAsset(settings);
            wnd.ShowModal();
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
        

            // Instantiate UXML
            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);
        }

        public void SetSettingsAsset(UserStudySettings settings)
        {
            var serializedObject = new SerializedObject(settings);
            rootVisualElement.Q<ObjectField>().Bind(serializedObject);
        }
        
    }
}
