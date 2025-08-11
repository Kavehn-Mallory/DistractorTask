using System;
using System.Text;
using UnityEditor.UIElements;
using UnityEngine;

namespace DistractorTask.Editor.UI
{
    public class GraphContainerConverter : UxmlAttributeConverter<Graph2D.GraphContainer>
    {
        public override Graph2D.GraphContainer FromString(string value)
        {
            
            var items = value.Split(';');
            var result = new Graph2D.GraphContainer();
            result.points = new Vector2[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                result.points[i] = (Vector2)Convert.ChangeType(item[0], typeof(Vector2));
                
            }

            return result;
        }

        public override string ToString(Graph2D.GraphContainer value)
        {
            var builder = new StringBuilder();
            builder.AppendJoin(';', value.points);
            return builder.ToString();
        }
    }
}