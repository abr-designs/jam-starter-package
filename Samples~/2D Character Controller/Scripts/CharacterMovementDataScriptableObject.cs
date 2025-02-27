using UnityEngine;

[CreateAssetMenu(fileName = "2D Character Movement Data", menuName = "Character Controller/2D Movement Data")]
public class CharacterMovementDataScriptableObject : ScriptableObject
{

    [Header("Walk")] [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;
    [Range(0.25f, 50f)] public float GroundAcceleration = 5f;
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;
    [Range(0f, 50f)] public float AirAcceleration = 5f;
    [Range(0.25f, 50f)] public float AirDeceleration = 5f;

    [Header("Run")] [Range(1f, 100f)] public float MaxRunSpeed = 20f;

    [Header("Grounded/Collision Checks")] public LayerMask GroundLayer;
    public float GroundDetectionRayLength = 0.02f;
    public float HeadDetectionRayLength = 0.02f;
    [Range(0f, 1f)] public float HeadWidth = 0.75f;

    [Header("Jump")] public float JumpVelocity = 20f;

    // public float JumpHeight = 6.5f;
    // public float TimeTillJumpApex = 0.35f;
    [Range(0f, 1f)] public float JumpHorizontalDampening = .5f;

    [Range(0.01f, 20f)] public float FallGravityMultiplier = 2f;
    public float MaxFallSpeed = 26f;

    [Header("Jump Buffer")] [Range(0f, 1f)]
    public float JumpBufferTime = 0.125f;

    [Header("Jump Coyote Time")] [Range(0f, 1f)]
    public float JumpCoyoteTime = 0.1f;


    // public float Gravity { get; private set; }
    // public float InitialJumpVelocity { get; private set; }

    // private void OnValidate()
    // {
    //     CalculateValues();
    // }

    // private void OnEnable()
    // {
    //     CalculateValues();
    // }

    // private void CalculateValues()
    // {
    //     Gravity = -(2f * JumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
    //     InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
    // }


}
