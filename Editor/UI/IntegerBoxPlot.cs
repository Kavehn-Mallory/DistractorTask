using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace DistractorTask.Editor.UI
{
    [UxmlElement]
    public partial class IntegerBoxPlot : BoxPlot<int>
    {
        public IntegerBoxPlot()
        {
            var values = new int[11];
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
            
            generateVisualContent += DrawCanvas;
        }

        private void DrawCanvas(MeshGenerationContext ctx)
        {
            if (Values == null || Values.Length == 0)
            {
                return;
            }
            var painter = ctx.painter2D;

            Array.Sort(Values);

            var graphMin = Values[0] - 0.1f;
            var graphMax = Values[^1] + 0.1f;

            var lowerQuartileIndex = (int)(Values.Length * 0.25f);
            var upperQuartileIndex = (int)(Values.Length * 0.75f);
            
            
            painter.DrawBoxPlot(graphMin, graphMax, Values[0], Values[^1], Values[lowerQuartileIndex],
                Values[upperQuartileIndex], Values[Values.Length / 2], new Vector2(), this.layout.size, Color.white,
                Color.red, Color.red, Color.black, Color.blue);
            

        }
        
        


        
    }
}