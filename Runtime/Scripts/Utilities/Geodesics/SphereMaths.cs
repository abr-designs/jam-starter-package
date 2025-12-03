using UnityEngine;

namespace Utilities.Geodesics
{
    public static class SphereMaths
    {
        /// <summary>
        /// Static helper that computes a new position moved along the sphere surface.
        /// Corrected rotation-axis direction so 'direction' moves forward.
        /// </summary>
        /// <param name="currentPos">Current world position.</param>
        /// <param name="sphereCenter">Sphere center world pos.</param>
        /// <param name="radius">Sphere radius (without offset).</param>
        /// <param name="direction">Desired forward direction (will be projected onto tangent).</param>
        /// <param name="speed">Linear speed along surface units/sec.</param>
        /// <param name="deltaTime">Time step (usually Time.deltaTime).</param>
        /// <param name="surfaceOffset">Offset above sphere surface.</param>
        /// <returns>New world position.</returns>
        public static Vector3 MoveAlongSphere(Vector3 currentPos, Vector3 sphereCenter, float radius, Vector3 direction, float speed, float deltaTime, float surfaceOffset = 0f)
        {
            Vector3 fromCenter = currentPos - sphereCenter;
            float currentRadius = radius + surfaceOffset;

            // If caller passed a zero vector position, bail
            if (fromCenter.sqrMagnitude < 1e-6f)
                return currentPos;

            // Project direction onto tangent plane
            Vector3 tangent = Vector3.ProjectOnPlane(direction, fromCenter).normalized;
            if (tangent.sqrMagnitude < 1e-6f)
                return currentPos; // no valid movement direction

            // Angular displacement (radians)
            float angularSpeed = speed / currentRadius; // radians/sec
            float angle = angularSpeed * deltaTime; // radians

            // Correct rotation axis: r x t (so small rotation moves along +t)
            Vector3 rotationAxis = Vector3.Cross(fromCenter, tangent).normalized;
            if (rotationAxis.sqrMagnitude < 1e-6f)
                return currentPos;

            // Quaternion wants degrees
            Quaternion rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * angle, rotationAxis);

            Vector3 newFromCenter = rotation * fromCenter;
            Vector3 newPos = sphereCenter + newFromCenter.normalized * currentRadius;
            return newPos;
        }
    }
}