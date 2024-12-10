using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents
{
    [RequireComponent(typeof(TMP_Text))]
    public class DistractorComponent : MonoBehaviour
    {
        public int distractorIndex = -1;

        public DistractorTaskComponent Component { get; set; }

        private PressableButton _button;
        private TMP_Text _text;
        private RectTransform _rectTransform;
    
        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _rectTransform = GetComponent<RectTransform>();
            //replace the pressable button with normal buttons and replace the input logic 
            var pressableButton = GetComponent<PressableButton>();
            GetComponent<Button>()?.onClick.AddListener(OnButtonClicked);
        
            pressableButton?.OnClicked.AddListener(OnButtonClicked);
            _button = pressableButton;
        
            //pressableButton.is
        }
    

        public void UpdateDistractorSize(float distractorSize)
        {
            _rectTransform.sizeDelta = new Vector2(distractorSize, distractorSize);
            _text.fontSize = distractorSize;
        }



        private void OnButtonClicked()
        {
            Component.OnButtonClicked(distractorIndex);
        }
    
    }
}