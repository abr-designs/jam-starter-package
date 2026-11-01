using UnityEngine;

namespace Samples.CharacterController3D.Scripts
{
    /// <summary>
    /// Drives the Animator Speed & Grounded parameters from the <see cref="IThirdPersonCharacter"/> on this
    /// object or a parent, so any controller implementing the interface can share it.
    /// </summary>
    /// <remarks>Rewritten by Claude (claude-opus-5-5) — 2026-11-01</remarks>
    public class CharacterAnimationController : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("Grounded");

        [SerializeField]
        private Animator animator;

        [SerializeField, Min(0f), Tooltip("Seconds to ease the Speed parameter. Useful for controllers that change velocity instantly.")]
        private float speedSmoothTime = 0.25f;

        private IThirdPersonCharacter m_character;
        private float m_animationSpeed, m_speedVelocity;

        //Unity Functions
        //============================================================================================================//

        private void Start()
        {
            m_character = GetComponentInParent<IThirdPersonCharacter>();
        }

        private void Update()
        {
            var isGrounded = m_character.IsGrounded;
            animator.SetBool(IsGroundedHash, isGrounded);

            //If we're in the air, we don't need to worry about setting the speed
            if (!isGrounded)
                return;

            m_animationSpeed = Mathf.SmoothDamp(m_animationSpeed, GetNormalizedSpeed(), ref m_speedVelocity, speedSmoothTime);
            animator.SetFloat(SpeedHash, m_animationSpeed);
        }

        //CharacterAnimationController Functions
        //============================================================================================================//

        private float GetNormalizedSpeed()
        {
            var velocity = m_character.Velocity;
            velocity.y = 0;
            return velocity.magnitude / m_character.MaxSpeed;
        }
    }
}
