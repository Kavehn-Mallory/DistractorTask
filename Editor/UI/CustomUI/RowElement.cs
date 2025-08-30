using UnityEngine.UIElements;

namespace DistractorTask.Editor.UI.CustomUI
{
    public class RowElement : VisualElement
    {


        private Label[] _labels;
        
        public RowElement() : this(1)
        {
            
        }

        public RowElement(int columns) : this(new string[columns])
        {
            
        }

        public RowElement(string[] columnContent)
        {
            style.flexDirection = FlexDirection.Row;
            _labels = new Label[columnContent.Length];
            for (int i = 0; i < columnContent.Length; i++)
            {
                _labels[i] = new Label(columnContent[i])
                {
                    style =
                    {
                        width = 100
                    }
                };
                Add(_labels[i]);
            }
        }
    }
}