using System;
using UnityEngine;
using Utilities.Geodesics;

namespace Samples.Geodesics.Sphere
{
    public class SphereMovementController : MonoBehaviour
    {
        [Header("Sphere")]
        [SerializeField]
        private Transform sphereTransform;
        [SerializeField] 
        private float planetRadius;

        [Header("Movement ")] 
        [SerializeField] 
        private Transform playerTransform;
        [SerializeField] 
        private float moveSpeed = 3f;

        //Degrees per Second
        [SerializeField, Tooltip("Degrees per second")]
        private float rotationSpeed = 90f;

        [Tooltip("Distance above the sphere surface (positive = above).")]
        public float surfaceOffset;

        private float m_currentXInput;
        private float m_currentYInput;

        // Internal tangent direction (unit, tangent to sphere)
        [Header("Initial Heading")]
        [Tooltip("Initial desired forward direction. Will be projected to tangent at start.")]
        private Vector3 m_tangentDir;

        //Unity Functions
        //============================================================================================================//

        public void Start()
        {
            SetupInitialMovementValues();
        }
        
        private void Update()
        {
            InputProcessor();
            
            float dt = Time.deltaTime;

            // Steering input
            float steerInput = m_currentXInput;

            if (Math.Abs(steerInput) > 1e-5f)
                Steer(steerInput * rotationSpeed * dt);

            var speedWithMult = moveSpeed * m_currentYInput;

            // Move forward based on tangentDir
            Vector3 newPos = SphereSurfaceMover.MoveAlongSphere(playerTransform.position, 
                sphereTransform.position,
                planetRadius, 
                m_tangentDir, 
                speedWithMult, 
                dt, 
                surfaceOffset);

            // Reproject tangentDir to new tangent plane and keep it normalized
            Vector3 newNormal = (newPos - sphereTransform.position).normalized;
            m_tangentDir = Vector3.ProjectOnPlane(m_tangentDir, newNormal).normalized;
            if (m_tangentDir.sqrMagnitude < 1e-6f)
            {
                // fallback
                m_tangentDir = Vector3.Cross(newNormal, Vector3.up);
                if (m_tangentDir.sqrMagnitude < 1e-6f)
                    m_tangentDir = Vector3.Cross(newNormal, Vector3.right);
                m_tangentDir.Normalize();
            }

            playerTransform.SetPositionAndRotation(
                newPos,
                Quaternion.LookRotation(m_tangentDir, newNormal)
                );
        }
        
        //============================================================================================================//
        
        private void SetupInitialMovementValues()
        {
            // Ensure we start at the correct offset from the sphere
            Vector3 toCenter = (playerTransform.position - sphereTransform.position);
            if (toCenter.sqrMagnitude < 1e-6f)
            {
                // If object is exactly at center, push it out along world up
                toCenter = Vector3.up * (planetRadius + surfaceOffset);
                playerTransform.position = sphereTransform.position + toCenter;
            }
            else
            {
                float desiredRadius = planetRadius + surfaceOffset;
                playerTransform.position = sphereTransform.position + toCenter.normalized * desiredRadius;
            }

            // Initialize tangent direction by projecting initialDirection onto tangent plane
            Vector3 normal = (playerTransform.position - sphereTransform.position).normalized;
            m_tangentDir = Vector3.ProjectOnPlane(playerTransform.forward.normalized, normal).normalized;

            // If initialDirection was parallel to normal, pick a default tangent direction
            if (m_tangentDir.sqrMagnitude < 1e-6f)
            {
                // choose a tangent perpendicular to the normal (arbitrary stable choice)
                m_tangentDir = Vector3.Cross(normal, Vector3.up);
                if (m_tangentDir.sqrMagnitude < 1e-6f)
                    m_tangentDir = Vector3.Cross(normal, Vector3.right);
                m_tangentDir.Normalize();
            }

            // Ensure forward of the object matches tangent (optional)
            playerTransform.rotation = Quaternion.LookRotation(m_tangentDir, normal);
        }

        //============================================================================================================//

        private void InputProcessor()
        {
            AxisInput(KeyCode.D, KeyCode.A, ref m_currentXInput);
            AxisInput(KeyCode.W, KeyCode.S, ref m_currentYInput);
            return;

            void AxisInput(KeyCode positive, KeyCode negative, ref float value)
            {
                if (Input.GetKey(positive) || Input.GetKey(negative))
                {
                    if(Input.GetKeyDown(positive))
                        value = 1f;
                    if (Input.GetKeyDown(negative))
                        value = -1f;
                }
                else
                {
                    value = 0f;
                }
            }
        }

        //============================================================================================================//

        /// <summary>
        /// Rotate the current heading (tangentDir) around the local normal (sphere normal) by degrees.
        /// Positive degrees rotate to the right when looking down the normal (right-hand rule).
        /// Call this to steer programmatically.
        /// </summary>
        /// <param name="degrees">Degrees to rotate this frame (can be negative).</param>
        private void Steer(float degrees)
        {
            Vector3 normal = (playerTransform.position - sphereTransform.position).normalized;
            Quaternion turn = Quaternion.AngleAxis(degrees, normal);
            m_tangentDir = (turn * m_tangentDir).normalized;
        }

        //============================================================================================================//

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;
            Gizmos.DrawWireSphere(sphereTransform.position, planetRadius);
        }
#endif
    }
}