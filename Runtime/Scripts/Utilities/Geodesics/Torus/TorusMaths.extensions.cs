using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Geodesics
{
    public static partial class TorusMaths
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 TorusUVToWorldPoint(Torus torus, float u, float v)
        {
            return TorusUVToWorldPoint(torus.transform, torus.majorRadius, torus.minorRadius, u, v);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetTorusTangent(Torus torus, float u, float v, bool majorDirection)
        {
            return GetTorusTangent(torus.transform, torus.majorRadius, torus.minorRadius, u, v, majorDirection);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetTorusNormal(Torus torus, float u, float v)
        {
            return GetTorusNormal(torus.transform, torus.majorRadius, torus.minorRadius, u, v);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ClosestPointOnTorus(Torus torus, Vector3 worldPos)
        {
            return ClosestPointOnTorus(torus.transform, torus.majorRadius, torus.minorRadius, worldPos);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WorldPointToTorusUV(Torus torus, Vector3 worldPos,
            out float u, out float v)
        {
            WorldPointToTorusUV(torus.transform, torus.majorRadius, torus.minorRadius, worldPos, out u, out v);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WorldDirectionToTorusDirection(Torus torus,
            Vector3 worldDirection, Vector3 worldPosition, out float du, out float dv)
        {
            WorldDirectionToTorusDirection(torus.transform, torus.majorRadius, torus.minorRadius, worldDirection,
                worldPosition, out du, out dv);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(Torus torus, Vector2 uv1, Vector2 uv2)
        {
            return DistanceSqr(torus.majorRadius, torus.minorRadius, uv1, uv2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Torus torus, Vector2 uv1, Vector2 uv2)
        {
            return Distance(torus.majorRadius, torus.minorRadius, uv1, uv2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetOppositeWorldPoint(Torus torus, Vector2 uv)
        {
            return GetOppositeWorldPoint(torus.transform, torus.majorRadius, torus.minorRadius, uv);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetMajorCirclePosition(Torus torus, float u)
        {
            return GetMajorCirclePosition(torus.transform, torus.majorRadius, torus.minorRadius, u);
        }
    }
}