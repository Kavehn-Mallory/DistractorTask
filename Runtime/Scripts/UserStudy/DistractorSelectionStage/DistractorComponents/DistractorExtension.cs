using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents
{
    public static class DistractorPlacementExtension
    {
    
        public static void PlaceLabelsAtPosition(this RectTransform rectTransform, float distanceFromCenter, float angle)
        {
            var rotatedVector = Rotate(new Vector2(0, distanceFromCenter), angle * Mathf.Deg2Rad);
            rectTransform.anchoredPosition = rotatedVector;
        }

        private static Vector2 Rotate(Vector2 v, float delta) {
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }
        
        public static float CalculateActualSize(float r, float alpha)
        {
            var radians = math.radians(alpha);
            return math.abs(2f * r * math.tan((radians / 2f)));
        }

        public static Vector2 XY(this Vector3 value)
        {
            return new Vector2(value.x, value.y);
        }
    
    }
}