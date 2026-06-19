---
title: Transform Tweening
---
# Transform Tweening
Using extension methods, there are now some new Tween functions that allow you to move the Transform via a simple function

By calling `Transform.TweenTo()` _(or `TweenScaleTo()`)_ you are able to animate an object while 
remaining hands off! When you call TweenTo, a `TweenController` will be created in the Scene which will be the logic centre
for all the Tweening.

### Features
- You are able to target Local or World Positions & Rotations using [`SPACE.WORLD`](../../Runtime/Scripts/Utilities/Enums/SPACE.cs) or [`SPACE.LOCAL`](../../Runtime/Scripts/Utilities/Enums/SPACE.cs)!
- Performant functionality, that pools backend tweening data to remove Memory allocations!
  - This includes if you destroy a `GameObject` that is currently Tweening!
  - Uses the `Update()` Unity Event, without using Coroutines
- Call a Coroutine variant using `TweenToCoroutine()`
  - This still uses the `Update()` loop update, but allows waiting when called from a coroutine
  - The added `callback` will invoke once the Coroutine is completed, to ensure execution order expectation

### Stacking Tweens
- If you call to tween a `Transform` Position & Rotation, each will be treated as separate tweens
- If you call to tween a position on a transform that is already moving
  - The original tween will be overwritten with the new tween
  - > **NOTE** _This means that if you set a Callback, the original will also be overwritten!!_

## Curves
Provided are some options for how your target transform should Lerp from A to B. By default, calling `.TweenTo()` will
use `LINEAR` but below are the current options.
### Linear
This is a basic `Mathf.Lerp()`

![linear-curve.gif](../Images/Utilities/linear-curve.gif)
### Ease In
This uses the [`LerpFunctions.Coserp()`](../../Runtime/Scripts/Utilities/LerpFunctions.cs)

![ease-in-curve.gif](../Images/Utilities/ease-in-curve.gif)
### Ease Out
This uses the [`LerpFunctions.Sinerp()`](../../Runtime/Scripts/Utilities/LerpFunctions.cs)

![ease-out-curve.gif](../Images/Utilities/ease-out-curve.gif)
### Ease In Out
This uses the [`LerpFunctions.Hermite()`](../../Runtime/Scripts/Utilities/LerpFunctions.cs)

![ease-in-out-curve.gif](../Images/Utilities/ease-in-out-curve.gif)

## Async (UniTask)
If your project has [UniTask](https://github.com/Cysharp/UniTask) installed, you also get an awaitable variant:
[`TweenToAsync()`](../../Runtime/Scripts/Utilities/Tweening/UniTask/TransformTweenExtensions.UniTask.cs) _(or `TweenScaleToAsync()`)_.
These return a `UniTask` that you can `await`, compose with `UniTask.WhenAll()`, and cancel natively. They live in a
separate assembly that only compiles when UniTask is present, so the core package keeps UniTask optional.

Unlike the sync engine, the async path has no central `TweenController`. Each call is its own async state machine driven
by UniTask's player loop, so you pick the backend at the call site by choosing which method to call.

### Features
- Choose which `PlayerLoopTiming` ticks the tween, such as `Update`, `FixedUpdate` or `LateUpdate`!
- Cancel with a `CancellationToken`!
  - Cancelling throws an `OperationCanceledException` and freezes the Transform at its current value
  - Use `.SuppressCancellationThrow()` if you would rather not handle the exception
- `await` the call instead of passing a `callback`
- Shares the same [Curves](#curves) and `SPACE.WORLD` / `SPACE.LOCAL` options as the sync engine

### Stacking Async Tweens
- If you call an async tween on a `Transform` that is already running an async tween on the same property
  - The original async tween is cancelled, throwing an `OperationCanceledException` to whoever was awaiting it
- > **NOTE** _Mixing a sync `TweenTo()` and an async `TweenToAsync()` on the same `Transform` & property at the same time is undefined behaviour. In `DEBUG` builds an assertion will fire to warn you._
  
### Async Example

Here is an example of how you might use the async tweening.

![async-tween.gif](../Images/Utilities/async-tween.gif)

![async-tween-inspector.png](../Images/Utilities/async-tween-inspector.png)

```csharp
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utilities.Enums;
using Utilities.Tweening;

public class AsyncTweenExample : MonoBehaviour
{
    [SerializeField, Min(0.1f)]
    private float tweenTime;

    [SerializeField, Min(0f)]
    private float delay;

    [SerializeField]
    private Vector3 endPositionOffset;
    [SerializeField]
    private Vector3 endScale;
    [SerializeField]
    private Vector3 endRotation;
    
    private Vector3 m_startPosition;
    private Vector3 m_startScale;
    private Quaternion m_startRotation;
    
    private CancellationTokenSource m_cancellationTokenSource;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        m_startPosition = transform.position;
        m_startScale = transform.localScale;
        m_startRotation = transform.rotation;
        
        
        try
        {
            var token = this.GetCancellationTokenOnDestroy();
            LoopMoveForeverAsync(token);
            LoopRotateForeverAsync(token);
            LoopScaleForeverAsync(token);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
    
    private async UniTask LoopMoveForeverAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var targetPosition = m_startPosition + endPositionOffset;
            
            await transform.TweenToAsync(SPACE.WORLD, targetPosition, tweenTime, CURVE.EASE_IN_OUT, PlayerLoopTiming.Update, cancellationToken);
            
            if(delay > 0f)
                await UniTask.Delay((int)(delay * 1000), DelayType.DeltaTime, PlayerLoopTiming.Update, cancellationToken, true);
            
            await transform.TweenToAsync(SPACE.WORLD, m_startPosition, tweenTime, CURVE.EASE_IN_OUT, PlayerLoopTiming.Update, cancellationToken);
            
            if(delay > 0f)
                await UniTask.Delay((int)(delay * 1000), DelayType.DeltaTime, PlayerLoopTiming.Update, cancellationToken, true);
        }
    }
    
    private async UniTask LoopRotateForeverAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var rotation = Quaternion.Euler(endRotation);
            
            await transform.TweenToAsync(SPACE.WORLD, rotation, tweenTime, CURVE.EASE_IN_OUT, PlayerLoopTiming.Update, cancellationToken);
            
            if(delay > 0f)
                await UniTask.Delay((int)(delay * 1000), DelayType.DeltaTime, PlayerLoopTiming.Update, cancellationToken, true);
            
            await transform.TweenToAsync(SPACE.WORLD, m_startRotation, tweenTime, CURVE.EASE_IN_OUT, PlayerLoopTiming.Update, cancellationToken);
            
            if(delay > 0f)
                await UniTask.Delay((int)(delay * 1000), DelayType.DeltaTime, PlayerLoopTiming.Update, cancellationToken, true);
        }
    }
    
    private async UniTask LoopScaleForeverAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            await transform.TweenScaleToAsync(endScale, tweenTime, CURVE.EASE_IN_OUT, PlayerLoopTiming.Update, cancellationToken);
            
            if(delay > 0f)
                await UniTask.Delay((int)(delay * 1000), DelayType.DeltaTime, PlayerLoopTiming.Update, cancellationToken, true);
            
            await transform.TweenScaleToAsync(m_startScale, tweenTime, CURVE.EASE_IN_OUT, PlayerLoopTiming.Update, cancellationToken);
            
            if(delay > 0f)
                await UniTask.Delay((int)(delay * 1000), DelayType.DeltaTime, PlayerLoopTiming.Update, cancellationToken, true);
        }
    }
}
```

