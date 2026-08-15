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

The value is **raw and unscaled** at the source. A scroll binding emits notches, `PinchingComposite` emits screen pixels, and neither applies a sensitivity of its own. Converting to world units is the consumer's job, so the same tuning values hold whether the project runs on the Input Manager or the Input System. See `.agents/docs/adr/0004-pinch-composite-signed-delta.md`.

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
