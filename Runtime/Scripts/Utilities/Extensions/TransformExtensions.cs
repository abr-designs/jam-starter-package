using System.Runtime.CompilerServices;
using UnityEngine;
using Utilities.Enums;

namespace Utilities.Extensions
{
    public static class TransformExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPosition(this Transform transform, SPACE space, Vector3 targetPosition)
        {
            if (space == SPACE.WORLD)
                transform.position = targetPosition;
            else
                transform.localPosition = targetPosition;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRotation(this Transform transform, SPACE space, Quaternion targetRotation)
        {
            if (space == SPACE.WORLD)
                transform.rotation = targetRotation;
            else
                transform.localRotation = targetRotation;
        }
    }
}