using System;
using GameInput;
using UnityEngine;
using Utilities.Debugging;

namespace Samples.CharacterController2D.Scripts
{
    // Based on the controller from Sasquatch Studios
    // https://www.youtube.com/watch?v=zHSWG05byEc
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class CharacterController2D : MonoBehaviour
    {
        [SerializeField, Header("Data")] 
        private CharacterMovementDataScriptableObject characterMovementData;
        
        [Header("Colliders")] 
        [SerializeField] 
        private Collider2D feetCollider;
        [SerializeField] 
        private Collider2D bodyCollider;
        
        //------------------------------------------------//

        private Rigidbody2D m_rigidbody2D;

        // move vars (horizontal input)
        private Vector2 m_moveVelocity;
        private bool m_isFacingRight;

        // collision check
        private bool LeftSideHeadHit => m_wallHitChecks[0];
        private bool LeftSideBodyHit => m_wallHitChecks[1];
        private bool LeftSideFeetHit => m_wallHitChecks[2];
        
        private bool RightSideHeadHit => m_wallHitChecks[3];
        private bool RightSideBodyHit => m_wallHitChecks[4];
        private bool RightSideFeetHit => m_wallHitChecks[5];
        

        private readonly bool[] m_wallHitChecks = new bool[6];
        
        private bool m_isOnWall;
        private Vector2 m_wallNormal;

        public bool IsGrounded => m_isGrounded;
        private bool m_isGrounded;
        private bool m_bumpedHead;

        // jump vars
        public float VerticalVelocity { get; private set; }
        private bool m_isJumping;
        private bool m_isJumpAvailable;
        private bool m_isFalling;
        private bool m_isFastFalling;
        private float m_fastFallTime;
        private float m_fastFallReleaseSpeed;
        private int m_numberOfJumpsUsed;
        private bool m_isPastApexThreshold;

        // jump buffer vars
        private float m_jumpBufferTimer;
        private bool m_jumpReleasedDuringBuffer;

        // coyote time vars
        private float m_coyoteTimer;

        // input
        private Vector2 m_lastFrameInput = Vector2.zero;
        private Vector2 m_moveInput;
        private bool m_lastFrameJumpPressed;
        private bool m_isJumpPressed;
        private bool m_isRunPressed;

        // external force
        private Vector2 m_externalVel = Vector2.zero;
        
        private float m_apexPoint;
        private float m_timePastApexThreshold;
        
        private readonly RaycastHit2D[] m_nonAllocRaycastHit2Ds = new RaycastHit2D[1];

        //Unity Functions
        //============================================================================================================//

        #region Unity Functions

        private void Awake()
        {
            m_isFacingRight = true;
            m_isGrounded = false;
        }

        private void OnEnable()
        {
            GameInputDelegator.OnMovementChanged += OnMovementChanged;
            GameInputDelegator.OnJumpPressed += OnJumpPressed;
        }

        private void Start()
        {
            m_rigidbody2D = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        private void Update()
        {
            CountTimers();
            JumpInputChecks();

            m_lastFrameInput = m_moveInput;
            m_lastFrameJumpPressed = m_isJumpPressed;
        }

        private void FixedUpdate()
        {
            //Collision Checks
            //------------------------------------------------//
            GroundCheck();
            BumpHeadCheck();
            WallCheck();
            //------------------------------------------------//
            
            Jump();

            // Use air/ground acceleration values for horizontal velocity
            if (m_isGrounded)
            {
                Move(characterMovementData.GroundAcceleration, characterMovementData.GroundDeceleration, m_moveInput);
            }
            else
            {
                Move(characterMovementData.AirAcceleration, characterMovementData.AirDeceleration, m_moveInput);
            }

        }

        private void OnDisable()
        {
            GameInputDelegator.OnMovementChanged -= OnMovementChanged;
            GameInputDelegator.OnJumpPressed -= OnJumpPressed;
        }

        #endregion //Unity Functions

        //Movement
        //============================================================================================================//

        #region Movement

        // Horizontal movement
        private void Move(float acceleration, float deceleration, Vector2 moveInput)
        {
            // Apply any external velocity this frame
            if (Math.Abs(m_externalVel.x) > 0f)
            {
                m_moveVelocity.x = m_externalVel.x;
                m_externalVel = new Vector2(0f, m_externalVel.y);
            }

            //------------------------------------------------//
            if (moveInput != Vector2.zero)
            {
                TurnCheck(moveInput);
                var targetVelocity = Vector2.right * (moveInput.x * (m_isRunPressed ? characterMovementData.MaxRunSpeed : characterMovementData.MaxWalkSpeed));

                m_moveVelocity = Vector2.Lerp(m_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
                m_rigidbody2D.linearVelocity = new Vector2(m_moveVelocity.x, m_rigidbody2D.linearVelocity.y);
            }
            else if (moveInput == Vector2.zero)
            {
                m_moveVelocity = Vector2.Lerp(m_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
                m_rigidbody2D.linearVelocity = new Vector2(m_moveVelocity.x, m_rigidbody2D.linearVelocity.y);
            }
            //------------------------------------------------//
            
            // Apply correction for being against a wall (we need to immediately stop the velocity)
            bool leftWall = LeftSideHeadHit || LeftSideBodyHit || LeftSideHeadHit;
            bool rightWall = RightSideHeadHit || RightSideBodyHit || RightSideFeetHit;
            if (rightWall && m_rigidbody2D.linearVelocity.x > 0f)
            {
                m_rigidbody2D.linearVelocity = new Vector2(0f, m_rigidbody2D.linearVelocity.y);
            }

            if (leftWall && m_rigidbody2D.linearVelocity.x < 0f)
            {
                m_rigidbody2D.linearVelocity = new Vector2(0f, m_rigidbody2D.linearVelocity.y);
            }

            // If character has a body above a ledge but feet below we shimmy them up
            // TODO -- this is where ledge grab would kick in
            if (!LeftSideBodyHit && LeftSideFeetHit && m_moveInput.x < 0f)
            {
                m_rigidbody2D.position += Vector2.up * 0.1f;
            }

            if (!RightSideBodyHit && RightSideFeetHit && m_moveInput.x > 0f)
            {
                m_rigidbody2D.position += Vector2.up * 0.1f;
            }

        }

        private void TurnCheck(Vector2 moveInput)
        {
            if (m_isFacingRight && moveInput.x < 0)
                m_isFacingRight = false;
            else if (!m_isFacingRight && moveInput.x > 0)
                m_isFacingRight = true;
        }

        #endregion

        //Collision Checks
        //============================================================================================================//

        #region Collision Checks

        private void GroundCheck()
        {
            Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
            Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x, characterMovementData.GroundDetectionRayLength);

            var contactFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = characterMovementData.GroundLayer,
            };

            var hitCount = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, contactFilter, m_nonAllocRaycastHit2Ds, characterMovementData.GroundDetectionRayLength);
            m_isGrounded = hitCount > 0;

            #region Ground Check Debug

#if UNITY_EDITOR
            if (characterMovementData.CanDraw(DEBUG_DRAW.GROUND_CHECK))
            {
                var rayColor = m_isGrounded ? Color.green : Color.red;

                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * characterMovementData.GroundDetectionRayLength, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * characterMovementData.GroundDetectionRayLength, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - characterMovementData.GroundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
            }
#endif

            #endregion //Ground Check Debug
        }

        private void BumpHeadCheck()
        {
            var boxCastOrigin = new Vector2(feetCollider.bounds.center.x, bodyCollider.bounds.max.y);
            var boxCastSize   = new Vector2(characterMovementData.HeadWidth, characterMovementData.HeadDetectionRayLength);
            
            var contactFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = characterMovementData.GroundLayer,
            };

            var hitCount = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, contactFilter, m_nonAllocRaycastHit2Ds, characterMovementData.HeadDetectionRayLength);
            m_bumpedHead = hitCount > 0;

            #region HeadHitDebug

#if UNITY_EDITOR
            if (characterMovementData.CanDraw(DEBUG_DRAW.HEAD_CHECK))
            {
                float headWidth = characterMovementData.HeadWidth;
                var rayColor = m_bumpedHead ? Color.green : Color.red;

                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.up * characterMovementData.HeadDetectionRayLength, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2), boxCastOrigin.y), Vector2.up * characterMovementData.HeadDetectionRayLength, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y + characterMovementData.HeadDetectionRayLength), Vector2.right * headWidth, rayColor);
            }
#endif

            #endregion
        }

        private void WallCheck()
        {
            Span<Vector2> checkPoints = stackalloc Vector2[6]
            {
                /*Left headPoint*/new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.max.y),
                /*Left bodyPoint*/new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.center.y),
                /*Left feetPoint*/new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y),
                /*Right headPoint*/new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.max.y),
                /*Right bodyPoint*/new Vector2(bodyCollider.bounds.center.x, bodyCollider.bounds.center.y),
                /*Right feetPoint*/new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y),
            };
            Span<Vector2> directions = stackalloc Vector2[6]
            {
                Vector2.left,
                Vector2.left,
                Vector2.left,
                Vector2.right,
                Vector2.right,
                Vector2.right,
            };
            var castDistance = bodyCollider.bounds.extents.x + characterMovementData.WallDetectionRayLength;

            for (int i = 0; i < checkPoints.Length; i++)
            {
                m_wallHitChecks[i] = Physics2D.RaycastNonAlloc(checkPoints[i], directions[i], m_nonAllocRaycastHit2Ds, castDistance, characterMovementData.GroundLayer) > 0;
            }

            #region Wallcheck Debug

#if UNITY_EDITOR
            if(characterMovementData.CanDraw(DEBUG_DRAW.WALL_CHECK))
            {

                for (int i = 0; i < checkPoints.Length; i++)
                {
                    Debug.DrawRay(checkPoints[i], directions[i] * castDistance, m_wallHitChecks[i] ? Color.green : Color.red);
                }
            }
#endif

            #endregion //Wallcheck Debug
        }

        #endregion

        //Jump
        //============================================================================================================//

        #region Jump

        // Process vertical velocity
        private void JumpInputChecks()
        {
            // Player pressed jump button this frame -- start the jump buffer
            bool jumpPressedThisFrame = !m_lastFrameJumpPressed && m_isJumpPressed;
            bool jumpReleasedThisFrame = m_lastFrameJumpPressed && !m_isJumpPressed;
            
            // Jumping starts the buffer timer -- hitting ground within this timer will trigger a jump
            if (jumpPressedThisFrame)
            {
                m_jumpBufferTimer = characterMovementData.JumpBufferTime;
                m_jumpReleasedDuringBuffer = false;
            }

            if (jumpReleasedThisFrame)
            {
                if (m_jumpBufferTimer > 0f)
                    m_jumpReleasedDuringBuffer = true;

                if (m_isJumping && VerticalVelocity > 0f)
                {
                    if (m_isPastApexThreshold)
                    {
                        m_isPastApexThreshold = false;
                        m_isFastFalling = true;
                        m_fastFallTime = characterMovementData.TimeForUpwardsCancel;
                        VerticalVelocity = 0f;
                    }
                    else
                    {
                        m_isFastFalling = true;
                        m_fastFallReleaseSpeed = VerticalVelocity;
                    }
                }
            }

            //Initiate Jump with Jump buffering & coyote time
            //------------------------------------------------//

            if (m_jumpBufferTimer > 0f && !m_isJumping && (m_isGrounded || m_coyoteTimer > 0f))
            {
                DoJump(1);

                if (m_jumpReleasedDuringBuffer)
                {
                    m_isFastFalling = true;
                    m_fastFallReleaseSpeed = VerticalVelocity;
                }
            }
            //Double Jump
            //------------------------------------------------//
            else if (m_jumpBufferTimer > 0f && m_isJumping &&
                     m_numberOfJumpsUsed < characterMovementData.NumberOfJumpsAllowed)
            {
                m_isFastFalling = false;
                DoJump(1);
            }
            //Air Jump after Coyote Time Lapsed
            //------------------------------------------------//
            else if (m_jumpBufferTimer > 0f && m_isFalling && m_numberOfJumpsUsed < characterMovementData.NumberOfJumpsAllowed - 1)
            {
                DoJump(2);
                m_isFastFalling = false;
            }

            //Landed
            //------------------------------------------------//
            if ((m_isJumping || m_isFalling) && m_isGrounded && VerticalVelocity <= 0f)
            {
                m_isJumping = false;
                m_isFalling = false;
                m_isFastFalling = false;
                m_fastFallTime = 0f;
                m_isPastApexThreshold = false;
                m_numberOfJumpsUsed = 0;

                VerticalVelocity = Physics2D.gravity.y;
            }
        }

        // Set velocity and animations for jump
        private void DoJump(int numberOfJumpsUsed)
        {
            if (!m_isJumping)
            {
                m_isJumping = true;
            }

            m_jumpBufferTimer = 0f;
            VerticalVelocity = characterMovementData.InitialJumpVelocity;
            m_numberOfJumpsUsed += numberOfJumpsUsed;
        }

        private void Jump()
        {

            //Apply Gravity While Jumping
            //------------------------------------------------//
            if (m_isJumping)
            {
                //Check for Head Bump
                //------------------------------------------------//
                if (m_bumpedHead)
                    m_isFastFalling = true;
                //Gravity on Ascending
                //------------------------------------------------//
                if (VerticalVelocity >= 0f)
                {
                    //Apex Controls
                    //------------------------------------------------//
                    m_apexPoint = Mathf.InverseLerp(characterMovementData.InitialJumpVelocity, 0f, VerticalVelocity);

                    if (m_apexPoint > characterMovementData.ApexThreshold)
                    {
                        if (!m_isPastApexThreshold)
                        {
                            m_isPastApexThreshold = true;
                            m_timePastApexThreshold = 0f;
                        }

                        if (m_isPastApexThreshold)
                        {
                            m_timePastApexThreshold += Time.fixedDeltaTime;
                            if (m_timePastApexThreshold < characterMovementData.ApexHangTime)
                                VerticalVelocity = 0f;
                            else
                                VerticalVelocity = -0.01f;
                        }
                    }
                    //Gravity on Ascending but not past Apex Threshold
                    //------------------------------------------------//
                    else
                    {
                        VerticalVelocity += characterMovementData.Gravity * Time.fixedDeltaTime;
                        if (m_isPastApexThreshold)
                        {
                            m_isPastApexThreshold = false;
                        }
                    }
                }
                //Gravity on Descending
                //------------------------------------------------//
                else if(!m_isFastFalling)
                {
                    VerticalVelocity += characterMovementData.Gravity * characterMovementData.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }
                else if (VerticalVelocity < 0f)
                {
                    if (!m_isFalling)
                        m_isFalling = true;
                }
            }
            

            //Jump Cut
            //------------------------------------------------//
            if (m_isFastFalling)
            {
                if (m_fastFallTime >= characterMovementData.TimeForUpwardsCancel)
                {
                    VerticalVelocity += characterMovementData.Gravity * characterMovementData.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }
                else if (m_fastFallTime < characterMovementData.TimeForUpwardsCancel)
                {
                    VerticalVelocity = Mathf.Lerp(m_fastFallReleaseSpeed, 0f, (m_fastFallTime / characterMovementData.TimeForUpwardsCancel));
                }

                m_fastFallTime += Time.fixedDeltaTime;
            }
            
            //Normal Gravity While Falling
            //------------------------------------------------//

            if (!m_isGrounded && !m_isJumping)
            {
                if (!m_isFalling)
                    m_isFalling = true;

                VerticalVelocity += characterMovementData.Gravity * Time.fixedDeltaTime;
            }
            
            //Clamp Fall Speed
            //------------------------------------------------//
            VerticalVelocity = Math.Clamp(VerticalVelocity, -characterMovementData.MaxFallSpeed, characterMovementData.MaxVerticalVelocity);
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, VerticalVelocity);
        }

        #endregion

        //Timers
        //============================================================================================================//

        #region Timers

        private void CountTimers()
        {
            m_jumpBufferTimer -= Time.deltaTime;
            if (!m_isGrounded)
            {
                m_coyoteTimer -= Time.deltaTime;
            }
            else
            {
                m_coyoteTimer = characterMovementData.JumpCoyoteTime;
            }
        }

        #endregion
        
        //Input Handlers
        //============================================================================================================//

        #region Input Handlers

        private void OnMovementChanged(Vector2 moveInput)
        {
            m_moveInput = moveInput;
        }

        private void OnJumpPressed(bool pressed)
        {
            m_isJumpPressed = pressed;
        }
        
        private void OnRunPressed(bool pressed)
        {
            m_isRunPressed = pressed;
        }

        #endregion

        //Misc
        //============================================================================================================//

        // Add speed from a source outside the input
        public void AddExternalVel(Vector2 vel)
        {
            //Debug.Log($"Adding external vel {vel}");
            m_externalVel += vel;
        }

        //Unity Editor
        //============================================================================================================//

        #region Unity Editor

#if UNITY_EDITOR
        
        [NonSerialized]
        private readonly RaycastHit2D[] m_editorNonAllocRaycastHits = new RaycastHit2D[1];

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                var vel = m_rigidbody2D == null ? Vector2.zero : m_rigidbody2D.linearVelocity;
                Draw.Label(transform.position, $"Grounded: {m_isGrounded} Jump: {m_isJumping}\nVelocity: {vel}\nExtern: {m_externalVel}");
            }

            if (characterMovementData.CanDraw(DEBUG_DRAW.JUMP_ARC))
                DrawJumpArc(characterMovementData.MaxRunSpeed, Color.red);
            if (characterMovementData.CanDraw(DEBUG_DRAW.JUMP_RUN_ARC))
                DrawJumpArc(characterMovementData.MaxWalkSpeed, Color.green);
        }


        
        private void DrawJumpArc(float moveSpeed, Color gizmoColor)
        {
            var startPosition = new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
            var previousPos = startPosition;

            var speed = characterMovementData.CanDraw(DEBUG_DRAW.FACE_RIGHT) ? moveSpeed : -moveSpeed;

            var velocity = new Vector2(speed, characterMovementData.InitialJumpVelocity);

            Gizmos.color = gizmoColor;
            var timeStep = 2 * characterMovementData.TimeTillJumpApex / characterMovementData.ArcResolution;

            var count = characterMovementData.VisualizationSteps;
            for (int i = 0; i < count; i++)
            {
                var timeTillJumpApex = characterMovementData.TimeTillJumpApex;
                
                var simTime = i * timeStep;
                Vector2 displacement;

                if (simTime < timeTillJumpApex) //Ascending
                {
                    displacement = velocity * simTime + 0.5f * new Vector2(0f, characterMovementData.Gravity) * simTime * simTime;
                }
                else if (simTime < timeTillJumpApex + characterMovementData.ApexHangTime) //Apex Hang Time
                {
                    var apexTime = simTime - timeTillJumpApex;
                    displacement = velocity * timeTillJumpApex + 0.5f * new Vector2(0f, characterMovementData.Gravity) * timeTillJumpApex * timeTillJumpApex;
                    displacement += new Vector2(speed, 0) * apexTime;
                }
                else //Descending
                {
                    float descendTime = simTime - (timeTillJumpApex + characterMovementData.ApexHangTime);
                    displacement = velocity * timeTillJumpApex + 0.5f * new Vector2(0f, characterMovementData.Gravity) * timeTillJumpApex * timeTillJumpApex;
                    displacement += new Vector2(speed, 0f) * characterMovementData.ApexHangTime;
                    displacement += new Vector2(speed, 0f) * descendTime + 0.5f * new Vector2(0f, characterMovementData.Gravity) * descendTime * descendTime;
                }

                var drawPoint = startPosition + displacement;

                if (characterMovementData.CanDraw(DEBUG_DRAW.JUMP_WITH_COLLISION))
                {
                    var dir = drawPoint - previousPos;
                    var length = dir.magnitude;
                    var hitCount = Physics2D.RaycastNonAlloc(previousPos, dir, m_editorNonAllocRaycastHits, length, characterMovementData.GroundLayer.value);

                    if (hitCount > 0)
                    {
                        Gizmos.DrawLine(previousPos, m_editorNonAllocRaycastHits[0].point);
                        break;
                    }
                }
                
                Gizmos.DrawLine(previousPos, drawPoint);
                previousPos = drawPoint;
            }
            
        }
        
        

#endif

        #endregion //Unity Editor
        
        //============================================================================================================//
    }
}
