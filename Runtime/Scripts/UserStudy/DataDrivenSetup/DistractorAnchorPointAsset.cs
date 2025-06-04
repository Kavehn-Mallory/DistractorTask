using System;
using DistractorTask.Core;
using UnityEngine;

namespace DistractorTask.UserStudy.DataDrivenSetup
{
    [CreateAssetMenu(fileName = "DistractorAnchorPointAsset", menuName = "DistractorTask/DataContainers/DistractorAnchorPointAsset", order = 0)]
    public class DistractorAnchorPointAsset : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private Vector3[] distractorPlacementPositions = Array.Empty<Vector3>();

        public int Length => distractorPlacementPositions.Length;
        
        
        public void Reset()
        {
            distractorPlacementPositions = Array.Empty<Vector3>();
        }

        public void InitializeContainer(int length)
        {
            distractorPlacementPositions = new Vector3[length];
        }

        public bool SetPosition(int index, Vector3 position)
        {
            if (index < 0 || index >= distractorPlacementPositions.Length)
            {
                return false;
            }

            distractorPlacementPositions[index] = position;
            return true;
        }

        public Vector3 GetRandomPosition()
        {
            return distractorPlacementPositions.RandomElement();
        }

        public Vector3 GetPosition(int index)
        {
            if (index < 0 || index >= distractorPlacementPositions.Length)
            {
                throw new IndexOutOfRangeException($"Index {index} was out of range {Length} of the {nameof(DistractorAnchorPointAsset)}");
            }
            return distractorPlacementPositions[index];
        }
        
        public bool TryGetPosition(int index, out Vector3 position)
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