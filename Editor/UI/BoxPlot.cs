using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DistractorTask.Editor.UI
{
    [Serializable]
    [UxmlElement]
    public abstract partial class BoxPlot<T> : VisualElement
    {
        [Header("Color Selection")]
        [UxmlAttribute]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                _borderColorWasSet = true;
            }
        }
        
        [UxmlAttribute]
        public Color BoxBorderColor
        {
            get => _boxBorderColor;
            set
            {
                _boxBorderColor = value;
                _boxBorderColorWasSet = true;
            }
        }

        [UxmlAttribute(name = "box-color")]
        public Color BoxColor
        {
            get => _boxColor;
            set
            {
                _boxColor = value;
                _boxColorWasSet = true;
            }
        }

        [UxmlAttribute]
        public Color WhiskerColor
        {
            get => _whiskerColor;
            set
            {
                _whiskerColor = value;
                _whiskerColorWasSet = true;
            }
        }
        
        [UxmlAttribute]
        public Color MedianColor
        {
            get => _medianColor;
            set
            {
                _medianColor = value;
                _medianColorWasSet = true;
            }
        }
        
        [UxmlAttribute]
        public Color MeanColor
        {
            get => _meanColor;
            set
            {
                _meanColor = value;
                _meanColorWasSet = true;
            }
        }
        
        
        private bool _borderColorWasSet = false;
        private bool _boxBorderColorWasSet = false;
        private bool _boxColorWasSet = false;
        private bool _whiskerColorWasSet = false;
        private bool _medianColorWasSet = false;
        private bool _meanColorWasSet = false;
        
        
        
        public static readonly string ussClassName = "statistics-boxplot";
        static readonly CustomStyleProperty<Color> BorderColorStyleProperty = new CustomStyleProperty<Color>("--border-color");
        static readonly CustomStyleProperty<Color> BoxBorderColorStyleProperty = new CustomStyleProperty<Color>("--box-border-color");
        static readonly CustomStyleProperty<Color> BoxColorStyleProperty = new CustomStyleProperty<Color>("--box-color");
        static readonly CustomStyleProperty<Color> WhiskerColorStyleProperty = new CustomStyleProperty<Color>("--whisker-color");
        static readonly CustomStyleProperty<Color> MedianColorStyleProperty = new CustomStyleProperty<Color>("--median-color");
        static readonly CustomStyleProperty<Color> MeanColorStyleProperty = new CustomStyleProperty<Color>("--mean-color");
        static readonly CustomStyleProperty<bool> DrawBorderStyleProperty = new CustomStyleProperty<bool>("--draw-border");
        static readonly CustomStyleProperty<bool> DrawMeanStyleProperty = new CustomStyleProperty<bool>("--draw-mean");
        
        
        internal static readonly BindingId medianColorStyleProperty = (BindingId) "style.median-color";
        
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

        [Header("Display Settings")]
        [UxmlAttribute]
        public bool DrawBorder
        {
            get => _drawBorder;
            set => _drawBorder = value;
        }

        [UxmlAttribute]
        public bool DrawMean
        {
            get => _drawMean;
            set => _drawMean = value;
        }
        
        private bool _drawBorder;
        private bool _drawMean;


        private Color _borderColor;
        private Color _boxBorderColor;
        private Color _boxColor;
        private Color _whiskerColor;
        private Color _medianColor;
        private Color _meanColor;
        


        public BoxPlot()
        {
            AddToClassList(ussClassName);
            RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);
            //this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(DefaultStyleSheetPath));
            
        }

        public void OnStylesResolved(CustomStyleResolvedEvent evt)
        {
            
            evt.ResolveCustomStyleSheetProperty(BorderColorStyleProperty, ref _borderColor, _borderColorWasSet);
            evt.ResolveCustomStyleSheetProperty(BoxBorderColorStyleProperty, ref _boxBorderColor, _boxBorderColorWasSet);
            evt.ResolveCustomStyleSheetProperty(BoxColorStyleProperty, ref _boxColor, _boxColorWasSet);
            evt.ResolveCustomStyleSheetProperty(WhiskerColorStyleProperty, ref _whiskerColor, _whiskerColorWasSet);
            evt.ResolveCustomStyleSheetProperty(MedianColorStyleProperty, ref _medianColor, _medianColorWasSet);
            evt.ResolveCustomStyleSheetProperty(MeanColorStyleProperty, ref _meanColor, _meanColorWasSet);
            
            evt.ResolveCustomStyleSheetProperty(DrawBorderStyleProperty, ref _drawBorder);
            evt.ResolveCustomStyleSheetProperty(DrawMeanStyleProperty, ref _drawMean);
            
        }
    }
}