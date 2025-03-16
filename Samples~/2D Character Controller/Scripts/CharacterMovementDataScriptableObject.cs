using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NaughtyAttributes;
using UnityEngine;

namespace Samples.CharacterController2D.Scripts
{
    [CreateAssetMenu(fileName = "2D Character Movement Data", menuName = "Character Controller/2D Movement Data")]
    public class CharacterMovementDataScriptableObject : ScriptableObject
    {
        public float Gravity { get; private set; }
        public float InitialJumpVelocity { get; private set; }
        
        //Walk
        //------------------------------------------------//
        [Header("Walk")] [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;
        [Range(0.25f, 50f)] public float GroundAcceleration = 5f;
        [Range(0.25f, 50f)] public float GroundDeceleration = 20f;
        [Range(0f, 50f)] public float AirAcceleration = 5f;
        [Range(0.25f, 50f)] public float AirDeceleration = 5f;

        //Run
        //------------------------------------------------//
        [Header("Run")] [Range(1f, 100f)] 
        public float MaxRunSpeed = 20f;

        //Grounded/Collision Checks
        //------------------------------------------------//
        [Header("Grounded/Collision Checks")] 
        public LayerMask GroundLayer;
        public float GroundDetectionRayLength = 0.02f;
        public float HeadDetectionRayLength = 0.02f;
        [Range(0f, 1f)] public float HeadWidth = 0.75f;

        //Jump
        //------------------------------------------------//
        [Header("Jump"), Range(1.0f, 1.1f)] 
        public float JumpHeightCompensationFactor = 1.054f;
        public float MaxVerticalVelocity = 50f;
        public float JumpHeight = 6.5f;
        public float TimeTillJumpApex = 0.35f;
        [Range(0.1f, 5f)]
        public float GravityOnReleaseMultiplier = 2f;
        public float MaxFallSpeed = 26f;
        [Range(0, 10)]
        public int NumberOfJumpsAllowed = 2;

        //Jump Cut
        //------------------------------------------------//
        [Header("Jump Cut"), Range(0f, 1f)] 
        public float TimeForUpwardsCancel = 0.027f;

        //Jump Apex
        //------------------------------------------------//
        [Header("Jump Apex"), Range(0f, 1f)]
        public float ApexThreshold;
        public float ApexHangTime = 0.075f;

        //Jump Buffer
        //------------------------------------------------//
        [Header("Jump Buffer"), Range(0f, 1f)] 
        public float JumpBufferTime = 0.125f;

        //Jump Coyote Time
        //------------------------------------------------//
        [Header("Jump Coyote Time"), Range(0f, 1f)]
        public float JumpCoyoteTime = 0.1f;

        //Wall Checks
        //------------------------------------------------//
        [Header("Wall Collision Checks"), Range(0f, 1f)] 
        public float WallDetectionRayLength = 0.02f;

        //Misc
        //------------------------------------------------//
        [Header("Misc"), Range(0f, 1f)] 
        public float JumpHorizontalDampening = .5f;

        //Debug Drawing
        //------------------------------------------------//
        [SerializeField, Header("Debug Drawing"), EnumFlags] 
        private DEBUG_DRAW DebugDraw;
        [Header("Jump Draw"), Range(3, 100)]
        public int ArcResolution = 20;
        [Range(3, 500)]
        public int VisualizationSteps = 90;

        //Functions
        //============================================================================================================//

         private void OnEnable()
         {
             CalculateValues();
         }

         private void CalculateValues()
         {
             var adjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
             Gravity = -(2f * adjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
             InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
         }
         
         [Conditional("UNITY_EDITOR")]
         private void OnValidate()
         {
             CalculateValues();
         }


         //============================================================================================================//

#if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDraw(DEBUG_DRAW toDraw)
        {
            return DebugDraw.HasFlag(toDraw);
        }
#endif
    }
    
    [Flags]
    public enum DEBUG_DRAW
    {
        NONE = 0,
        GROUND_CHECK = 1 << 0,
        HEAD_CHECK = 1 << 1,
        WALL_CHECK = 1 << 2,
        JUMP_ARC = 1 << 3,
        JUMP_RUN_ARC = 1 << 4,
        JUMP_WITH_COLLISION = 1 << 5,
        FACE_RIGHT = 1 << 6,
             
    }
}
