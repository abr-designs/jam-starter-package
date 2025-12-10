using UnityEngine;
using Utilities;

namespace Samples.Geodesics.Sphere
{
    public class CharacterAnimationController : MonoBehaviour
    {
        private static readonly int SpeedAnimatorHash = Animator.StringToHash("Speed");

        [SerializeField]
        private Animator animator;

        private float m_currentXInput;
        private float m_currentYInput;

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
            InputHelper.AxisInput(KeyCode.D, KeyCode.A, ref m_currentXInput);
            InputHelper.AxisInput(KeyCode.W, KeyCode.S, ref m_currentYInput);
        }
    }
}