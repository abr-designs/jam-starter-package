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
