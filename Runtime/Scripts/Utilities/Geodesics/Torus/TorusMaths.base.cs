using UnityEngine;

namespace Utilities.Geodesics
{
    public static partial class TorusMaths
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="torusTransform"></param>
        /// <param name="R">Major Radius</param>
        /// <param name="r">Minor Radius</param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 TorusUVToWorldPoint(Transform torusTransform, float R, float r, float u, float v)
        {
            float cosU = Mathf.Cos(u), sinU = Mathf.Sin(u);
            float cosV = Mathf.Cos(v), sinV = Mathf.Sin(v);

            float rad = R + r * cosV;

            Vector3 local = new Vector3(
                rad * cosU,
                r * sinV,
                rad * sinU
            );

            return LocalToWorldScaled(local, torusTransform);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="torusTransform"></param>
        /// <param name="R">Major Radius</param>
        /// <param name="r">Minor Radius</param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="majorDirection"></param>
        /// <returns></returns>
        public static Vector3 GetTorusTangent(Transform torusTransform, float R, float r,float u, float v, bool majorDirection)
        {
            // Tangent in u or v direction
            if (majorDirection)
            {
                // ∂P/∂u
                float cosV = Mathf.Cos(v);
                float sinU = Mathf.Sin(u);
                float cosU = Mathf.Cos(u);

                float rad = R + r * cosV;

                float x = -rad * sinU;
                float y = 0;
                float z = rad * cosU;

                return torusTransform.localToWorldMatrix.MultiplyVector(new Vector3(x, y, z).normalized);
            }
            else
            {
                // ∂P/∂v
                float cosV = Mathf.Cos(v);
                float sinV = Mathf.Sin(v);
                float cosU = Mathf.Cos(u);
                float sinU = Mathf.Sin(u);

                float x = -r * sinV * cosU;
                float y = r * cosV;
                float z = -r * sinV * sinU;

                return torusTransform.localToWorldMatrix.MultiplyVector(new Vector3(x, y, z).normalized);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="torusTransform"></param>
        /// <param name="R">Major Radius</param>
        /// <param name="r">Minor Radius</param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 GetTorusNormal(Transform torusTransform, float R, float r, float u, float v)
        {
            Vector3 tangentU = GetTorusTangent(torusTransform, R, r, u, v, true);
            Vector3 tangentV = GetTorusTangent(torusTransform, R, r, u, v, false);
            return Vector3.Cross(tangentU, tangentV).normalized;
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="torusTransform"></param>
        /// <param name="R">Major Radius</param>
        /// <param name="r">Minor Radius</param>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public static Vector3 ClosestPointOnTorus(Transform torusTransform, float R, float r, Vector3 worldPos)
        {
            WorldPointToTorusUV(torusTransform, R, r, worldPos, out float u, out float v);
            return TorusUVToWorldPoint(torusTransform, R, r, u, v);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="torusTransform"></param>
        /// <param name="R">Major Radius</param>
        /// <param name="r">Minor Radius</param>
        /// <param name="worldPos"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        public static void WorldPointToTorusUV(Transform torusTransform, float R, float r, Vector3 worldPos, out float u, out float v)
        {
            // Step 1: Convert world position to local space (including scale)
            Vector3 local = WorldToLocalScaled(worldPos, torusTransform);

            // Step 2: Toroidal angle u — angle in XZ plane (local torus plane)
            u = Mathf.Atan2(local.z, local.x);

            // Step 3: Major circle center at this u
            float cosU = Mathf.Cos(u), sinU = Mathf.Sin(u);
            Vector3 majorCenter = new Vector3(R * cosU, 0f, R * sinU);

            // Step 4: Vector from major circle to the point (poloidal offset)
            Vector3 offset = local - majorCenter;

            // Step 5: Construct local poloidal frame at angle u (no hardcoding!)
            // We'll define this in torus-local space first
            Vector3 toroidalTangent = new Vector3(-sinU, 0f, cosU);  // tangent along torus ring
            Vector3 toroidalNormal = Vector3.up;                     // up is "out of the ring" in local
            Vector3 poloidalRight  = Vector3.Cross(toroidalNormal, toroidalTangent); // perpendicular to ring

            // Step 6: Project offset into local poloidal plane to get (x, y)
            float x = Vector3.Dot(offset, poloidalRight);
            float y = Vector3.Dot(offset, toroidalNormal);

            // Step 7: Compute v as poloidal angle
            v = Mathf.Atan2(y, x);

            // Normalize to [0, 2π]
            if (u < 0f) u += 2f * Mathf.PI;
            if (v < 0f) v += 2f * Mathf.PI;
        }

        public static Vector3 WorldToLocalScaled(Transform torusTransform, Vector3 worldPos) => WorldToLocalScaled(worldPos, torusTransform);
        
        static Vector3 WorldToLocalScaled(Vector3 worldPos, Transform t)
        {
            Vector3 relative = worldPos - t.position;
            Quaternion invRot = Quaternion.Inverse(t.rotation);
            Vector3 scaled = invRot * relative;
            return new Vector3(
                scaled.x / t.localScale.x,
                scaled.y / t.localScale.y,
                scaled.z / t.localScale.z
            );        }

        static Vector3 LocalToWorldScaled(Vector3 localPos, Transform t)
        {
            Vector3 scaled = new Vector3(
                localPos.x * t.localScale.x,
                localPos.y * t.localScale.y,
                localPos.z * t.localScale.z
            );
            return t.position + t.rotation * scaled;        
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="torusTransform"></param>
        /// <param name="R">Major Radius</param>
        /// <param name="r">Minor Radius</param>
        /// <param name="worldDirection"></param>
        /// <param name="worldPosition"></param>
        /// <param name="du"></param>
        /// <param name="dv"></param>
        public static void WorldDirectionToTorusDirection(Transform torusTransform, float R, float r, Vector3 worldDirection, Vector3 worldPosition, out float du, out float dv)
        {
            WorldPointToTorusUV(torusTransform, R, r, worldPosition, out var u, out var v);
            // Step 2: Project direction onto tangent plane
            Vector3 normal = TorusMaths.GetTorusNormal(torusTransform, R, r,u, v);
            Vector3 projected = Vector3.ProjectOnPlane(worldDirection.normalized, normal);

            // Step 3: Get tangent basis and decompose
            Vector3 tangentU = TorusMaths.GetTorusTangent(torusTransform, R, r,u, v, true);
            Vector3 tangentV = TorusMaths.GetTorusTangent(torusTransform, R, r,u, v, false);
            du = Vector3.Dot(projected, tangentU);
            dv = Vector3.Dot(projected, tangentV);
        }
        
        public static Vector2 GetShortestTorusUVDirection(Vector2 from, Vector2 to)
        {
            float du = Mathf.Repeat((to.x - from.x + Mathf.PI), 2f * Mathf.PI) - Mathf.PI;
            float dv = Mathf.Repeat((to.y - from.y + Mathf.PI), 2f * Mathf.PI) - Mathf.PI;
            return new Vector2(du, dv).normalized;
        }
        
        public static Vector2 TorusShortestPathLerp(Vector2 uv1, Vector2 uv2, float t)
        {
            float u = ShortestAngleLerp(uv1.x, uv2.x, t);
            float v = ShortestAngleLerp(uv1.y, uv2.y, t);
            return new Vector2(u, v);
            
            float ShortestAngleLerp(float a, float b, float t)
            {
                float delta = Mathf.Repeat((b - a + Mathf.PI), 2f * Mathf.PI) - Mathf.PI;
                return a + delta * t;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="R">Major Radius</param>
        /// <param name="r">Minor Radius</param>
        /// <param name="uv1"></param>
        /// <param name="uv2"></param>
        /// <returns></returns>
        public static float DistanceSqr(float R, float r, Vector2 uv1, Vector2 uv2)
        {
            float du = Mathf.Repeat(uv2.x - uv1.x + Mathf.PI, 2f * Mathf.PI) - Mathf.PI;
            float dv = Mathf.Repeat(uv2.y - uv1.y + Mathf.PI, 2f * Mathf.PI) - Mathf.PI;

            float distU = Mathf.Abs(du) * R;
            float distV = Mathf.Abs(dv) * r;

            return distU * distU + distV * distV;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="R">Major Radius</param>
        /// <param name="r">Minor Radius</param>
        /// <param name="uv1"></param>
        /// <param name="uv2"></param>
        /// <returns></returns>
        public static float Distance(float R, float r, Vector2 uv1, Vector2 uv2) => Mathf.Sqrt(DistanceSqr(R, r, uv1, uv2));
        
        public static Vector2 GetOppositeUV(Vector2 uv)
        {
            float u2 = Mathf.Repeat(uv.x + Mathf.PI, 2f * Mathf.PI);
            float v2 = Mathf.Repeat(uv.y + Mathf.PI, 2f * Mathf.PI);
            return new Vector2(u2, v2);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="torusTransform"></param>
        /// <param name="R">Major Radius</param>
        /// <param name="r">Minor Radius</param>
        /// <param name="uv"></param>
        /// <returns></returns>
        public static Vector2 GetOppositeWorldPoint(Transform torusTransform, float R, float r, Vector2 uv)
        {
            var oppositeUV = GetOppositeUV(uv);
            return TorusUVToWorldPoint(torusTransform, R, r,oppositeUV.x, oppositeUV.y);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="torusTransform"></param>
        /// <param name="R">Major Radius</param>
        /// <param name="r">Minor Radius</param>
        /// <param name="u"></param>
        /// <returns></returns>
        public static Vector3 GetMajorCirclePosition(Transform torusTransform, float R, float r, float u)
        {
            float cosU = Mathf.Cos(u);
            float sinU = Mathf.Sin(u);
            Vector3 local = new Vector3(R * cosU, 0f, r * sinU);
            return LocalToWorldScaled(local, torusTransform);
        }
    }
}