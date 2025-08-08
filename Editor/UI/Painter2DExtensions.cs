using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace DistractorTask.Editor.UI
{
    public static class Painter2DExtensions
    {
        public static void DrawRectangle(this Painter2D painter, Vector2 bottomLeft, Vector2 topRight, bool fillRectangle = false)
        {
            painter.BeginPath();
            painter.MoveTo(bottomLeft);
            painter.LineTo(new Vector2(bottomLeft.x, topRight.y));
            painter.LineTo(new Vector2(topRight.x, topRight.y));
            painter.LineTo(new Vector2(topRight.x, bottomLeft.y));
            painter.ClosePath();
            if (fillRectangle)
            {
                painter.Fill();
                return;
            }
            painter.Stroke();
        }

        public static void DrawLine(this Painter2D painter, Vector2 start, Vector2 end)
        {
            painter.BeginPath();
            painter.MoveTo(start);
            painter.LineTo(end);
            painter.Stroke();
        }
        
        public static void DrawBoxPlot(this Painter2D painter, float graphMin, float graphMax, float min,
            float max, float lowerQuartile, float upperQuartile, float median, Vector2 topLeft, Vector2 size, Color borderColor,
            Color boxBorderColor, Color boxColor, Color whiskerColor, Color medianColor)
        {

            var bottomRight = topLeft + size;
            
            var graphTopLeft = topLeft + new Vector2(0.1f, 0) * size;
            var graphBottomRight = bottomRight - new Vector2(0.1f, 0) * size;
            
            var actualSize = graphBottomRight - graphTopLeft;
            
            graphMin = Math.Min(graphMin, min - 0.1f);
            graphMax = Math.Max(graphMax, max + 0.1f);
            
            var distanceMinMax = graphMax - graphMin;
            var distancePerUnit = actualSize.y / distanceMinMax;

            var boxTopLeft = graphTopLeft + new Vector2(0, distancePerUnit * (graphMax - upperQuartile));
            var boxBottomRight = graphBottomRight - new Vector2(0, distancePerUnit * (lowerQuartile - graphMin));
            
            painter.strokeColor = boxBorderColor;
            painter.fillColor = boxColor;
            
            painter.DrawRectangle(boxTopLeft, boxBottomRight, true);


            var boxWidth = boxBottomRight.x - boxTopLeft.x;

            var bottomCenter = boxBottomRight - new Vector2(boxWidth / 2f, 0);
            var bottomWhiskerEnd = bottomCenter + new Vector2(0, distancePerUnit * (lowerQuartile - min));
            
            
            var topCenter = boxTopLeft + new Vector2(boxWidth / 2f, 0);
            
            var topWhiskerEnd = topCenter - new Vector2(0, distancePerUnit * (max - upperQuartile));
            
            
            painter.strokeColor = whiskerColor;
            //whisker lines vertical
            painter.DrawLine(bottomCenter, bottomWhiskerEnd);
            painter.DrawLine(topCenter,  topWhiskerEnd);
            
            
            //whisker lines horizontal
            painter.DrawLine(bottomWhiskerEnd - new Vector2(boxWidth * 0.25f, 0), bottomWhiskerEnd + new Vector2(boxWidth * 0.25f, 0));
            painter.DrawLine(topWhiskerEnd - new Vector2(boxWidth * 0.25f,  0), topWhiskerEnd + new Vector2(boxWidth * 0.25f,  0));

            painter.strokeColor = medianColor;
            
            //median
            painter.DrawLine(graphTopLeft + new Vector2(0, distancePerUnit * (graphMax - median)), graphTopLeft + new Vector2(boxWidth, distancePerUnit * (graphMax - median)));





        }




    }
}