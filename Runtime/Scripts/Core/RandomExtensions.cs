﻿using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace DistractorTask.Core
{
    public static class RandomExtensions
    {
        public static T RandomElement<T>(this T[] elements)
        {
            return elements[Random.Range(0, elements.Length)];
        }
        
        public static T RandomElement<T>(this T[] elements, int startIndex, int endIndex)
        {
            startIndex = math.clamp(startIndex, 0, elements.Length);
            endIndex = math.clamp(endIndex, 0, elements.Length);
            return elements[Random.Range(startIndex, endIndex)];
        }
    }
}