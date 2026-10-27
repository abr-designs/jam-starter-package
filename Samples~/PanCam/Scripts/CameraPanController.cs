#if !JAM_INPUT_DELEGATOR
#define OLD_INPUT_SYSTEM
#endif

using UnityEngine;
using Cysharp.Threading.Tasks;
using Utilities.Tweening;
using System;
using Utilities.Enums;

#if JAM_INPUT_DELEGATOR
using GameInput;
using UnityEngine.InputSystem;
#endif

namespace Samples.CameraPan
{

    [RequireComponent(typeof(Camera))]
    public class CameraPanController : MonoBehaviour
    {
        private static readonly Plane GroundPlane = new(Vector3.up, Vector3.zero);
        public static event Action<bool> OnDragStateChanged;

        //Pointer Drag
        //----------------------------------------------------------//

        [SerializeField, Header("Camera Pointer Drag")]
        [Tooltip("Drag threshold in screen pixels")]
        private float dragThreshold = 5.0f;

        private bool m_isPointerPressed;
        private bool m_isDragging;
        private bool m_dragDuringLastClick;
        private Vector2 m_pressStartScreenPosition;
        private Vector3 m_dragWorldAnchor;

        //Reposition
        //----------------------------------------------------------//

        [Header("Camera Focus Move")]
        [SerializeField, Min(0f)]
        [Tooltip("Amount of time in Seconds it takes for the camera to move to its new focus")]
        private float lookAtFocusTime = 0.2f;

        //Camera Zoom
        //----------------------------------------------------------//

        [Header("Zoom Bounds")]
        [SerializeField, Tooltip("SmoothDamp time for the zoom interpolation. Lower = snappier.")]
        private float zoomSmoothTime = 0.12f;

        [SerializeField, Min(0.01f)]
        [Tooltip("Zoom scales multiplicatively, so this can never be zero. A zero distance would " +
                 "trap the camera at the focal point with nothing left to scale.")]
        private float minZoomDistance = 10f;

        [SerializeField] private float maxZoomDistance = 30f;
        private float m_currentZoom;
        private float m_targetZoom;
        private float m_zoomVelocity;

        [SerializeField, Min(0f)]
        [Tooltip("Fraction of the current zoom distance added or removed per scroll wheel notch. " +
                 "0.1 = 10% per notch, so a notch covers more ground the further out the camera is.")]
        private float scrollZoomPercent = 0.1f;

        [SerializeField, Min(0f)]
        [Tooltip("How closely zoom tracks the pinch ratio. 1 = the camera moves the same " +
                 "percentage the fingers did, 0.5 = softer, 2 = stronger.")]
        private float pinchSensitivity = 1f;

        private bool m_isPinching;
        private float m_previousPinchDistance;

        //public bool SuppressLastClick = false;

        //Movement
        //----------------------------------------------------------//
        [Header("Keyboard Movement")]
        [SerializeField]
        private bool useKeyboardMovement;
        [SerializeField] 
        private float moveSpeed = 20f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("How strongly Move Speed follows the zoom distance. 1 holds the same screen-space " +
                 "speed at any range, 0.5 softens the ramp, 0 keeps Move Speed fixed.")]
        private float moveSpeedZoomScale = 0.5f;

        [SerializeField, Min(0.0001f)]
        [Tooltip("Seconds for movement to reach 99% of Move Speed. Lower is snappier. Framerate " +
                 "independent. Cannot be zero, since a paused timeScale would divide by it.")]
        private float settleTime = 0.4f;
        
        private Vector3 m_currentVelocity;
        private Vector2 m_keyboardInput;
        
        //Bounds
        //----------------------------------------------------------//
        [Header("Bounds (Optional)")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 xBounds = new Vector2(-100, 100);
        [SerializeField] private Vector2 zBounds = new Vector2(-100, 100);

        //Misc
        //----------------------------------------------------------//

        private Camera m_camera;

        //================================================================================================================//

#if JAM_INPUT_DELEGATOR
        private void OnEnable()
        {
            GameInputDelegator.OnZoomChanged += OnCameraZoom;
            GameInputDelegator.OnPrimaryPressChanged += OnPrimaryPress;
            GameInputDelegator.OnMovementChanged += OnMovementChanged;
        }
#endif

        private void Start()
        {
            m_camera = GetComponent<Camera>();
            //m_currentZoom = minZoomDistance;
            m_targetZoom = minZoomDistance;
        }

#if OLD_INPUT_SYSTEM
        /// <summary>
        /// Polls scroll and press from the legacy Input Manager. Only compiled when the
        /// GameInput sample is absent, so this path never references the Input System package.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) — 2026-08-15</remarks>
        /// <remarks>Rewritten by Claude (claude-opus-5) — 2026-09-01</remarks>
        private void Update()
        {
            var scrollNotches = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollNotches) > Mathf.Epsilon)
                OnScrollZoom(scrollNotches);

            if (Input.GetMouseButtonDown(0))
                OnPrimaryPress(true);
            else if (Input.GetMouseButtonUp(0))
                OnPrimaryPress(false);
        }
#endif

        private void LateUpdate()
        {
            ProcessZoom();
            ProcessDrag();
            ProcessMovement();
            ProcessBounds();
        }

#if JAM_INPUT_DELEGATOR
        private void OnDisable()
        {
            GameInputDelegator.OnZoomChanged -= OnCameraZoom;
            GameInputDelegator.OnPrimaryPressChanged -= OnPrimaryPress;
            GameInputDelegator.OnMovementChanged -= OnMovementChanged;
        }
#endif

        //================================================================================================================//

        private async UniTaskVoid SetCameraLookAt(Vector3 worldPosition)
        {
            var targetCameraPos = worldPosition + (-transform.forward * minZoomDistance);

            m_currentZoom = minZoomDistance;
            m_targetZoom = minZoomDistance;
            m_zoomVelocity = 0f;

            await AnimateCamera(targetCameraPos);

            return;

            async UniTask AnimateCamera(Vector3 targetPos)
            {
#if JAM_INPUT_DELEGATOR
                GameInputDelegator.SetInputLock(true);
#endif
                await transform.TweenToAsync(SPACE.WORLD, targetPos, lookAtFocusTime, CURVE.EASE_IN_OUT);
#if JAM_INPUT_DELEGATOR
                GameInputDelegator.SetInputLock(false);
#endif
            }
        }

        //Input Processing
        //================================================================================================================//

        #region Input Processing

        /// <summary>
        /// Reads the primary pointer in screen pixels. Resolves to the legacy mouse position, or to
        /// whichever Pointer device is current, which covers mouse, pen and primary touch alike.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) — 2026-08-15</remarks>
        private static Vector2 GetPointerScreenPosition()
        {
#if OLD_INPUT_SYSTEM
            return Input.mousePosition;
#else
            var pointer = Pointer.current;

            return pointer == null ? Vector2.zero : pointer.position.ReadValue();
#endif
        }

        /// <summary>
        /// Counts fingers currently touching the screen. Returns zero on devices with no touchscreen.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) — 2026-08-15</remarks>
        private static int GetActiveTouchCount()
        {
#if OLD_INPUT_SYSTEM
            return Input.touchCount;
#else
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return 0;

            var touches = touchscreen.touches;
            var count = 0;

            for (int i = 0; i < touches.Count; i++)
            {
                if (touches[i].press.isPressed)
                    count++;
            }

            return count;
#endif
        }

        /// <summary>
        /// Measures the screen-pixel distance between the first two touch slots. Returns false unless
        /// both are pressed, so the caller can hold its previous reading.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) — 2026-09-01</remarks>
        private static bool TryGetPinchDistance(out float distance)
        {
#if OLD_INPUT_SYSTEM
            if (Input.touchCount < 2)
            {
                distance = 0f;
                return false;
            }

            distance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
            return true;
#else
            distance = 0f;

            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return false;

            var touches = touchscreen.touches;
            if (touches.Count < 2)
                return false;

            var first = touches[0];
            var second = touches[1];

            if (!first.press.isPressed || !second.press.isPressed)
                return false;

            distance = Vector2.Distance(first.position.ReadValue(), second.position.ReadValue());
            return true;
#endif
        }

        /// <summary>
        /// Raycasts the pointer against the ground plane, clamping to the screen first so an off-screen
        /// pointer still resolves to a usable world point.
        /// </summary>
        /// <remarks>Rewritten by Claude (claude-opus-5) — 2026-08-15</remarks>
        private bool TryGetPointerWorldPosition(out Vector3 worldPosition)
        {
            var screenPosition = GetPointerScreenPosition();

            screenPosition.x = Mathf.Clamp(screenPosition.x, 0f, Screen.width - 1f);
            screenPosition.y = Mathf.Clamp(screenPosition.y, 0f, Screen.height - 1f);

            var ray = m_camera.ScreenPointToRay(screenPosition);
            if (GroundPlane.Raycast(ray, out var distance))
            {
                worldPosition = ray.GetPoint(distance);
                return true;
            }

            worldPosition = default;
            return false;
        }

        /// <summary>
        /// Tracks whether a two-finger gesture owns the frame. A starting pinch cancels any drag in
        /// progress, and an ending pinch drops the press flag so the finger left on screen cannot
        /// resume panning from a stale anchor.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) — 2026-08-15</remarks>
        private void UpdatePinchState()
        {
            var wasPinching = m_isPinching;
            m_isPinching = GetActiveTouchCount() >= 2;

            if (m_isPinching == wasPinching)
                return;

            //Both edges drop the baseline so the next pinch seeds from its own first frame.
            m_previousPinchDistance = 0f;

            if (m_isPinching)
            {
                m_dragDuringLastClick = true;
                //SuppressLastClick = true;

                if (!m_isDragging) 
                    return;
                
                m_isDragging = false;
                OnDragStateChanged?.Invoke(false);

                return;
            }

            m_isPointerPressed = false;
        }

        #endregion //Input Processing

        //Movement Processing
        //================================================================================================================//

        #region Movement Processing

        /// <summary>
        /// Finds the ground point the camera is aimed at by casting its forward ray onto the ground
        /// plane. Reads only the transform, so editor gizmos can call it before the camera reference
        /// is assigned. Returns false when the camera is level or tilted up and the ray never lands.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) — 2026-09-01</remarks>
        private bool TryGetFocalPoint(out Vector3 focalPoint)
        {
            var ray = new Ray(transform.position, transform.forward);

            if (!GroundPlane.Raycast(ray, out var distance))
            {
                focalPoint = default;
                return false;
            }

            focalPoint = ray.GetPoint(distance);
            return true;
        }

        /// <summary>
        /// Zooms by the ratio between this frame's finger distance and the last one, so the gesture
        /// reads the same on any screen density and at any zoom.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) — 2026-09-01</remarks>
        private void ProcessPinchZoom()
        {
            if (!m_isPinching)
                return;

            if (!TryGetPinchDistance(out var pinchDistance) || pinchDistance <= 0f)
                return;

            //First frame of the gesture only seeds the baseline; there is nothing to divide by yet.
            if (m_previousPinchDistance <= 0f)
            {
                m_previousPinchDistance = pinchDistance;
                return;
            }

            var ratio = pinchDistance / m_previousPinchDistance;

            ApplyZoomTarget(m_targetZoom / Mathf.Pow(ratio, pinchSensitivity));

            m_previousPinchDistance = pinchDistance;
        }

        private void ProcessZoom()
        {
            UpdatePinchState();
            ProcessPinchZoom();

            if (Mathf.Abs(m_currentZoom - m_targetZoom) < 0.0001f)
                return;

            if (!TryGetFocalPoint(out var focalPoint))
                return;

            m_currentZoom = Mathf.SmoothDamp(m_currentZoom, m_targetZoom, ref m_zoomVelocity, zoomSmoothTime);
            transform.position = focalPoint - transform.forward * m_currentZoom;
        }

        private void ProcessDrag()
        {
            //Pinch owns the gesture outright; panning stays out until the fingers lift.
            if (m_isPinching)
                return;

            if (!m_isPointerPressed)
                return;

            if (m_isDragging)
            {
                TryGetPointerWorldPosition(out var newWorldPos);
                var offset = m_dragWorldAnchor - newWorldPos;
                transform.position += offset;
                return;
            }

            var pointerDelta = GetPointerScreenPosition() - m_pressStartScreenPosition;
            if (!(Mathf.Abs(pointerDelta.x) > dragThreshold) && !(Mathf.Abs(pointerDelta.y) > dragThreshold))
                return;

            m_isDragging = true;
            m_dragDuringLastClick = true;
            OnDragStateChanged?.Invoke(m_isDragging);
        }

        /// <summary>
        /// Moves the camera along the ground from keyboard input, scaling the speed by how far out
        /// the camera is, by an amount moveSpeedZoomScale controls. Drag panning needs no equivalent,
        /// being anchored to the world point under the pointer.
        /// </summary>
        /// <remarks>Rewritten by Claude (claude-opus-5) — 2026-09-01</remarks>
        private void ProcessMovement()
        {
            if (!useKeyboardMovement)
                return;
            
#if JAM_INPUT_DELEGATOR
            var input = GameInputDelegator.LockInputs ? Vector2.zero : m_keyboardInput;
#else
            //Nothing feeds m_keyboardInput without the delegator, so this stays zero on the legacy path.
            var input = m_keyboardInput;
#endif

            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            //Measured off the live transform rather than m_currentZoom, which reads zero until the
            //first zoom settles and never accounts for the height the camera was authored at.
            var zoomScale = TryGetFocalPoint(out var focalPoint)
                ? Mathf.Pow(Vector3.Distance(transform.position, focalPoint) / minZoomDistance, moveSpeedZoomScale)
                : 1f;

            Vector3 desiredVelocity = (forward * input.y + right * input.x) * (moveSpeed * zoomScale);

            //Reaches 99% of the target velocity after settleTime seconds.
            float t = 1f - Mathf.Pow(0.01f, Time.deltaTime / settleTime);
            m_currentVelocity = Vector3.Lerp(m_currentVelocity, desiredVelocity, t);

            transform.position += m_currentVelocity * Time.deltaTime;
        }

        /// <summary>
        /// Keeps the ground point the camera looks at inside the configured rectangle. The camera sits
        /// behind that point by the zoom distance, so clamping the transform itself would shove the
        /// focal point along as the camera pulls back, and that shift would persist after zooming in.
        /// </summary>
        /// <remarks>Rewritten by Claude (claude-opus-5) — 2026-09-01</remarks>
        private void ProcessBounds()
        {
            if (!useBounds)
                return;

            if (!TryGetFocalPoint(out var focalPoint))
                return;

            var clampedX = Math.Clamp(focalPoint.x, xBounds.x, xBounds.y);
            var clampedZ = Math.Clamp(focalPoint.z, zBounds.x, zBounds.y);

            transform.position += new Vector3(clampedX - focalPoint.x, 0f, clampedZ - focalPoint.z);
        }

        #endregion //Movement Processing

        //Zoom Sources
        //================================================================================================================//

        /// <summary>
        /// Scales the zoom distance by a percentage per notch, so one notch covers more ground the
        /// further out the camera is. The raw magnitude is platform specific (one notch is 1 on the
        /// legacy Input Manager, 120 on Windows through the Input System), so it is clamped to a
        /// single notch and one sensitivity value serves both paths. A partial notch from a trackpad
        /// survives the clamp and scales the exponent.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) — 2026-08-15</remarks>
        /// <remarks>Rewritten by Claude (claude-opus-5) — 2026-09-01</remarks>
        private void OnScrollZoom(float rawScroll)
        {
            var notches = Mathf.Clamp(rawScroll, -1f, 1f);

            ApplyZoomTarget(m_targetZoom * Mathf.Pow(1f + scrollZoomPercent, -notches));
        }

        /// <summary>
        /// Clamps a requested zoom distance into the configured bounds. Every zoom source computes
        /// its own target by scaling the current one, so the bounds are enforced in one place.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) — 2026-09-01</remarks>
        private void ApplyZoomTarget(float desiredZoom)
        {
            if (Mathf.Abs(desiredZoom - m_targetZoom) < 0.001f)
                return;

            m_targetZoom = Mathf.Clamp(
                desiredZoom,
                minZoomDistance,
                maxZoomDistance
            );
        }

        //Callbacks
        //================================================================================================================//

#if JAM_INPUT_DELEGATOR
        /// <summary>
        /// Routes the shared Zoom action to the scroll sensitivity. The pinch binding also drives this
        /// action, but its pixel delta is discarded because ProcessZoom reads the fingers directly.
        /// </summary>
        /// <remarks>Rewritten by Claude (claude-opus-5) — 2026-09-01</remarks>
        private void OnCameraZoom(float rawZoomDelta)
        {
            if (GetActiveTouchCount() >= 2)
                return;

            OnScrollZoom(rawZoomDelta);
        }

        private void OnMovementChanged(Vector2 input)
        {
            m_keyboardInput = input;
        }
#endif

        private void OnPrimaryPress(bool pressed)
        {
            //A press that arrives mid-pinch belongs to the pinch, not to a new pan.
            if (m_isPinching)
                return;

            m_isPointerPressed = pressed;

            if (m_isDragging)
            {
                m_isDragging = false;
                OnDragStateChanged?.Invoke(m_isDragging);
            }

            if (!pressed)
                return;

            m_dragDuringLastClick = false;

            TryGetPointerWorldPosition(out m_dragWorldAnchor);
            m_pressStartScreenPosition = GetPointerScreenPosition();
        }

        //Unity Editor
        //================================================================================================================//

#if UNITY_EDITOR
        /// <summary>
        /// Draws the bounds on the ground plane they constrain, plus a marker on the point the camera
        /// is aimed at so the clamp can be seen acting while tuning. The marker scales with the zoom
        /// distance to stay visible at any range.
        /// </summary>
        /// <remarks>Rewritten by Claude (claude-opus-5) — 2026-09-01</remarks>
        private void OnDrawGizmosSelected()
        {
            if (!useBounds)
                return;

            Span<Vector3> points = stackalloc Vector3[]
            {
                new Vector3(xBounds.x, 0f, zBounds.y),
                new Vector3(xBounds.y, 0f, zBounds.y),
                new Vector3(xBounds.y, 0f, zBounds.x),
                new Vector3(xBounds.x, 0f, zBounds.x)
            };

            Gizmos.color = Color.yellow;
            Gizmos.DrawLineStrip(points, true);

            if (!TryGetFocalPoint(out var focalPoint))
                return;

            Gizmos.DrawSphere(focalPoint, Vector3.Distance(transform.position, focalPoint) * 0.02f);
        }

#endif
    }
}
