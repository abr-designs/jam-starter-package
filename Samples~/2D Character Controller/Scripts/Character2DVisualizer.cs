using GameInput;
using UnityEngine;

namespace Samples.CharacterController2D.Scripts
{
    public class Character2DVisualizer : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        //Unity Functions
        //============================================================================================================//

#if JAM_INPUT_DELEGATOR
        private void OnEnable()
        {
            GameInputDelegator.OnMovementChanged += OnMovementChanged;
        }

        private void OnDisable()
        {
            GameInputDelegator.OnMovementChanged -= OnMovementChanged;
        }

        
        //============================================================================================================//
        
        private void OnMovementChanged(Vector2 movementInput)
        {
            if (movementInput.x == 0f)
                return;
            
            spriteRenderer.flipX = movementInput.x < 0;
        }
#else

        private void LateUpdate()
        {
            var inputX = 0f;
            Utilities.InputHelper.AxisInput(KeyCode.D, KeyCode.A, ref inputX);
            
            if (inputX == 0f)
                return;
            
            spriteRenderer.flipX = inputX < 0;
        }

#endif
    }
}