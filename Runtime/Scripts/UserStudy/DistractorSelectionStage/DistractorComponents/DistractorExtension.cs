using TMPro;
using UnityEngine;

namespace DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents
{
    public static class DistractorPlacementExtension
    {
    
        public static void PlaceLabelsAtPosition(this TMP_Text label, float distanceFromCenter, float angle)
        {
        
            var rotatedVector = Rotate(new Vector3(0, distanceFromCenter), angle * Mathf.Deg2Rad);
            
            Debug.Log(rotatedVector);

            label.rectTransform.anchoredPosition = new Vector2(rotatedVector.x, rotatedVector.y);
        }
        
        public static Vector2 Rotate(Vector2 v, float delta) {
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }
    
    }
}