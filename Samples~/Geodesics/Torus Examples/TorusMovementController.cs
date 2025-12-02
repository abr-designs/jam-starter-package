using System;
using UnityEngine;
using Utilities.Geodesics;

namespace Samples.Geodesics.Sphere
{
    public class TorusMovementController : MonoBehaviour
    {
        [SerializeField] 
        private Transform playerTransform;

        [Header("Torus")] 
        [SerializeField] 
        private Torus torus;

        [Header("Movement")] 
        [SerializeField] 
        private float moveSpeed;
        [SerializeField] 
        private float turnSpeed;

        [SerializeField] 
        private float verticalOffset;

        private float m_currentXInput;
        private float m_currentYInput;

        private Vector3 lastUp = Vector3.up;

        protected float U, V;
        protected float Du, Dv;

        //Unity Functions
        //============================================================================================================//
        protected void Start()
        {
            TorusMaths.WorldPointToTorusUV(torus, playerTransform.position, out U, out V);
        }

        // Update is called once per frame
        public void Update()
        {
            InputProcessor();

            float localRadiusU = torus.majorRadius + torus.minorRadius * Mathf.Cos(V); // radius at this v
            float scaleU = localRadiusU;
            float scaleV = torus.minorRadius; // always the same

            //FIXME This might need to be separated to keep consistent directions
            //------------------------------------------------//
            
            Vector3 playerForward = Vector3.ProjectOnPlane(playerTransform.forward, lastUp);
            //Vector3 playerRight = Vector3.ProjectOnPlane(playerTransform.right, lastUp);

            Vector3 moveInput = (playerForward * m_currentYInput).normalized;

            //------------------------------------------------//

            Vector3 tangentU = TorusMaths.GetTorusTangent(torus, U, V, true);
            Vector3 tangentV = TorusMaths.GetTorusTangent(torus, U, V, false);

            // Project camera-relative movement into torus tangent space
            Du = Vector3.Dot(moveInput, tangentU);
            Dv = Vector3.Dot(moveInput, tangentV);
            
            float angleSpeed = moveSpeed * Time.deltaTime;

            // normalize speed by dividing by arc length radius
            U = (U + Du * angleSpeed / scaleU + 2 * Mathf.PI) % (2 * Mathf.PI);
            V = (V + Dv * angleSpeed / scaleV + 2 * Mathf.PI) % (2 * Mathf.PI);

            Vector3 pos = TorusMaths.TorusUVToWorldPoint(torus, U, V);
            //Vector3 forward = TorusMaths.GetTorusTangent(torus, U, V, true); // du/dt
            Vector3 up = TorusMaths.GetTorusNormal(torus, U, V);

            playerTransform.position = pos;

            // If the player's up is pointing opposite to the surface up, invert horizontal input.
            float yawDegrees = m_currentXInput * turnSpeed * Time.deltaTime;

            // Apply yaw around the stable surfaceUp
            Quaternion yaw = Quaternion.AngleAxis(yawDegrees, -up);

            // Align player to surface, then apply yaw in surface space
            Quaternion surfaceAlign = Quaternion.FromToRotation(playerTransform.up, -up);
            playerTransform.rotation = surfaceAlign * yaw * playerTransform.rotation;

            lastUp = -up;
        }

        //============================================================================================================//

        private void InputProcessor()
        {
            Input.AxisInput(KeyCode.D, KeyCode.A, ref m_currentXInput);
            Input.AxisInput(KeyCode.W, KeyCode.S, ref m_currentYInput);
        }
    }
}