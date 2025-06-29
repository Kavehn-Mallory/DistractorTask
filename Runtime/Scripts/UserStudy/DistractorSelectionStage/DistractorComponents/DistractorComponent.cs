using TMPro;
using UnityEngine;

namespace DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents
{
    public class DistractorComponent : MonoBehaviour
    {
        public int distractorIndex = -1;

        public DistractorTaskComponent Component { get; set; }
        
        private TMP_Text _text;
        private RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform;
        public TMP_Text Text => _text;
    
        private void Awake()
        {
            _text = GetComponentInChildren<TMP_Text>();
            _rectTransform = GetComponent<RectTransform>();
        }
    

        public void UpdateDistractorSize(float distractorSize)
        {
            _rectTransform.sizeDelta = new Vector2(distractorSize, distractorSize);
            _text.fontSize = distractorSize;
        }
        

        
    
    }
}