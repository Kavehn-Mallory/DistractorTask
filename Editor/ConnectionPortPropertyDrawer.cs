using DistractorTask.Transport;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DistractorTask.Editor
{
    [CustomPropertyDrawer(typeof(ConnectionPortProperty))]
    public class ConnectionPortPropertyDrawer : PropertyDrawer
    {
        
        private const string RootElementPath = "Assets/UI/PortProperty.uxml";
        private VisualTreeAsset _rootAsset;
        private PropertyField _propertyField;
        private ObjectField _objectField;
        private TemplateContainer _rootElement;
        private Foldout _foldout;

        private const string ReferencePortField = "data";
        private const string ValuePortField = "port";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            if (!_rootAsset)
            {
                _rootAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RootElementPath);
            }
            
            _rootElement = _rootAsset.Instantiate();

            _propertyField = new PropertyField();
            
            _foldout = _rootElement.Q<Foldout>("PortFoldOut");
            _foldout.text = property.displayName;
            _foldout.viewDataKey = "FoldoutState";

            var referenceToggle = _rootElement.Q<Toggle>("UseReference");

            referenceToggle.RegisterValueChangedCallback(OnReferenceTogglePressed);
            
            _objectField = new ObjectField
            {
                bindingPath = ReferencePortField,
                objectType = typeof(PortData),
                label = "Reference"
            };

            _propertyField.bindingPath = ValuePortField;
            _foldout.Add(_propertyField);
            if (property.FindPropertyRelative(nameof(ConnectionPortProperty.useReference)).boolValue)
            {
                _foldout.Remove(_propertyField);
                _foldout.Add(_objectField);
            }

            
            _rootElement.BindProperty(property);

            return _rootElement;
        }

        private void OnReferenceTogglePressed(ChangeEvent<bool> evt)
        {
            if (!evt.previousValue && evt.newValue)
            {
                _foldout.Remove(_propertyField);
                _foldout.Add(_objectField);
            }
            else if(evt.previousValue && !evt.newValue)
            {
                _foldout.Remove(_objectField);
                _foldout.Add(_propertyField);
            }
        }
    }
}