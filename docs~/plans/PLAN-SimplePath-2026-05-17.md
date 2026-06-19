# PLAN: SimplePath Base Class Extraction
**Date:** 2026-05-17
**Status:** Complete

## Goal
Extract path structure and sampling logic from `SimplePathFollow` into a new `SimplePath` MonoBehaviour base class. `SimplePathFollow` inherits from it and retains all motion logic.

## Files Affected
| File | Change |
|------|--------|
| `Runtime/Scripts/Utilities/Animations/SimplePath.cs` | **CREATE** — new base class |
| `Runtime/Scripts/Utilities/Animations/SimplePathFollow.cs` | **MODIFY** — inherit from `SimplePath`, remove extracted members |
| `Editor/Scripts/Utilities/Animations/SimplePathFollowEditor.cs` | **MODIFY** — re-target to `SimplePath`, rename references |

---

## Step 1 — Create `SimplePath.cs`

**Location:** `Runtime/Scripts/Utilities/Animations/SimplePath.cs`

### Visibility rules
| Member | Visibility | Reason |
|--------|-----------|--------|
| `pathPoints` | `internal` | Editor assembly access via `InternalsVisibleTo` |
| `catmullResolution` | `internal` | Editor assembly access |
| `looping` | `protected internal` | Subclass (motion logic) + Editor assembly |
| `motion` | `internal` | Editor assembly access |
| `MOTION` enum | `internal` | Editor assembly access |
| `m_arcLengthTable` | `private` | Internal only |
| `m_totalLength` | `protected` | Subclass needs for distance math |
| `Evaluate()` | `public` | Public API |
| `BakeArcLengthTable()` | `protected` | Subclass may need to rebake |
| `SamplePathByIndex()` | `private` | Internal only |
| `SamplePath()` | `private` | Internal only |
| `GetCatmullPoint()` | `internal` | Editor assembly access |
| `AddPoint()` | `internal` | Editor assembly access |
| `Awake()` | `protected virtual` | Bakes on startup; subclass can override |

### Key implementation notes
- `[assembly: InternalsVisibleTo("Jam-starter.Editor")]` moves here (remove from `SimplePathFollow.cs`)
- `Awake()` calls `BakeArcLengthTable()`
- `Evaluate(float t, out Vector3 tangent)` maps `t` → `distance = t * m_totalLength`, calls `SamplePath(distance, out tangent)`
- Tangent in `SamplePath` changes from `((b - a) * speed).normalized` → `(b - a).normalized` (speed removed from base)

---

## Step 2 — Modify `SimplePathFollow.cs`

- Change class declaration: `public class SimplePathFollow : SimplePath`
- Remove all extracted fields/methods (moved to `SimplePath`):
  - `pathPoints`, `catmullResolution`, `looping`, `motion`, `MOTION` enum
  - `m_arcLengthTable`, `m_totalLength`
  - `BakeArcLengthTable()`, `SamplePathByIndex()`, `SamplePath()`, `GetCatmullPoint()`
  - `AddPoint()`
- Remove `[assembly: InternalsVisibleTo("Jam-starter.Editor")]`
- `Start()`: remove `BakeArcLengthTable()` call (now in `SimplePath.Awake()`); keep `m_distanceTravelled = startingPosition * m_totalLength` and `m_pingPongForward = speed > 0f`
  - Call `base.Awake()` is not needed — Unity calls `Awake` on the instance automatically; just ensure `Start()` runs after `Awake()`
- `Update()`: replace `SamplePath(m_distanceTravelled, out var tangent)` with `Evaluate(m_distanceTravelled / m_totalLength, out var tangent)`
- Tangent flip: after `Evaluate(...)`, negate tangent when `speed < 0` before applying to `targetMoveTransform.forward`

---

## Step 3 — Modify `SimplePathFollowEditor.cs`

### `[CustomEditor]` attribute
```csharp
// Replace:
[CustomEditor(typeof(SimplePathFollow))]
// With:
[CustomEditor(typeof(SimplePath), editorForChildClasses: true)]
```

### Field type
```csharp
// Replace:
private SimplePathFollow m_simplePathFollow;
// With:
private SimplePath m_simplePath;
```

### `OnEnable()`
- Cast: `(SimplePathFollow)target` → `(SimplePath)target`
- `nameof(SimplePathFollow.pathPoints)` → `nameof(SimplePath.pathPoints)` *(string value unchanged)*

### `OnInspectorGUI()` / `OnSceneGUI()`
- All `m_simplePathFollow.*` → `m_simplePath.*`

### `BuildSegmentsNonAlloc()`
- `SimplePathFollow.MOTION.LINEAR` → `SimplePath.MOTION.LINEAR`
- `SimplePathFollow.MOTION.SMOOTH` → `SimplePath.MOTION.SMOOTH`

### `AddPoint(SimplePathFollow, ...)` / `DeletePoint(SimplePathFollow, ...)`
- Parameter types: `SimplePathFollow` → `SimplePath`

### `DrawGizmo`
```csharp
// Replace parameter type:
private static void OnSceneGUIAlways(SimplePathFollow simplePathFollow, GizmoType gizmoType)
// With:
private static void OnSceneGUIAlways(SimplePath simplePath, GizmoType gizmoType)
```
- All `simplePathFollow.*` → `simplePath.*` inside the method
- `SimplePathFollow.MOTION.*` → `SimplePath.MOTION.*`

### `PathRegistry`
```csharp
// Replace:
private static List<SimplePathFollow> s_paths
// With:
private static List<SimplePath> s_paths
// And FindObjectsByType<SimplePathFollow> → FindObjectsByType<SimplePath>
// Public property type: List<SimplePathFollow> → List<SimplePath>
```

### `PathHandleGlobal`
- Loop variable type: `SimplePathFollow` → `SimplePath`

---

## Acceptance Criteria
- [x] `SimplePath` compiles standalone with no motion fields
- [x] `Evaluate(float t, out Vector3 tangent)` returns correct world position and forward tangent for both LINEAR and SMOOTH modes
- [x] `SimplePathFollow` behaviour is identical to pre-refactor for a moving object
- [x] Tangent is correctly negated in `SimplePathFollow` when `speed < 0`
- [x] Gizmos draw for both `SimplePath` and `SimplePathFollow` components in the scene
- [x] Point add/insert/delete works in the editor for both component types
- [x] No compiler errors or warnings
