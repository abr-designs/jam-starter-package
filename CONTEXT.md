# Context Map

Domain language for the jam-starter-package runtime systems.

---

## Path System (`Utilities.Animations`)

### Arc-length parameterization
`t` (0–1) maps to a **uniform distance** along the path, not a raw segment index. `BakeArcLengthTable()` pre-computes cumulative distances so `Evaluate(t)` moves at constant speed regardless of segment density.

### Phantom extension
For non-looping Catmull-Rom paths, control points beyond the first and last are **extrapolated**: `phantom_before = p[0] + (p[0] - p[1])` and `phantom_after = p[n-1] + (p[n-1] - p[n-2])`. This ensures smooth tangents at path endpoints without requiring ghost points in the inspector.

### Ping-pong
Non-looping `SimplePathFollow` **reverses direction** when `m_distanceTravelled` reaches 0 or `m_totalLength`. The `m_pingPongForward` flag tracks current direction. Negative `speed` initialises `m_pingPongForward = false`, causing the first traversal to move in the positive-distance direction (and face in reverse via `-tangent`).

### Looping
When `looping = true`, `m_distanceTravelled` wraps via `% m_totalLength` rather than bouncing. The arc-length table includes one closing sample that returns `pathPoints[0]`, guaranteeing no float-precision gap at the seam.

### Motion modes
- `LINEAR` — straight-line segments between waypoints; arc table has one entry per point (plus one for looping close).
- `SMOOTH` — Catmull-Rom spline; arc table has `(segments × catmullResolution) + 1` entries.

---

## Camera Pan (`Samples.CameraPan`, `GameInput.Composites`)

### Zoom delta
A **signed, per-frame** change requesting camera movement along the view axis. Positive zooms in (reduces distance to the focal point), negative zooms out, zero means idle. Every zoom source emits this shape, including the scroll wheel and the two-finger pinch, which is what lets them share a single `Zoom` action.

The value is **raw and unscaled** at the source. A scroll binding emits notches, `PinchingComposite` emits screen pixels, and neither applies a sensitivity of its own. Converting to world units is the consumer's job, so the same tuning values hold whether the project runs on the Input Manager or the Input System. See `.agents/docs/adr/0005-pinch-composite-signed-delta.md`.

### Focal point
The ground point directly under the camera's forward ray, found by `TryGetFocalPoint`. It is **derived every frame** from the transform rather than stored, so whatever moved the camera last owns it. Zoom rebuilds the camera position as `focalPoint - forward * distance`, which is what holds the view steady while the distance changes.

Deriving carries one consequence. Anything that moves the camera on X or Z moves the focal point with it, so a constraint meant to hold the focus still has to be expressed as a correction to the focal point and applied back to the transform.

The distance from the camera to its focal point is the controller's measure of how far out it is. The bounds clamp reads it, and `ProcessMovement` raises it over `Min Zoom Distance` to the `moveSpeedZoomScale` power to decide how much keyboard speed grows with range. It is taken from the transform rather than from `m_currentZoom`, which reads zero until the first zoom settles and never reflects the height the camera was authored at.

### Multiplicative zoom
Inside `CameraPanController`, a zoom source never adds world units. It **scales the current distance**, and `ApplyZoomTarget` clamps the result into the configured bounds. A scroll notch multiplies by `1 + scrollZoomPercent`, and a pinch multiplies by its ratio.

Scaling makes a step proportional to how far out the camera already is, so the notches needed to cross the full range depend on the ratio between `Min Zoom Distance` and `Max Zoom Distance` rather than the raw gap between them. A fixed world-unit step reaches the same feel only at one distance, and a project with a wide zoom range crawls at the far end. The consequence is that `Min Zoom Distance` can never be zero, since zero leaves nothing to scale.

### Ground bounds
The optional X/Z rectangle constrains the **focal point**, not the camera body. An angled camera stands behind the point it looks at by roughly `zoom × horizontal(forward)`, and that gap grows with the zoom distance, so a camera pulled far out legitimately sits well outside the rectangle while still looking inside it.

`ProcessBounds` clamps the focal point and applies the difference to the transform, which keeps the correction zoom-independent. Clamping the transform directly pushes the focal point off target at high zoom, and since the focal point is re-derived each frame, the shift survives zooming back in.

### Pinch ratio
This frame's distance between two fingers divided by last frame's. `1` means idle, above `1` means they spread apart, below `1` means they came together. Screen density cancels out of the division, so the same gesture reads the same on any device, which a pixel delta cannot do.

`CameraPanController` measures this itself from the live touches rather than reading the zoom delta, since a delta carries no absolute separation to divide by. It divides the target distance by `ratio^sensitivity` and hands the result to `ApplyZoomTarget`, the same path a scroll notch takes. The pinch binding still drives the `Zoom` action; the controller discards its value while two fingers are down.

### Pinch lockout
The arbitration rule between the two gestures a camera pan controller recognises. While two or more fingers touch the screen, **pinch owns the frame** and dragging is suppressed entirely, including a drag already in progress.

Lifting back to one finger does not resume the drag. The remaining finger is not a fresh press, and its world anchor is stale from before the pinch, so panning would snap the camera. The controller instead waits for a new press.

### Primary press
A press from whichever pointing device is in use, without the consumer knowing which. Bound through the `Pointer` layout rather than per-device, so mouse left button, pen tip and first touch all arrive as the same signal.

---

## Tweening (`Utilities.Tweening`)

### Sync engine vs async engine
The tweening subsystem runs on **two independent backends**.

- **Sync engine** drives `TweenTo(...)` and `TweenToCoroutine(...)`. Lives in the core asmdef. A `HiddenSingleton<TweenController>` ticks pooled `TweenData` every `Update()`.
- **Async engine** drives `TweenToAsync(...)`. Lives in the gated asmdef `Jam-starter.Runtime.tweening.unitask`, only compiled when UniTask is installed. Each tween is its own async state machine scheduled by UniTask's `PlayerLoopTiming`.

Both engines share curve evaluation through `TweenMath`. Callers pick the backend at the call site by choosing which extension method to use. See `.agents/docs/adr/0001-dual-backend-tweening.md`.

### Tween conflict
Two tweens racing on the same `(Transform, TRANSFORM)` key. Each backend resolves its own intra-backend conflicts:

- **Sync engine** replaces the prior tween in-place by reusing the same `TweenData` entry in `TweenController`'s dictionary.
- **Async engine** cancels the prior tween by cancelling its registered `CancellationTokenSource` in `TweenRegistry`.

**Cross-backend conflicts** (a sync `TweenTo` and an async `TweenToAsync` running on the same key at the same time) are **undefined behaviour**. In `#if DEBUG` builds, the async path asserts when starting on a key that has an active sync tween.
