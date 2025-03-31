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
    }
}