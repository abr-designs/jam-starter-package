#if !JAM_INPUT_DELEGATOR
#define OLD_INPUT_SYSTEM
#endif

using System.Diagnostics;
using UnityEngine;
using Utilities;

#if JAM_INPUT_DELEGATOR
using GameInput;
using UnityEngine.InputSystem;
#endif


namespace Samples.CharacterController3D.Scripts
{
    [RequireComponent(typeof(CharacterController))]

    public class ThirdPersonCharacterController : MonoBehaviour, IThirdPersonCharacter
    {
        [Header("References")] [SerializeField]
        private Transform cameraTransform;

        [Header("Movement")] [SerializeField, Min(0f)]
        private float moveSpeed = 5f;

        [SerializeField, Min(0f)] private float rotationSpeed = 12f;
        [SerializeField] private bool rotateTowardsMovement = true;

        [Header("Slope & Grounding")] [SerializeField, Range(0f, 90f)]
        private float slopeLimit = 45f;

        [SerializeField, Min(0f)] private float groundStickForce = 2f;
        [SerializeField, Min(0f)] private float gravity = 25f;
        [SerializeField, Min(0f)] private float groundCheckDistance = 0.2f;

        private CharacterController m_characterController;
        private Vector2 m_moveInput;
        private float m_verticalVelocity;

        public Vector2 MoveInput => m_moveInput;
        public bool IsGrounded => m_characterController.isGrounded;
        // [Claude 2026-11-01 start]
        public Vector3 Velocity => m_characterController.velocity;
        public float MaxSpeed => moveSpeed;
        // [Claude 2026-11-01 end]

        //Unity Functions
        //================================================================================================================//

#if JAM_INPUT_DELEGATOR
    private void OnEnable()
    {
        GameInputDelegator.OnMovementChanged += SetMoveInput;
    }
#endif

        private void Start()
        {
            m_characterController = GetComponent<CharacterController>();
            m_characterController.slopeLimit = slopeLimit;
        }

        private void Update()
        {
            ProcessInputs(); //Only called if there is not Input Delegator
            UpdateMovement();
        }

#if JAM_INPUT_DELEGATOR
    private void OnDisable()
    {
        GameInputDelegator.OnMovementChanged -= SetMoveInput;
    }
#endif

        //ThirdPersonCharacterController Functions
        //================================================================================================================//

        public void SetCameraTransform(Transform cameraTransform)
        {
            this.cameraTransform = cameraTransform;
        }

        private void UpdateMovement()
        {
            Vector3 moveDirection = GetCameraRelativeDirection();

            if (rotateTowardsMovement && moveDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(moveDirection, Vector3.up);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            bool grounded = m_characterController.isGrounded
                            || IsNearGround();

            if (grounded && m_verticalVelocity < 0f)
            {
                m_verticalVelocity = -groundStickForce;
            }
            else
            {
                m_verticalVelocity -= gravity * Time.deltaTime;
            }

            Vector3 velocity = moveDirection * moveSpeed;
            velocity.y = m_verticalVelocity;

            m_characterController.Move(velocity * Time.deltaTime);
        }

        private Vector3 GetCameraRelativeDirection()
        {
            Vector3 forward;
            Vector3 right;

            if (cameraTransform != null)
            {
                forward = Vector3.ProjectOnPlane(
                    cameraTransform.forward, Vector3.up
                ).normalized;

                right = Vector3.ProjectOnPlane(
                    cameraTransform.right, Vector3.up
                ).normalized;
            }
            else
            {
                forward = Vector3.forward;
                right = Vector3.right;
            }

            Vector3 direction =
                right * m_moveInput.x + forward * m_moveInput.y;

            return Vector3.ClampMagnitude(direction, 1f);
        }

        private bool IsNearGround()
        {
            Vector3 origin = transform.TransformPoint(
                m_characterController.center
            );

            float radius = m_characterController.radius * 0.9f;
            float distance =
                m_characterController.height * 0.5f
                - m_characterController.radius
                + groundCheckDistance;

            return Physics.SphereCast(
                origin,
                radius,
                Vector3.down,
                out _,
                distance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore
            );
        }

        //Callbacks
        //================================================================================================================//

#if JAM_INPUT_DELEGATOR

    /// <summary>
    /// Sets movement input from an external input system.
    /// X = left/right, Y = forward/backward.
    /// </summary>
    public void SetMoveInput(Vector2 input)
    {
        m_moveInput = Vector2.ClampMagnitude(input, 1f);
    }

#endif

        [Conditional("OLD_INPUT_SYSTEM")]
        private void ProcessInputs()
        {
            InputHelper.AxisInput(KeyCode.W, KeyCode.S, ref m_moveInput.y);
            InputHelper.AxisInput(KeyCode.D, KeyCode.A, ref m_moveInput.x);

            //m_isJumpPressed = Input.GetKey(KeyCode.Space);
        }

        //Unity Editor Functions
        //================================================================================================================//

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_characterController == null)
                m_characterController = GetComponent<CharacterController>();

            if (m_characterController != null)
                m_characterController.slopeLimit = slopeLimit;
        }
#endif

        //================================================================================================================//

    }
}
