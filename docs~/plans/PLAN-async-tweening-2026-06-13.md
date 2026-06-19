# PLAN: Async Tweening Backend (Optional UniTask)

**Date:** 2026-06-13
**Status:** Complete
**Session:** async-tweening

## Summary

Add a UniTask-powered async tween backend that lives alongside the existing `TweenController` engine. Public sync (`TweenTo`) and coroutine (`TweenToCoroutine`) APIs stay on the existing engine, untouched. A new `TweenToAsync` family in a gated assembly returns `UniTask`, exposes `PlayerLoopTiming`, and supports native `CancellationToken` cancellation.

This is the **F1** architecture: two independent backends, two independent API surfaces, end-user picks at the call site.

## Goals

- Existing `TweenTo` and `TweenToCoroutine` call sites continue to work with no changes
- New `TweenToAsync` overloads return awaitable `UniTask` values
- New async overloads only compile when UniTask is installed (optional dependency)
- Async engine supports `PlayerLoopTiming` (Update / FixedUpdate / LateUpdate / etc.)
- Async engine cancels via `CancellationToken`, throwing `OperationCanceledException`
- Shared math between both engines so curve evaluation cannot diverge

## Non-Goals

- Replacing the sync engine
- Making UniTask a hard dependency of the core package
- `FromToAsync`, `PunchScale`, `Shake` overloads (deferred to future PRs)
- Cross-backend conflict prevention beyond a dev-time assertion
- Unscaled-time support on the async engine (deferred, signposted with TODO)
- GC-allocation regression tests (deferred to a perf-focused PR)

## Architecture

### Two backends, one shared math module

```
Runtime/Scripts/Utilities/Tweening/
├── CURVE.cs                          (unchanged)
├── TweenMath.cs                      NEW   curve evaluation, shared by both engines
├── TweenController.cs                MOD   add HasActiveTween(), delete AsAsncTask() stub
├── TransformTweenExtensions.cs       (unchanged sync & coroutine API)
└── UniTask/
    ├── Jam-starter.Runtime.tweening.unitask.asmdef   (existing, define-gated)
    ├── TweenRegistry.cs                              NEW   async-side conflict registry
    └── TransformTweenExtensions.UniTask.cs           NEW   async overloads
```

`[assembly: InternalsVisibleTo("Jam-starter.Runtime.tweening.unitask")]` lives in a new `Runtime/Scripts/Utilities/Tweening/AssemblyInfo.cs` so the gated asmdef can reach `TweenController.HasActiveTween()` and `TRANSFORM`.

### Async engine model

Each `TweenToAsync` call is its own async state machine. There is no central controller for the async path. The loop is:

```csharp
public static async UniTask TweenToAsync(this Transform t, SPACE space, Vector3 target,
    float duration, CURVE curve = CURVE.LINEAR,
    PlayerLoopTiming timing = PlayerLoopTiming.Update,
    CancellationToken ct = default)
{
    if (t == null) return;

    #if DEBUG
    Debug.Assert(!TweenController.HasActiveTween(t, TRANSFORM.MOVE),
        $"[Tweening] TweenToAsync started on '{t.name}' while a sync tween for MOVE is active. " +
        "Mixing sync & async tweens on the same transform+transformation is undefined behaviour.");
    #endif

    var cts = TweenRegistry.Register(t, TRANSFORM.MOVE, ct);
    try
    {
        var start = space == SPACE.LOCAL ? t.localPosition : t.position;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            // TODO(unscaled-time): expose ignoreTimeScale if pause-aware tweens become a need.
            await UniTask.Yield(timing, cts.Token);
            if (t == null) return;
            elapsed += Time.deltaTime;
            var k = TweenMath.GetCurveT(curve, Mathf.Clamp01(elapsed / duration));
            var p = Vector3.Lerp(start, target, k);
            if (space == SPACE.LOCAL) t.localPosition = p;
            else t.position = p;
        }
        if (t != null)
        {
            if (space == SPACE.LOCAL) t.localPosition = target;
            else t.position = target;
        }
    }
    finally
    {
        TweenRegistry.Release(t, TRANSFORM.MOVE, cts);
    }
}
```

The Rotate and Scale overloads follow the same shape.

### `TweenRegistry`

Static `Dictionary<int, CancellationTokenSource>` keyed by `HashCode.Combine(transform, (int)transformation)`. On `Register`, an existing CTS for that key is cancelled & disposed. The returned CTS is a linked source (`CreateLinkedTokenSource(externalCt)`) so both caller cancellation and preemption from a newer tween flow through the same token used inside the loop.

## Decisions

| # | Decision | Rationale |
|---|----------|-----------|
| Q1 | Use `AutoResetUniTaskCompletionSource`-style awaitable, NOT custom `IUniTaskSource` | UniTask already pools the state machine; rebuilding the source protocol is a maintenance liability |
| Q2a | Cancellation throws `OperationCanceledException` | Idiomatic UniTask, `.SuppressCancellationThrow()` is the documented opt-out |
| Q2b | Tween freezes at current value on cancel | Matches DOTween default `Kill()` semantics, lowest-surprise |
| Q3 | `onCompleted` callback does NOT fire on cancel | Cancellation is signalled by the OCE throw, not the completion callback |
| Q4 | F1: two independent backends, two API surfaces | Lowest coupling, end-user picks explicitly at the call site |
| Q5 | UniTask stays optional via gated asmdef | Honours the existing `defineConstraints: ["UNITASK"]` posture |
| Q6 | Cross-backend conflicts documented, NOT prevented | Cheap, honest, matches DOTween-vs-external-tween conventions |
| Q7a | Dev-time conflict check uses `#if DEBUG` | Production builds pay zero cost for a programmer-error check |
| Q7b | Dev-time conflict check uses `Debug.Assert` | Stops misuse early; severity matches the footgun |
| Q8a | Default `PlayerLoopTiming.Update` | Matches sync engine timing; lowest surprise |
| Q8b | `CancellationToken` defaulted | Callers who do not need cancellation pay no syntactic cost |
| Q8c | Suffix naming: `TweenToAsync`, `TweenScaleToAsync` | Standard .NET / UniTask convention |
| Q9 | Always use scaled `Time.deltaTime`; TODO comment marks the opt-in path | Strict parity with sync engine for now; cheap to add later |
| Q10 | Async-side registry uses linked CTS | Single cancellation token inside the loop carries both caller intent & preemption |
| Q11 | Strict parity (Move / Rotate / Scale only) for this PR | Keeps the PR reviewable; extras land as focused follow-ups |
| Q12 | Five PlayMode tests, no GC-alloc tests yet | One test per contract decision; GC tests belong in a perf PR |

## Public API

```csharp
namespace Utilities.Tweening
{
    public static class TransformTweenExtensions  // existing, unchanged
    {
        public static void TweenTo(this Transform, SPACE, Vector3, float, CURVE, Action);
        public static IEnumerator TweenToCoroutine(this Transform, SPACE, Vector3, float, CURVE, Action);
        public static void TweenTo(this Transform, SPACE, Quaternion, float, CURVE, Action);
        public static IEnumerator TweenToCoroutine(this Transform, SPACE, Quaternion, float, CURVE, Action);
        public static void TweenScaleTo(this Transform, Vector3, float, CURVE, Action);
        public static IEnumerator TweenScaleToCoroutine(this Transform, Vector3, float, CURVE, Action);
    }

    // New, gated assembly
    public static class TransformTweenExtensionsAsync
    {
        public static UniTask TweenToAsync(this Transform, SPACE, Vector3, float,
            CURVE = CURVE.LINEAR, PlayerLoopTiming = PlayerLoopTiming.Update, CancellationToken = default);
        public static UniTask TweenToAsync(this Transform, SPACE, Quaternion, float,
            CURVE = CURVE.LINEAR, PlayerLoopTiming = PlayerLoopTiming.Update, CancellationToken = default);
        public static UniTask TweenScaleToAsync(this Transform, Vector3, float,
            CURVE = CURVE.LINEAR, PlayerLoopTiming = PlayerLoopTiming.Update, CancellationToken = default);
    }
}
```

## Tests (PlayMode)

All under `Tests/PlayMode/Tweening/`:

1. `TweenToAsync_AwaitCompletes_AtTargetPosition`
2. `TweenToAsync_TokenCancelled_ThrowsOCE_FreezesAtCurrentValue`
3. `TweenToAsync_TwoOnSameKey_PriorCancelledByRegistry`
4. `TweenToAsync_TransformDestroyed_ReturnsCleanly`
5. `TweenToAsync_WithActiveSyncTween_DebugAssertFires` (uses `LogAssert.Expect`)

## Files Modified / Added

### New
- `Runtime/Scripts/Utilities/Tweening/TweenMath.cs`
- `Runtime/Scripts/Utilities/Tweening/AssemblyInfo.cs`
- `Runtime/Scripts/Utilities/Tweening/UniTask/TweenRegistry.cs`
- `Runtime/Scripts/Utilities/Tweening/UniTask/TransformTweenExtensions.UniTask.cs`
- `Tests/PlayMode/Tweening/TransformTweenExtensionsAsyncTests.cs`
- `docs~/adr/0001-dual-backend-tweening.md`

### Modified
- `Runtime/Scripts/Utilities/Tweening/TweenController.cs` (add `HasActiveTween`, remove `AsAsncTask()` stub, move `GetCurveT` to `TweenMath`)
- `CONTEXT.md` (add Tweening section)
- `CHANGELOG.md` (Added / Changed / Removed bullets)

## Out of Scope (Future Work)

- `TweenFromToAsync` overloads with explicit start values
- `PunchScale`, `Shake` flair tweens
- `ignoreTimeScale` on the async engine
- GC-allocation regression tests
- Sample scene demonstrating async sequencing with `UniTask.WhenAll`
- Documentation page in `Documentation~/` describing the dual-backend model
