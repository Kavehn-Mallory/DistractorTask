using DistractorTask.Transport;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DistractorTask.Editor
{
    [CustomPropertyDrawer(typeof(IpAddress))]
    public class IpAddressPropertyDrawer : PropertyDrawer
    {
        
        private const string RootElementPath = "Assets/UI/IpAddressField.uxml";
        private VisualTreeAsset _rootAsset;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (!_rootAsset)
            {
                _rootAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RootElementPath);
            }
            var rootElement = _rootAsset.Instantiate();
            rootElement.BindProperty(property);

            var test = rootElement.Query<IntegerField>().Build();
            foreach (var integerField in test)
            {
                integerField.RegisterValueChangedCallback((e) => OnIpValueChanged(integerField, e));
            }
            return rootElement;

        }

        private void OnIpValueChanged(IntegerField integerField, ChangeEvent<int> evt)
        {
            if (evt.newValue > 255)
            {
                integerField.SetValueWithoutNotify(255);
            }

            if (evt.newValue < 0)
            {
                integerField.SetValueWithoutNotify(0);
            }
        }
    }
}