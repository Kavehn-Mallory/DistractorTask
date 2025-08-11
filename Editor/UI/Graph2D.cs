using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DistractorTask.Editor.UI
{
    [UxmlElement]
    public partial class Graph2D : VisualElement
    {

        public const string VisualTreeAssetPath = "Packages/com.janwittke.distractortask/Editor/UI/Graph2D.uxml";
        
        [UxmlAttribute]
        public List<GraphContainer> Graphs
        {
            get => _graphs;
            set
            {
                
                _graphs = value;
                MarkDirtyRepaint();
            }
        }
        
        [UxmlAttribute]
        public Vector2 RangeXAxis
        {
            get => _rangeXAxis;
            set
            {
                _rangeXAxis = value;
                MarkDirtyRepaint();
            }
        }

        [UxmlAttribute]
        public Vector2 RangeYAxis
        {
            get => _rangeYAxis;
            set
            {
                _rangeYAxis = value;
                MarkDirtyRepaint();
            }
        }

        [UxmlAttribute]
        public GraphType SelectedGraphType
        {
            get => _graphType;
            set
            {
                _graphType = value;
                MarkDirtyRepaint();
            }
        }

        private GraphType _graphType = GraphType.PointAndLine;

        [UxmlAttribute]
        public AxisAutoResize AxisAutoResizeSetting
        {
            get => _axisAutoResizeSetting;
            set
            {
                _axisAutoResizeSetting = value;
                MarkDirtyRepaint();
            }
        }

        public void AddGraph(Vector2[] points)
        {
            _graphs.Add(points);
            MarkDirtyRepaint();
        }

        public void RemoveGraphAt(int index)
        {
            _graphs.RemoveAt(index);
            MarkDirtyRepaint();
        }

        public void ClearGraph()
        {
            _graphs.Clear();
            MarkDirtyRepaint();
        }
        

        private AxisAutoResize _axisAutoResizeSetting = AxisAutoResize.Both;

        private List<GraphContainer> _graphs = new();

        private Vector2 _rangeYAxis;
        private Vector2 _rangeXAxis;
        private float _radiusOfDataPoints = 0.2f;
        
        private GraphMargins _border = new GraphMargins(1f, 1f, 10f, 10f);
        
        
        private Color _backgroundColor = Color.white;
        private Color _graphColor = Color.red;
        private Color _axisColor = Color.black;

        private Label _yAxisLabel;
        private Label _xAxisLabel;

        public Graph2D()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(VisualTreeAssetPath);
            visualTreeAsset.CloneTree(this);
            _yAxisLabel = this.Q<Label>("YAxisLabel");
            _xAxisLabel = this.Q<Label>("YAxisLabel");
            //PlaceLabels();
            generateVisualContent += DrawCanvas;
        }

        private void PlaceLabels()
        {
           
            if (_yAxisLabel == null)
            {
                _yAxisLabel = new Label("Y-Axis");
                Add(_yAxisLabel);
            }
            var position = _yAxisLabel.style.position;
            position.value = Position.Absolute;
            _yAxisLabel.style.alignSelf = Align.FlexStart;
            _yAxisLabel.style.position = position;
            var labelPosition = layout.position / 2f;
            _yAxisLabel.layout.Set(0, labelPosition.y, _yAxisLabel.layout.width, _yAxisLabel.layout.height);
            
            
            if (_xAxisLabel == null)
            {
                _xAxisLabel = new Label("X-Axis");
                Add(_xAxisLabel);
            }
            
            position = _xAxisLabel.style.position;
            position.value = Position.Absolute;
            _xAxisLabel.style.position = position;
            labelPosition = layout.position / 2f;
            _xAxisLabel.layout.Set(labelPosition.x, layout.position.y + layout.size.y, _xAxisLabel.layout.width, _xAxisLabel.layout.height);
            
        }

        private void DrawCanvas(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;


            var topLeft = new Vector2();
            var bottomRight = topLeft + layout.size;

            //need to remember that this thing is "higher" than the topLeft position
            Vector2 topLeftGraph = topLeft + new Vector2(_border.left, _border.top);
            Vector2 bottomRightGraph = bottomRight - new Vector2(_border.right, _border.bottom);
            
            painter.fillColor = _backgroundColor;
            painter.strokeColor = _backgroundColor;
            painter.DrawRectangle(topLeft, bottomRight, true);
            //painter.Cl
            
            //draw background 
            
            //draw y axis

            painter.strokeColor = _axisColor;
            
            painter.DrawLine(topLeftGraph, new Vector2(topLeftGraph.x, bottomRightGraph.y));
            
            //draw x axis 

            painter.DrawLine(new Vector2(topLeftGraph.x, bottomRightGraph.y), bottomRightGraph);
            
            painter.fillColor = _graphColor;
            painter.strokeColor = _graphColor;
            
            foreach (var graph in _graphs)
            {
                var points = graph.points;
                for (int i = 0; i < points.Length - 1; i++)
                {
                    var p0 = math.clamp(points[i], topLeftGraph, bottomRightGraph);
                    var p1 = math.clamp(points[i + 1], topLeftGraph, bottomRightGraph);

                    
                    //if either x or y for both points are identical AND that x or y is identical to the min / max of the graph -> reject point
                    var isHorizontal = (p0.x.Equals(p1.x) && (p0.x.Equals(topLeftGraph.x) || p0.Equals(bottomRightGraph.x))) ||
                                       (p0.y.Equals(p1.y) && (p0.y.Equals(topLeftGraph.y) || p0.y.Equals(bottomRightGraph.y)));

                    if (isHorizontal)
                    {
                        continue;
                    }

                    p0 = new float2(p0.x, bottomRightGraph.y - p0.y);
                    p1 = new float2(p1.x, bottomRightGraph.y - p1.y);

                    if (SelectedGraphType == GraphType.PointAndLine)
                    {
                        painter.DrawLine(p0, p1);
                        painter.DrawCircle(p0, _radiusOfDataPoints);
                        continue;
                    }

                    if (SelectedGraphType == GraphType.Point)
                    {
                        painter.DrawCircle(p0, _radiusOfDataPoints);
                        continue;
                    }
                    
                    if (SelectedGraphType == GraphType.Line)
                    {
                        painter.DrawLine(p0, p1);
                    }
                    
                    var lastPoint = math.clamp(points[^1], topLeftGraph, bottomRightGraph);
                    painter.DrawCircle(lastPoint, _radiusOfDataPoints);
                    
                }
                
                
            }
            
            //draw data points 
            
            //draw connection?
        }

        [Serializable]
        public enum AxisAutoResize
        {
            None,
            XAxis,
            YAxis,
            Both
        }

        [Serializable]
        public enum GraphType
        {
            Point,
            Line,
            PointAndLine
        }

        [Serializable]
        public struct GraphMargins
        {
            public float top;
            public float right;
            public float bottom;
            public float left;

            public GraphMargins(float top, float right, float bottom, float left)
            {
                this.top = top;
                this.right = right;
                this.bottom = bottom;
                this.left = left;
            }
        }

        [Serializable]
        public class GraphContainer
        {
            public Vector2[] points;

            public GraphContainer()
            {
                this.points = Array.Empty<Vector2>();
            }
            
            public GraphContainer(Vector2[] points)
            {
                this.points = points;
            }

            public static implicit operator GraphContainer(Vector2[] points) => new GraphContainer(points);

            public static implicit operator Vector2[](GraphContainer container) => container.points;
        }
    }
}