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
    public class CharacterAnimationController2 : MonoBehaviour
    {
        private static readonly int SpeedAnimationHash = Animator.StringToHash("Speed");

        [SerializeField]
        private Animator animator;

        private bool m_hasInput;
        private float m_animationSpeed, m_speedVelocity;

        //Unity Functions
        //============================================================================================================//

#if JAM_INPUT_DELEGATOR
        private void OnEnable()
        {
            GameInputDelegator.OnMovementChanged += OnMovementChanged;
        }
#endif
        
        // Update is called once per frame
        private void LateUpdate()
        {
            ProcessInputs();
            
            m_animationSpeed = Mathf.SmoothDamp(m_animationSpeed,
                m_hasInput ? 1f : 0f,
                ref m_speedVelocity, 
                0.25f);
            
            
            animator.SetFloat(SpeedAnimationHash, m_animationSpeed);
        }

#if JAM_INPUT_DELEGATOR
        private void OnDisable()
        {
            GameInputDelegator.OnMovementChanged -= OnMovementChanged;
        }
#endif

        //CharacterAnimationController Functions
        //============================================================================================================//

#if JAM_INPUT_DELEGATOR
        private void OnMovementChanged(Vector2 input)
        {
            m_hasInput = input != Vector2.zero;
        }
#endif

        private Vector2 m_moveInput;
        [Conditional("OLD_INPUT_SYSTEM")]
        private void ProcessInputs()
        {
            InputHelper.AxisInput(KeyCode.W, KeyCode.S, ref m_moveInput.y);
            InputHelper.AxisInput(KeyCode.D, KeyCode.A, ref m_moveInput.x);
            
            m_hasInput = m_moveInput.sqrMagnitude != 0;

            //m_isJumpPressed = Input.GetKey(KeyCode.Space);
        }
        
        
    }
}