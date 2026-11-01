#if !JAM_INPUT_DELEGATOR
#define OLD_INPUT_SYSTEM
#endif

using UnityEngine;

#if JAM_INPUT_DELEGATOR
using GameInput;
using UnityEngine.InputSystem;
#endif

namespace Samples.CharacterController3D.Scripts
{
    [DefaultExecutionOrder(-100)]
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")] [SerializeField] private Transform target;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.5f, 0f);
        [SerializeField] private float shoulderOffset = 0.5f;

        [Header("Camera")] [SerializeField] private float distance = 4f;
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 70f;


        [Header("Smoothing")]
        [SerializeField]
        private float rotationSmoothTime = 0.05f;

        [SerializeField] private float positionSmoothTime = 0.08f;

        private float m_yaw;
        private float m_pitch;
        private float m_targetYaw;
        private float m_targetPitch;
        private float m_yawVelocity;
        private float m_pitchVelocity;
        private Vector3 m_positionVelocity;
        private bool m_enabledLookAction;

        //Unity Functions
        //================================================================================================================//

        private void Start()
        {
            Vector3 angles = transform.eulerAngles;

            m_yaw = m_targetYaw = angles.y;
            m_pitch = m_targetPitch = angles.x;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            var mouseDelta = GetMouseDelta();
            // Read mouse delta.
            m_targetYaw += mouseDelta.x * mouseSensitivity;
            m_targetPitch -= mouseDelta.y * mouseSensitivity;
            m_targetPitch = Mathf.Clamp(m_targetPitch, minPitch, maxPitch);

            // Smooth camera rotation.
            m_yaw = Mathf.SmoothDampAngle(
                m_yaw, m_targetYaw, ref m_yawVelocity, rotationSmoothTime);

            m_pitch = Mathf.SmoothDampAngle(
                m_pitch, m_targetPitch, ref m_pitchVelocity, rotationSmoothTime);

            Quaternion rotation = Quaternion.Euler(m_pitch, m_yaw, 0f);

            // Calculate target pivot in world space.
            Vector3 pivot = target.TransformPoint(localOffset);

            // Position the camera behind the pivot and over the shoulder.
            Vector3 desiredPosition =
                pivot
                - rotation * Vector3.forward * distance
                + rotation * Vector3.right * shoulderOffset;

            // Smooth camera position and rotation.
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref m_positionVelocity,
                positionSmoothTime);

            transform.rotation = rotation;
        }
        
        //Public API
        //================================================================================================================//

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        //Callbacks
        //================================================================================================================//
        private static Vector2 GetMouseDelta()
        {
#if OLD_INPUT_SYSTEM
            Vector2 pixelDelta = Input.mousePositionDelta;
#else
            if (GameInputDelegator.LockInputs)
                return Vector2.zero;

            var pointer = Pointer.current;
            var pixelDelta = pointer == null ? Vector2.zero : pointer.delta.ReadValue();
#endif
            return pixelDelta / Screen.height;
        }
    }
}