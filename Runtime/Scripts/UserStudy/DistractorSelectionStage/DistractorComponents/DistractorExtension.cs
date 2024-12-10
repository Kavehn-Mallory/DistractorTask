using TMPro;
using UnityEngine;

namespace DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents
{
    public static class DistractorPlacementExtension
    {
    
        public static void PlaceLabelsAtPosition(this TMP_Text label, Transform cameraTransform, float distanceFromCenter, float angle)
        {
            var rotation = Quaternion.AngleAxis(angle, -cameraTransform.forward);
        
            var rotatedVector = rotation * new Vector2(0, distanceFromCenter);

            label.rectTransform.anchoredPosition = rotatedVector;
        }
    
    }
}