using System;
using DistractorTask.Core;
using DistractorTask.RoomAnalysis;
using UnityEngine;

namespace DistractorTask.UserStudy.DataDrivenSetup
{
    [CreateAssetMenu(fileName = "DistractorAnchorPointAsset", menuName = "DistractorTask/DataContainers/DistractorAnchorPointAsset", order = 0)]
    public class DistractorAnchorPointAsset : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private AnchorPoint[] distractorPlacementPositions = Array.Empty<AnchorPoint>();

        public int Length => distractorPlacementPositions.Length;
        
        
        public void Reset()
        {
            distractorPlacementPositions = Array.Empty<AnchorPoint>();
        }

        public void InitializeContainer(int length)
        {
            distractorPlacementPositions = new AnchorPoint[length];
        }

        public bool SetPosition(int index, AnchorPoint position)
        {
            if (index < 0 || index >= distractorPlacementPositions.Length)
            {
                return false;
            }

            distractorPlacementPositions[index] = position;
            return true;
        }

        public AnchorPoint GetRandomPosition()
        {
            return distractorPlacementPositions.RandomElement();
        }

        public AnchorPoint GetAnchorPoint(int index)
        {
            if (index < 0 || index >= distractorPlacementPositions.Length)
            {
                throw new IndexOutOfRangeException($"Index {index} was out of range {Length} of the {nameof(DistractorAnchorPointAsset)}");
            }
            return distractorPlacementPositions[index];
        }
        
        public bool TryGetPosition(int index, out AnchorPoint position)
        {
            if (index < 0 || index >= distractorPlacementPositions.Length)
            {
                position = default;
                return false;
            }

            position = distractorPlacementPositions[index];
            return true;
        }
        
    }
}