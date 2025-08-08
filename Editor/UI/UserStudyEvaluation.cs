using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DistractorTask.Editor.UI
{
    public class UserStudyEvaluation : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset visualTreeAsset = default;

        [MenuItem("Window/DistractorTask/UserStudyEvaluation")]
        public static void ShowExample()
        {
            UserStudyEvaluation wnd = GetWindow<UserStudyEvaluation>();
            wnd.titleContent = new GUIContent("UserStudyEvaluation");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            

            // Instantiate UXML
            VisualElement labelFromUXML = visualTreeAsset.Instantiate();
            
            var tabView = root.Q<TabView>();

            if (tabView != null)
            {
                SetupTabs(tabView);

                tabView.activeTabChanged += OnActiveTabChanged;
            }
            
            root.Add(labelFromUXML);
        }

        private void OnActiveTabChanged(Tab oldTab, Tab newTab)
        {
            //throw new System.NotImplementedException();
        }

        private void SetupTabs(TabView tabView)
        {
            var overviewTab = tabView.Q<Tab>("Overview");
            var participantTab = tabView.Q<Tab>("PerParticipant");
            var conditionTab = tabView.Q<Tab>("PerCondition");
            
            
        }
    }
}
