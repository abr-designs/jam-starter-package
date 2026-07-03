# ADR 0001: Dual-Backend Tweening with Optional UniTask Async Path

**Date:** 2026-06-13
**Status:** Accepted

## Context

The package ships a tweening system under `Utilities.Tweening`. The existing engine is driven by a `HiddenSingleton<TweenController>` that ticks pooled `TweenData` instances every `Update()`. The public extension API exposes two return shapes per operation:

- `void TweenTo(...)` (fire-and-forget)
- `IEnumerator TweenToCoroutine(...)` (yieldable)

Game-jam projects increasingly want async/await tween composition: awaiting a tween, `UniTask.WhenAll` across several, native `CancellationToken` cancellation, choice of `PlayerLoopTiming` (Update / FixedUpdate / LateUpdate). UniTask provides those primitives natively. The package already auto-installs UniTask via `Editor/AddPackages.cs` but exposes a `defineConstraints: ["UNITASK"]` gated asmdef at `Runtime/Scripts/Utilities/Tweening/UniTask/` so projects that strip UniTask still compile.

We need a path to async tweens that:

1. Does not break existing `TweenTo` / `TweenToCoroutine` callers
2. Does not promote UniTask to a hard dependency of the core package
3. Exposes UniTask features (PlayerLoopTiming, native cancellation, awaitable composition) where they matter
4. Avoids two divergent implementations of the underlying tween math

## Decision

Adopt a **dual-backend** model:

- **Sync engine** (existing). `TweenController` + pooled `TweenData`. Drives `TweenTo` and `TweenToCoroutine`. Untouched.
- **Async engine** (new). Lives in the gated asmdef `Jam-starter.Runtime.tweening.unitask`. Each tween is its own async state machine; cancellation flows through `UniTask.Yield(timing, ct)`; conflict prevention via an internal `TweenRegistry` of linked `CancellationTokenSource` values.

Both engines share curve evaluation via a new `TweenMath` static class in the core asmdef so the math cannot drift between them.

The end-user picks at the **call site**: `TweenTo` for sync, `TweenToCoroutine` for coroutines, `TweenToAsync` for awaitable. There is no transparent routing or compile-time swap.

## Alternatives Considered

### A. Thin awaitable wrapper over `TweenController` (option B in the design session)
Use `AutoResetUniTaskCompletionSource` to expose `UniTask` from the existing engine. Smallest change, but loses `PlayerLoopTiming` choice, loses native cancellation at yield points, and the async path remains tied to the `TweenController` MonoBehaviour. Rejected because the headline UniTask features are exactly the ones a wrapper cannot deliver.

### B. Async backend transparently replaces sync backend when UniTask is present (option F2)
`#if UNITASK` blocks in core route `TweenTo` calls into the async backend. Promised "you just add UniTask and the sync API gets better." Rejected for two reasons:
1. **Asmdef topology forbids it.** Core cannot reference the gated asmdef (circular). Core cannot conditionally reference UniTask directly (asmdef `references` is unconditional). The only workaround is a runtime delegate swap, which hides the routing and surprises maintainers reading `TweenTo`.
2. **It violates "leave it up to the end user."** Behaviour changes silently with package installation state.

### C. Promote UniTask to a hard package dependency (option F3)
Cleanest architecture: one engine, one API, sync `void` and coroutine wrappers `.Forget()` and `.ToCoroutine()` over a UniTask core. Rejected because the package explicitly supports projects that strip UniTask, and the `defineConstraints` posture on the existing gated asmdef encodes that intent.

### D. Full async-native rewrite alongside sync (option C in the design session)
Same as the chosen approach, but **also** retiring the sync engine over time. Rejected for this PR: the deprecation question is bigger than "add UniTask support" and conflates two decisions. The chosen approach keeps that path open without committing to it.

## Consequences

### Positive
- No breaking changes for existing callers
- UniTask remains optional
- Async path delivers the actual UniTask features (timing, cancellation, composition)
- Shared `TweenMath` prevents math drift between engines
- Decision is explicit at the call site, easy to read in user code

### Negative
- Two engines to maintain. Bug fixes to the tween *driver* must be considered in both places (math bugs are isolated to `TweenMath`).
- Cross-backend tween conflicts on the same `(Transform, TRANSFORM)` key are **undefined behaviour**. Mitigated by a `#if DEBUG` `Debug.Assert` on the async side when starting a tween whose sync counterpart is active. The reverse direction (sync starting while async is running) is not checked, because core has no reference to the gated asmdef.
- The async engine carries its own conflict-prevention registry (`TweenRegistry`), duplicating the conceptual role of `TweenController._tweenDataDict`. Justified by F1's "no core-to-gated reference" invariant.
- Adding new operations means touching two extension files.

### Neutral
- `[assembly: InternalsVisibleTo("Jam-starter.Runtime.tweening.unitask")]` is added to the core asmdef to let the async path call `TweenController.HasActiveTween(...)`. One-way visibility; core still has no compile-time knowledge of UniTask.

## Follow-Up Decisions Deferred

- Whether to add `ignoreTimeScale` to either engine
- Whether to add `TweenFromToAsync`, `PunchScale`, `Shake`
- Whether to eventually deprecate the sync engine (ADR 000X if and when revisited)
