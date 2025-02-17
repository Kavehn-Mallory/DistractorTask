using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents
{
    [RequireComponent(typeof(TMP_Text))]
    public class DistractorComponent : MonoBehaviour
    {
        public int distractorIndex = -1;

        public DistractorTaskComponent Component { get; set; }
        
        private TMP_Text _text;
        private RectTransform _rectTransform;
    
        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _rectTransform = GetComponent<RectTransform>();
        }
    

        public void UpdateDistractorSize(float distractorSize)
        {
            _rectTransform.sizeDelta = new Vector2(distractorSize, distractorSize);
            _text.fontSize = distractorSize;
        }
        

        
    
    }
}