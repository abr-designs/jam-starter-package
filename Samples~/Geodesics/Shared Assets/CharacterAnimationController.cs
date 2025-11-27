using System;
using UnityEngine;

namespace Samples.Geodesics.Sphere
{
    public class CharacterAnimationController : MonoBehaviour
    {
        private static readonly int SpeedAnimatorHash = Animator.StringToHash("Speed");

        [SerializeField]
        private Animator animator;

        private float m_currentXInput;
        private float m_currentYInput;

        private void Start()
        {
            
        }

        private void Update()
        {
            InputProcessor();
            
            if (m_currentYInput != 0f)
                animator.SetFloat(SpeedAnimatorHash, 1f);
            else if (m_currentXInput != 0f)
                animator.SetFloat(SpeedAnimatorHash, 0.25f);
            else
                animator.SetFloat(SpeedAnimatorHash, 0f);
        }

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
    }
}