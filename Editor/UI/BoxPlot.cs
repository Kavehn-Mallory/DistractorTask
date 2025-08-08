using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace DistractorTask.Editor.UI
{
    [UxmlElement]
    public abstract partial class BoxPlot<T> : VisualElement
    {

        private T[] _values = Array.Empty<T>();

        [UxmlAttribute]
        public T[] Values
        {
            get => _values;
            set
            {
                _values = value;
                MarkDirtyRepaint();
            }
        }


        public BoxPlot()
        {
            
        }

        

    }
}