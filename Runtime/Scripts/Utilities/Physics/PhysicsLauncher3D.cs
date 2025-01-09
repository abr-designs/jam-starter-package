using System;
using UnityEngine;
using Utilities.Debugging;
using Random = UnityEngine.Random;

namespace Utilities.Physics
{
    [Serializable]
    public struct PhysicsLauncher3D
    {
        public Vector3 SpawnLocation => spawnLocation;
        [SerializeField] private Vector3 spawnLocation;
        [SerializeField] private Vector3 spawnDirection;
        [SerializeField, Min(0f)] private float spawnAngle;
        [SerializeField] private Vector2 spawnForceRange;

        public Vector3 GetLaunchVelocity()
        {
            var dir = Quaternion.Euler(
                Random.Range(-spawnAngle, spawnAngle), 
                Random.Range(-spawnAngle, spawnAngle), 
                Random.Range(-spawnAngle, spawnAngle)) * spawnDirection.normalized;
            
            var force = Random.Range(spawnForceRange.x, spawnForceRange.y);

            return dir * force;
        }

#if UNITY_EDITOR

        public void DrawGizmos()
        {
            const int SEGMENT_COUNT = 4;
            const int CONE_LINE_COUNT = 8;
            
            const float SEGMENT_LENGTH = 1f / SEGMENT_COUNT;
            const float CONE_LINE_DEGREES = 360f / CONE_LINE_COUNT;
            
            //Calculator: https://www.calculator.net/right-triangle-calculator.html
            float GetRadius(float angle, float length)
            {
                var cLength = length / Mathf.Cos(angle * Mathf.Deg2Rad);
                return Mathf.Sqrt((cLength * cLength) - (length * length));
            }

            //Draw Launch Direction
            //------------------------------------------------//
            var dirNormalized = spawnDirection.normalized;
            Draw.Arrow(spawnLocation, spawnDirection.normalized, Color.green);

            //Draw Circles to show launch area
            //------------------------------------------------//
            for (int i = 0; i < SEGMENT_COUNT; i++)
            {
                var temp = SEGMENT_LENGTH * (i + 1);
                var offset = temp * dirNormalized;
                var radius = GetRadius(spawnAngle, temp);
                
                Draw.Circle(spawnLocation + offset, dirNormalized, Color.red, radius);
            }

            //Draw Launch cone lines
            //------------------------------------------------//
            var rot = Quaternion.FromToRotation(Vector3.forward, dirNormalized);
            var r = GetRadius(spawnAngle, 1f);
            for (int i = 0; i < CONE_LINE_COUNT; i++)
            {
                var deg = i * CONE_LINE_DEGREES;
                var position = (rot * JMath.GetAsPointOnCircle(deg, r));
                Debug.DrawLine(spawnLocation, spawnLocation + dirNormalized + position, Color.yellow);

            }
        }

#endif
    }
}