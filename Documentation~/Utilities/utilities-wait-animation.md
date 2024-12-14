# WaitForAnimations

These scripts are designed to allow editor/inspector focused animations that can be used within coroutines, giving you
the ability to wait for sections, and to target groups of objects.

The scripts are designed to give you flexibility on what your targeting, and how the values are applied.

## Example

In the example below you can see that an animation for a start button plays in one direction, then will return the other once called.

```csharp
if (startButton)
{
    SFX.SHIFT.PlaySound();
    yield return startButton.DoAnimation(animationTime, ANIM_DIR.TO_START);
}

yield return new WaitUntil(() => _startButton.InputValue >= 1f);

if (startButton)
{
    SFX.SHIFT.PlaySound();
    startButton.DoAnimation(animationTime, ANIM_DIR.TO_END);
}

if (tvScreenAnimation)
    tvScreenAnimation.DoAnimation(animationTime, ANIM_DIR.TO_START);
```

### Animation Directions
These directions are used when deciding how to animate a target
- `START_TO_END`
- `END_TO_START`

## `IWaitForAnimation`
All of the WaitFor animation scripts will inherit from `IWaitForAnimation` which means it can be easily called without 
needing to know the contents of how the script operates.

Ideally, when traversing through a `Coroutine` _(For a cutscene, for example)_, you can call `IWaitForAnimation.DoAnimation()`
and all you have to do is wait for it to complete.

## `WaitForAnimationBase`
When inheriting from this class you will need to specify 2 value types:
- `TR`: the type of transform that will be the target of the animation _(`Transform`, `RectTransform`)_
- `T`: The value that will be used when Setting Values & Lerping _(`Vector3`, `Vector2`, `float`)_

Any new animations should inherit from this class, as it provides the essentials to operate.
You will need to provide the content for the following, when creating a new script:
- `DoAnimation()`
- `Lerp()`
- `SetValue()`
Since you may want different animations to affect different properties or items, these will let you control that.
Starting values are provided:
- `startingValue`: The t value between `0.0-1.0` that you want these objects to start at when loading
- `curve`: The animation curve that will be evaluated when calling `Lerp()`
- `objectsToAnimate`: You can pass a collection of `AnimationData` which will target many objects

### `WaitForMoveAnimations`
Below showcases the requirements for setting a position
```csharp
public override Coroutine DoAnimation(float time, ANIM_DIR animDir)
{
    return StartCoroutine(DoAnimationCoroutine(time, animDir));
}
protected override Vector3 Lerp(Vector3 start, Vector3 end, float t)
{
    return Vector3.Lerp(start, end, t);
}
protected override void SetValue(AnimationData data, Vector3 value)
{
    data.transform.position = value;
}
```
### WaitForScaleAnimations

`public class WaitForScaleAnimations: WaitForAnimationBase<Transform, Vector3>`

```csharp
public override Coroutine DoAnimation(float time, ANIM_DIR animDir)
{
    return StartCoroutine(DoAnimationCoroutine(time, animDir));
}
protected override Vector3 Lerp(Vector3 start, Vector3 end, float t)
{
    return Vector3.Lerp(start, end, t);
}
protected override void SetValue(AnimationData data, Vector3 value)
{
    data.transform.localScale = value;
}
```
### WaitForUIMoveAnimations

`public class WaitForUIMoveAnimations : WaitForAnimationBase<RectTransform, Vector2>`

```csharp
public override Coroutine DoAnimation(float time, ANIM_DIR animDir)
{
    return StartCoroutine(DoAnimationCoroutine(time, animDir));
}
protected override Vector2 Lerp(Vector2 start, Vector2 end, float t)
{
    return Vector2.Lerp(start, end, t);
}
protected override void SetValue(AnimationData data, Vector2 value)
{
    var rectTransform = data.transform;
    rectTransform.anchoredPosition = value;
}
```