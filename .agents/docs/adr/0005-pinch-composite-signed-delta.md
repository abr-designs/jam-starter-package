# ADR 0005: Pinch Composite Emits a Signed Per-Frame Delta

**Date:** 2026-08-15
**Status:** Accepted

## Context

The `GameInput` sample exposes a single `Zoom` action carrying two bindings: `<Mouse>/scroll/y` and a `PinchingComposite` over `<Touchscreen>/touch0` and `touch1`. Both feed one `float` through `GameInputDelegator.OnZoomChanged`, so a consumer such as the camera pan sample receives one number with no indication of which binding produced it.

The composite was adapted from a forum implementation that returns `distance / startDistance`, a ratio measured from the moment the gesture began. That value disagrees with a scroll wheel on every axis that matters:

| | `<Mouse>/scroll/y` | Ratio composite |
|---|---|---|
| Value space | signed delta | ratio |
| Neutral | `0` | `1` |
| Idle | `0` | `0` |
| Sign | positive or negative | never negative |
| Frame semantics | per-frame impulse | cumulative from gesture start |

Two failures followed directly. A consumer taking `Mathf.Sign(delta)` saw `+1` for every pinch, since the ratio is never negative, so pinching out zoomed in. A consumer guarding on `Mathf.Abs(delta) < 0.01f` never rejected an idle two-finger hold, since the neutral ratio is `1`.

Sharing one action across both bindings is worth keeping. It is what lets a project ship desktop and mobile zoom with one subscription and one set of tuning values.

## Decision

`PinchingComposite.ReadValue` returns the **signed change in distance between the two touches since the previous frame, in screen pixels**, computed from each touch's `delta`. Zero means idle, positive means the fingers spread apart, negative means they came together. This matches the impulse shape of a scroll binding, so a consumer can treat both identically.

Two supporting choices follow from it:

- `EvaluateMagnitude` normalises `abs(value)` against a `k_FullActuationPixels` constant, because the Input System expects actuation in the 0-1 range and uses it to disambiguate between bindings on the same action.
- The composite applies **no sensitivity of its own**, and neither does the scroll binding through a `Scale` processor. Both emit raw values and the consumer scales them. A project supporting the legacy Input Manager alongside the Input System would otherwise tune the same behaviour in two places and watch them drift.

The phase gate accepts `Began`, `Moved` and `Stationary` rather than requiring both touches to be `Moved`, so one finger can hold still while the other drives the gesture.

## Alternatives Considered

### A. Signed ratio offset (`ratio - 1`)
Keeps the cumulative gesture-relative model but fixes the neutral point and the sign. Truer to how a pinch actually feels, since the zoom tracks total finger separation rather than accumulating frame deltas. Rejected because it forces every consumer to hold gesture-start state and reset it on touch-count changes, and because it still does not match the impulse shape of a scroll wheel, so the shared action would carry two models regardless.

### B. Separate `Pinch` action
Leave the composite alone and give it its own action and its own delegator event. No semantics to reconcile at all. Rejected because it pushes the reconciliation into every consumer instead: each one grows two subscriptions and two code paths for one user-facing concept.

### C. Normalise inside `GameInputDelegator`
Keep both bindings on `Zoom` and have the delegator inspect the binding index to decide whether the incoming float is a delta or a ratio. Rejected because it makes the delegator's correctness depend on the binding order inside the `.inputactions` asset, which any inspector edit can silently reorder.

## Consequences

- The forum link at the top of `PinchingComposite.cs` now describes a different algorithm to the one below it. Readers following it will find a ratio where the code has a delta, which is the reason this record exists.
- Pinch zoom accumulates frame deltas rather than tracking absolute finger separation. Frames dropped mid-gesture lose their contribution, so a stuttering pinch zooms slightly less than a smooth one covering the same distance.
- `k_FullActuationPixels` is a tuning constant with no perfect value. It affects binding disambiguation only, never the value delivered to the consumer.
- On a device carrying both a mouse and a touchscreen, a scroll wheel actuating at the same time as a pinch wins disambiguation, because raw scroll magnitude on Windows is 120 against a pinch magnitude clamped to 1. Not reachable on the platforms this sample targets.
- Consumers must scale the raw value themselves. A consumer that forwards it unscaled will zoom roughly a hundred times too fast on a pinch.
- A pixel delta carries no screen density, so a consumer scaling it in world units per pixel gets a different gesture on every device. `CameraPanController` therefore measures the two touches itself and works from the ratio between this frame's separation and the last one, discarding the composite's value while two fingers are down. The composite keeps its delta shape, because the ratio needs the absolute separations that a shared single-float action cannot carry.
- Alternative A was rejected partly for forcing consumers to hold gesture state, and a consumer now holds it anyway. The distinction that survives is where: a per-frame baseline reset on the touch-count edge, not a gesture-start origin every reader of the action would have to reconstruct.
