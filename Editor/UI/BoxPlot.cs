using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DistractorTask.Editor.UI
{
    [Serializable]
    [UxmlElement]
    public partial class BoxPlot : VisualElement
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

        [UxmlAttribute]
        public Vector2 GraphRange
        {
            get => _graphRange;
            set
            {
                _graphRange = value;
                CheckGraphRange();
                MarkDirtyRepaint();
            }
        }

        private void CheckGraphRange()
        {
            //if(math.any)
            _graphRange.x = Mathf.Min(_graphRange.x, _minMax.x);
            _graphRange.y = Mathf.Max(_graphRange.y, _minMax.y);
        }

        private Vector2 _graphRange;

        private Vector2 _quartiles;
        private Vector2 _minMax;
        
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
        
        private float[] _values = Array.Empty<float>();

        private float _median;
        private float _mean;

        [UxmlAttribute]
        public float[] Values
        {
            get => _values;
            set
            {
                _values = value;
                RecalculateBoxPlotValues();
                MarkDirtyRepaint();
            }
        }

        
        
        private void RecalculateBoxPlotValues()
        {
            if (Values == null || Values.Length == 0)
            {
                _quartiles = new Vector2(0, 0);
                _minMax = new Vector2(0, 0);
                return;
            }

            Array.Sort(Values);

            var lowerQuartileIndex = (int)(Values.Length * 0.25f);
            var upperQuartileIndex = (int)(Values.Length * 0.75f);

            _quartiles = new Vector2(Values[lowerQuartileIndex], Values[upperQuartileIndex]);
            _minMax = new Vector2(Values[0], Values[^1]);

            _median = Values[Values.Length / 2];
            if (Values.Length % 2 == 0 && Values.Length > 1)
            {
                var halfPoint = Values.Length / 2;
                _median = (Values[halfPoint - 1] + _median) / 2f;
            }

            _mean = Values.Average();

            Debug.Log($"Box Plot: {_minMax.ToString()}. Median: {_median}. Mean: {_mean}");
            CheckGraphRange();
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

            if (_values == Array.Empty<float>())
            {
                var values = new float[11];
                values[0] = 1;
                values[1] = 3;
                values[2] = 5;
                values[3] = 6;
                values[4] = 76;
                values[5] = 23;
                values[6] = 5;
                values[7] = 50;
                values[8] = 47;
                values[9] = 100;
                values[10] = 19;

                Values = values;
            }

            
            generateVisualContent += DrawCanvas;
            
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
        
        private void DrawCanvas(MeshGenerationContext ctx)
        {
            if (Values == null || Values.Length == 0)
            {
                return;
            }
            var painter = ctx.painter2D;
            
            
            painter.DrawBoxPlot(_graphRange.x, _graphRange.y, _minMax.x, _minMax.y, _quartiles.x,
                _quartiles.y, _median, new Vector2(), this.layout.size, BorderColor,
                BoxBorderColor, BoxColor, WhiskerColor, MedianColor, DrawBorder);
            

        }
    }
}