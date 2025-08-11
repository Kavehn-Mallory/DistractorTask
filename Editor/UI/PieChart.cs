using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


namespace DistractorTask.Editor.UI
{
    
    [UxmlElement]
    public partial class PieChart : VisualElement
    {
        private float _radius = 25.0f;

        public float radius
        {
            get => _radius;
            set
            {
                _radius = value;
            }
        }

        public float diameter => _radius * 2.0f;
        
        public float[] Values
        {
            get => _values;
        }

        private float[] _values = Array.Empty<float>();

        public PieChart()
        {
            /*_values = new float[]
            {
                20f, 50f, 100f, 30f
            };*/
            generateVisualContent += DrawCanvas;
        }

        private void DrawCanvas(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;
            painter.strokeColor = Color.white;
            painter.fillColor = Color.white;

            var percentages = _values;

            if (percentages.Length == 0)
            {
                percentages = new float[]
                {
                    1f
                };
            }

            var totalValue = percentages.Sum();



            var center = layout.size / 2f;
            
            var colors = new Color32[] {
                new Color32(182,235,122,255),
                new Color32(251,120,19,255),
                new Color32(50,120,50,255),
                new Color32(251,20,19,255)
            };
            float angle = 0.0f;
            float anglePct = 0.0f;
            int k = 0;
            foreach (var pct in percentages)
            {
                anglePct += 360.0f * (pct / totalValue);

                
                painter.fillColor = colors[k];
                k = ((k + 1) % colors.Length);
                painter.BeginPath();
                painter.MoveTo(center);
                painter.Arc(center, _radius, angle, anglePct);
                painter.Fill();

                angle = anglePct;
            }
        }
    }
}