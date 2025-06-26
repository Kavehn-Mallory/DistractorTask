using TMPro;
using UnityEngine;

namespace DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents
{
    public static class DistractorPlacementExtension
    {
    
        public static void PlaceLabelsAtPosition(this TMP_Text label, float distanceFromCenter, float angle)
        {
            var rotatedVector = Rotate(new Vector2(0, distanceFromCenter), angle * Mathf.Deg2Rad);
            label.rectTransform.anchoredPosition = rotatedVector;
        }

        private static Vector2 Rotate(Vector2 v, float delta) {
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }
    
    }
}