using System;
using DistractorTask.Transport;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DistractorTask.Editor
{
    [CustomPropertyDrawer(typeof(ConnectionDataSettings))]
    public class ConnectionDataSettingsPropertyDrawer : PropertyDrawer
    {

        private const string RootElementPath = "Assets/UI/ConnectionDataSettingsPropertyDrawer.uxml";
        private VisualTreeAsset _rootAsset;
        private VisualElement _contentContainer;
        private VisualElement[] _contentElements;
        private VisualElement _hiddenContainer;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (!_rootAsset)
            {
                _rootAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RootElementPath);
            }
            
            
            var rootElement = _rootAsset.Instantiate();

            rootElement.BindProperty(property);
            
            var enumField = rootElement.Q<EnumField>();
            enumField.Init(NetworkEndpointSetting.AnyIPv4);
            _contentContainer = rootElement.Q<VisualElement>("ContentArea");
            _hiddenContainer = rootElement.Q<VisualElement>("HiddenContainer");
            _contentContainer.Add(_hiddenContainer);
            CreateContentContainers(property);
            var networkEndpointSetting = property.FindPropertyRelative(nameof(ConnectionDataSettings.endpointSource));
            //enumField.bindingPath = nameof(ConnectionDataSettings.endpointSource);
            enumField.BindProperty(networkEndpointSetting);
            enumField.RegisterValueChangedCallback((e) => OnNetworkEndpointSettingsChanged(e, property));
            enumField.value = (NetworkEndpointSetting)networkEndpointSetting.enumValueIndex;


            
            
            //var enumValue = enumField.value is NetworkEndpointSetting value ? value : NetworkEndpointSetting.AnyIPv4;
            var chosenContainer = _contentElements[networkEndpointSetting.enumValueIndex];
            chosenContainer.RemoveFromHierarchy();
            _contentContainer.Insert(0, chosenContainer);
            
            var foldout = rootElement.Q<Foldout>("Root");
            foldout.text = property.displayName;
            
            return rootElement;
        }
        private void CreateContentContainers(SerializedProperty property)
        {
            var enumNames = Enum.GetNames(typeof(NetworkEndpointSetting));
            _contentElements = new VisualElement[enumNames.Length];

            for (int i = 0; i < _contentElements.Length; i++)
            {
                var container = CreateContentContainerForEnumData(enumNames[i], property);
                _contentElements[i] = container;
                _hiddenContainer.Add(container);
            }
        }

        private VisualElement CreateContentContainerForEnumData(string enumName, SerializedProperty property)
        {
            switch (enumName)
            {
                case nameof(NetworkEndpointSetting.AnyIPv4): return CreateAnyIPv4Container(property);
                case nameof(NetworkEndpointSetting.AnyIPv6): return CreateAnyIPv6Container(property);
                case nameof(NetworkEndpointSetting.LoopbackIPv4): return CreateLoopbackIPv4Container(property);
                case nameof(NetworkEndpointSetting.LoopbackIPv6): return CreateLoopbackIPv6Container(property);
                case nameof(NetworkEndpointSetting.Custom): return CreateCustomContainer(property);
            }

            Debug.LogWarning("We went out of range");
            return new VisualElement();
        }

        
        //todo set the actual ip address to the correct thing?
        private VisualElement CreateAnyIPv4Container(SerializedProperty property)
        {
            return CreatePortField(property);
        }

        private static PropertyField CreatePortField(SerializedProperty property)
        {
            var portProperty = property.FindPropertyRelative(nameof(ConnectionDataSettings.port));
            var portField = new PropertyField(portProperty);
            return portField;
        }
        
        private VisualElement CreateAnyIPv6Container(SerializedProperty property)
        {
            return CreatePortField(property);
        }
        
        private VisualElement CreateLoopbackIPv4Container(SerializedProperty property)
        {
            return CreatePortField(property);
        }
        
        private VisualElement CreateLoopbackIPv6Container(SerializedProperty property)
        {
            return CreatePortField(property);
        }
        
        private VisualElement CreateCustomContainer(SerializedProperty property)
        {
            var ipAddressProperty = property.FindPropertyRelative(nameof(ConnectionDataSettings.ipAddress));
            var portContainer = CreatePortField(property);

            var data = new VisualElement();
            data.Add(new PropertyField(ipAddressProperty));
            data.Add(portContainer);
            return data;
        }

        

        private void OnNetworkEndpointSettingsChanged(ChangeEvent<Enum> evt, SerializedProperty property)
        {
            if (evt.newValue is not NetworkEndpointSetting setting || evt.previousValue is not NetworkEndpointSetting old)
            {
                return;
            }

            if (setting == old)
            {
                return;
            }

            var oldContainer = _contentElements[(int)old];
            oldContainer.RemoveFromHierarchy();
            _hiddenContainer.Add(oldContainer);
            var newContainer = _contentElements[(int)setting];
            newContainer.RemoveFromHierarchy();
            _contentContainer.Insert(0, newContainer);
            
        }
    }
}