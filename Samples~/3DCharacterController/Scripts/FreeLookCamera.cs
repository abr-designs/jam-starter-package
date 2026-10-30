// Created by Claude (claude-opus-5-5)
// Date: 2026-10-28

#if !JAM_INPUT_DELEGATOR
#define OLD_INPUT_SYSTEM
#endif

using UnityEngine;

#if JAM_INPUT_DELEGATOR
using GameInput;
using UnityEngine.InputSystem;
#endif

namespace Samples.Cameras
{
    /// <summary>
    /// Orbits a target transform with the mouse, keeping a fixed distance from a look-at point
    /// offset from the target. No Cinemachine; place on any camera and assign a target.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-5-5) — 2026-10-28</remarks>
    [ExecuteAlways]
    public class FreeLookCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField]
        private Transform target;

        [SerializeField, Tooltip("World-space offset from the target's position that the camera orbits and looks at. " +
                                 "e.g. (0, 1.5, 0) to look at a character's head instead of their feet.")]
        private Vector3 lookAtOffset = new Vector3(0f, 1.5f, 0f);

        [SerializeField, Min(0f)]
        private float distance = 6f;

        [Header("Orbit")]
        [SerializeField, Tooltip("Degrees rotated when the mouse is dragged the full height of the screen. Independent of resolution and pixel density.")]
        private float orbitSpeed = 200f;

        [SerializeField, Tooltip("When enabled, the camera only orbits while the right mouse button is held.")]
        private bool requireRightMouseHeld = true;

        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 70f;

        [Header("Damping")]
        [SerializeField, Min(0f), Tooltip("Seconds to catch up to the target's position. 0 = locked to the target.")]
        private float positionDampTime = 0.1f;

        [SerializeField, Min(0f), Tooltip("Seconds to catch up to the mouse-driven orbit angle. 0 = instant.")]
        private float rotationDampTime = 0.05f;

        private float m_yaw;
        private float m_pitch;
        private float m_targetYaw;
        private float m_targetPitch;
        private float m_yawVelocity;
        private float m_pitchVelocity;

        private Vector3 m_lookAtPoint;
        private Vector3 m_lookAtVelocity;

        private bool m_isRightMouseHeld;

        //================================================================================================================//

#if JAM_INPUT_DELEGATOR
        private void OnEnable()
        {
            GameInputDelegator.OnRightClick += OnRightClick;
        }
#endif

        private void Start()
        {
            SetOrbitAnglesFromTransform();

            if (target != null)
                m_lookAtPoint = target.position + lookAtOffset;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            var desiredLookAtPoint = target.position + lookAtOffset;

            //In edit mode the transform's rotation is the source of truth, so rotating the camera in the
            //scene swings it around the target instead of being overwritten. Damping is skipped there,
            //since edit mode deltaTime is irregular.
            if (Application.isPlaying)
            {
                ProcessOrbitInput();

                m_yaw = Mathf.SmoothDamp(m_yaw, m_targetYaw, ref m_yawVelocity, rotationDampTime);
                m_pitch = Mathf.SmoothDamp(m_pitch, m_targetPitch, ref m_pitchVelocity, rotationDampTime);
                m_lookAtPoint = Vector3.SmoothDamp(m_lookAtPoint, desiredLookAtPoint, ref m_lookAtVelocity, positionDampTime);
            }
            else
            {
                SetOrbitAnglesFromTransform();
                m_lookAtPoint = desiredLookAtPoint;
            }

            var rotation = Quaternion.Euler(m_pitch, m_yaw, 0f);

            transform.SetPositionAndRotation(m_lookAtPoint - rotation * Vector3.forward * distance, rotation);
        }

#if JAM_INPUT_DELEGATOR
        private void OnDisable()
        {
            GameInputDelegator.OnRightClick -= OnRightClick;
        }
#endif

        //Public API
        //================================================================================================================//

        public void SetTarget(Transform newTarget)
        {
            //Coming from no target there is nothing to damp from, so start on the new one.
            if (target == null && newTarget != null)
                m_lookAtPoint = newTarget.position + lookAtOffset;

            target = newTarget;
        }

        public void SetLookAtOffset(Vector3 newLookAtOffset)
        {
            lookAtOffset = newLookAtOffset;
        }

        public void SetDistance(float newDistance)
        {
            distance = Mathf.Max(0f, newDistance);
        }

        public void SetOrbitSpeed(float newOrbitSpeed)
        {
            orbitSpeed = newOrbitSpeed;
        }

        //Input
        //================================================================================================================//

        /// <summary>
        /// Turns this frame's mouse movement into a target yaw and pitch for the damping to chase.
        /// Mouse delta is already a per-frame amount, so it is not scaled by deltaTime.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5) — 2026-10-28</remarks>
        private void ProcessOrbitInput()
        {
#if OLD_INPUT_SYSTEM
            m_isRightMouseHeld = Input.GetMouseButton(1);
#endif
            if (requireRightMouseHeld && !m_isRightMouseHeld)
                return;

            var mouseDelta = GetMouseDelta();

            m_targetYaw += mouseDelta.x * orbitSpeed;
            m_targetPitch = Mathf.Clamp(m_targetPitch - mouseDelta.y * orbitSpeed, minPitch, maxPitch);
        }

        /// <summary>
        /// Reads yaw and pitch back from the transform's current rotation, clamping pitch into range,
        /// and settles the damping on them. Seeds the orbit on Start so the camera doesn't snap, and
        /// drives it in edit mode.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5) — 2026-10-28</remarks>
        private void SetOrbitAnglesFromTransform()
        {
            var currentEuler = transform.eulerAngles;

            m_yaw = m_targetYaw = currentEuler.y;
            m_pitch = m_targetPitch = Mathf.Clamp(Mathf.DeltaAngle(0f, currentEuler.x), minPitch, maxPitch);
            m_yawVelocity = m_pitchVelocity = 0f;
        }

        /// <summary>
        /// Reads mouse movement as a fraction of the screen height from whichever input system is
        /// active. Dividing out the pixel count makes the same gesture turn the camera the same amount
        /// at any resolution or pixel density. Height alone is used so both axes share one scale and
        /// ultrawide screens don't turn slower horizontally.
        /// </summary>
        /// <remarks>Rewritten by Claude (claude-opus-5-5) — 2026-10-30</remarks>
        private static Vector2 GetMouseDelta()
        {
#if OLD_INPUT_SYSTEM
            Vector2 pixelDelta = Input.mousePositionDelta;
#else
            if (GameInputDelegator.LockInputs)
                return Vector2.zero;

            var pointer = Pointer.current;
            var pixelDelta = pointer == null ? Vector2.zero : pointer.delta.ReadValue();
#endif
            return pixelDelta / Screen.height;
        }

#if JAM_INPUT_DELEGATOR
        private void OnRightClick(bool pressed)
        {
            m_isRightMouseHeld = pressed;
        }
#endif

        //Unity Editor
        //================================================================================================================//

#if UNITY_EDITOR
        private const int k_GizmoSegmentCount = 32;

        /// <summary>
        /// Draws the look-at point, its offset from the target, the ring the camera orbits on at its
        /// current height, and the arc of reachable pitch. Measured from the live transform so it
        /// reads correctly in edit mode, before Start has seeded yaw and pitch.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5) — 2026-10-28</remarks>
        private void OnDrawGizmosSelected()
        {
            if (target == null)
                return;

            var lookAtPoint = target.position + lookAtOffset;
            var cameraPosition = transform.position;

            //Target and offset
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, 0.1f);
            Gizmos.DrawLine(target.position, lookAtPoint);
            Gizmos.DrawSphere(lookAtPoint, 0.15f);

            //Sight line, to the damped point the camera is actually aiming at
            Gizmos.color = Color.white;
            Gizmos.DrawLine(cameraPosition, Application.isPlaying ? m_lookAtPoint : lookAtPoint);

            //Orbit ring at the camera's current height
            var toCamera = cameraPosition - lookAtPoint;
            var ringRadius = new Vector2(toCamera.x, toCamera.z).magnitude;
            var ringCenter = lookAtPoint + Vector3.up * toCamera.y;

            Gizmos.color = Color.cyan;
            DrawGizmoArc(ringCenter, Vector3.forward * ringRadius, Vector3.right * ringRadius, 0f, 360f);

            //Pitch range, in the vertical plane the camera currently sits in
            var horizontalDirection = new Vector3(toCamera.x, 0f, toCamera.z);
            if (horizontalDirection.sqrMagnitude < 0.0001f)
                horizontalDirection = -transform.forward;
            horizontalDirection.y = 0f;
            horizontalDirection.Normalize();

            Gizmos.color = Color.magenta;
            DrawGizmoArc(lookAtPoint, horizontalDirection * distance, Vector3.up * distance, minPitch, maxPitch);
            Gizmos.DrawLine(lookAtPoint, GetArcPoint(lookAtPoint, horizontalDirection * distance, Vector3.up * distance, minPitch));
            Gizmos.DrawLine(lookAtPoint, GetArcPoint(lookAtPoint, horizontalDirection * distance, Vector3.up * distance, maxPitch));
        }

        /// <summary>
        /// Draws an arc from startAngle to endAngle in degrees, in the plane spanned by the two axes.
        /// The axes carry the radius, so a circle passes two perpendicular vectors of equal length.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5) — 2026-10-28</remarks>
        private static void DrawGizmoArc(Vector3 center, Vector3 axisA, Vector3 axisB, float startAngle, float endAngle)
        {
            var previousPoint = GetArcPoint(center, axisA, axisB, startAngle);

            for (int i = 1; i <= k_GizmoSegmentCount; i++)
            {
                var angle = Mathf.Lerp(startAngle, endAngle, i / (float)k_GizmoSegmentCount);
                var point = GetArcPoint(center, axisA, axisB, angle);

                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
        }

        private static Vector3 GetArcPoint(Vector3 center, Vector3 axisA, Vector3 axisB, float angle)
        {
            var radians = angle * Mathf.Deg2Rad;
            return center + axisA * Mathf.Cos(radians) + axisB * Mathf.Sin(radians);
        }
#endif
    }
}