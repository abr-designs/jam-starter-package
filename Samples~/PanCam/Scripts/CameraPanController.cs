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

        [SerializeField]
        private float minZoomDistance = 10f;

        [SerializeField] private float maxZoomDistance = 30f;
        private float m_currentZoom;
        private float m_targetZoom;
        private float m_zoomVelocity;

        [SerializeField, Tooltip("World units of zoom per scroll wheel notch.")]
        private float scrollZoomSensitivity = 2f;

        [SerializeField, Tooltip("World units of zoom per screen pixel of two-finger pinch.")]
        private float pinchZoomSensitivity = 0.05f;

        private bool m_isPinching;

        //public bool SuppressLastClick = false;

        //Movement
        //----------------------------------------------------------//
        [Header("Keyboard Movement")]
        [SerializeField]
        private bool useKeyboardMovement;
        [SerializeField] 
        private float moveSpeed = 20f;
        [SerializeField, Tooltip("Higher = snappier. Framerate independent.")]
        private float smoothing = 12f;
        
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
        /// Polls scroll, pinch and press from the legacy Input Manager. Only compiled when the
        /// GameInput sample is absent, so this path never references the Input System package.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) — 2026-08-15</remarks>
        private void Update()
        {
            var scrollNotches = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollNotches) > Mathf.Epsilon)
                OnScrollZoom(scrollNotches);

            if (Input.touchCount >= 2)
                DoPinchZoom();

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

        private void ProcessZoom()
        {
            UpdatePinchState();
            
            if (Mathf.Abs(m_currentZoom - m_targetZoom) < 0.0001f)
                return;

            var ray = new Ray(transform.position, transform.forward);
            if (!GroundPlane.Raycast(ray, out var distance))
                return;

            var focalPoint = ray.GetPoint(distance);

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

            Vector3 desiredVelocity = (forward * input.y + right * input.x) * moveSpeed;

            // Framerate-independent exponential smoothing.
            float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            m_currentVelocity = Vector3.Lerp(m_currentVelocity, desiredVelocity, t);

            transform.position += m_currentVelocity * Time.deltaTime;
        }

        private void ProcessBounds()
        {
            if (!useBounds)
                return;

            var pos = transform.position;
            pos.x = Math.Clamp(pos.x, xBounds.x, xBounds.y);
            pos.z = Math.Clamp(pos.z, zBounds.x, zBounds.y);
            transform.position = pos;
        }

        #endregion //Movement Processing

        //Zoom Sources
        //================================================================================================================//

        /// <summary>
        /// Converts a scroll wheel reading into world units of zoom. The raw magnitude is platform
        /// specific (one notch is 1 on the legacy Input Manager, 120 on Windows through the Input
        /// System), so it is clamped to a single notch and one sensitivity value serves both paths.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) — 2026-08-15</remarks>
        private void OnScrollZoom(float rawScroll)
        {
            ApplyZoomDelta(Mathf.Clamp(rawScroll, -1f, 1f) * scrollZoomSensitivity);
        }

        /// <summary>
        /// Converts a signed per-frame pinch delta, in screen pixels, into world units of zoom.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) — 2026-08-15</remarks>
        private void OnPinchZoom(float pixelDelta)
        {
            ApplyZoomDelta(pixelDelta * pinchZoomSensitivity);
        }

        private void ApplyZoomDelta(float delta)
        {
            if (Mathf.Abs(delta) < 0.001f)
                return;

            m_targetZoom = Mathf.Clamp(
                m_targetZoom - delta,
                minZoomDistance,
                maxZoomDistance
            );
        }

#if OLD_INPUT_SYSTEM
        /// <summary>
        /// Measures the per-frame change in distance between the first two legacy touches. Mirrors
        /// what PinchingComposite produces on the Input System path, so both feed the same sensitivity.
        /// </summary>
        /// <remarks>Rewritten by Claude (claude-opus-5) — 2026-08-15</remarks>
        private void DoPinchZoom()
        {
            var touch0 = Input.GetTouch(0);
            var touch1 = Input.GetTouch(1);

            var currentDistance = Vector2.Distance(touch0.position, touch1.position);
            var previousDistance = Vector2.Distance(
                touch0.position - touch0.deltaPosition,
                touch1.position - touch1.deltaPosition);

            OnPinchZoom(currentDistance - previousDistance);
        }
#endif

        //Callbacks
        //================================================================================================================//

#if JAM_INPUT_DELEGATOR
        /// <summary>
        /// Routes the shared Zoom action to the matching sensitivity. Both bindings emit a signed
        /// per-frame delta, but in different units, and only the live touch count tells them apart.
        /// </summary>
        /// <remarks>Rewritten by Claude (claude-opus-5) — 2026-08-15</remarks>
        private void OnCameraZoom(float rawZoomDelta)
        {
            if (GetActiveTouchCount() >= 2)
                OnPinchZoom(rawZoomDelta);
            else
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
        private void OnDrawGizmosSelected()
        {
            if (!useBounds)
                return;

            //Halfway between 0f & camera position
            var yPos = transform.position.y / 2f;

            Span<Vector3> points = stackalloc Vector3[]
            {
                new Vector3(xBounds.x, yPos, zBounds.y),
                new Vector3(xBounds.y, yPos, zBounds.y),
                new Vector3(xBounds.y, yPos, zBounds.x),
                new Vector3(xBounds.x, yPos, zBounds.x)
            };

            Gizmos.color = Color.yellow;
            Gizmos.DrawLineStrip(points, true);
        }

#endif
    }
}
