# Transform Tweening
Using extension methods, there are now some new Tween functions that allow you to move the Transform via a simple function

By calling `Transform.TweenTo()` _(or `TweenToLocal()`, or `TweenScaleTo()`)_ you are able to animate an object while 
remaining hands off! When you call TweenTo, a `TweenController` will be created in the Scene which will be the logic centre
for all the Tweening.

### Features
- You are able to target Local or World Positions & Rotations!
- Performant functionality, that pools backend tweening data to remove Memory allocations!
  - This includes if you destroy a `GameObject` that is currently Tweening!
  - Uses the `Update()` Unity Event, without using Coroutines

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

![linear-curve.gif](../Images/linear-curve.gif)
### Ease In
This uses the [`LerpFunctions.Coserp()`](../../Runtime/Scripts/Utilities/LerpFunctions.cs)

![ease-in-curve.gif](../Images/ease-in-curve.gif)
### Ease Out
This uses the [`LerpFunctions.Sinerp()`](../../Runtime/Scripts/Utilities/LerpFunctions.cs)

![ease-out-curve.gif](../Images/ease-out-curve.gif)
### Ease In Out
This uses the [`LerpFunctions.Hermite()`](../../Runtime/Scripts/Utilities/LerpFunctions.cs)

![ease-in-out-curve.gif](../Images/ease-in-out-curve.gif)