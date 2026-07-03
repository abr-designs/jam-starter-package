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
