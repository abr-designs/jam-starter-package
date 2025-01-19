# `Draw.cs`
This class extends the existing Debug.Draw methods to include
## Label

![example](../Images/draw-label.png)

This can only be called from [`OnDrawGizmos()`](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/MonoBehaviour.OnDrawGizmos.html)
or [`OnDrawGizmosSelected()`](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/MonoBehaviour.OnDrawGizmosSelected.html) as this uses the UnityEditor.Handles class to draw
the text.
- `Offset`: This represents the distance the text should move to the right. _You can apply a negative number to move the
  label to the left._
```csharp
private void OnDrawGizmos()
{
    Draw.Label(Vector3.zero, "Vector3.zero");
    Draw.Label(Vector3.up, "Vector3.up", offset: 0f);
    Draw.Label(Vector3.up, "Vector3.up", offset: 1f);
}
```
## Circle
> _**NOTE** This currently only faces the forward direction_

![example](../Images/draw-circle.png)

```csharp
Draw.Circle(Vector3.zero, Color.green);
Draw.Circle(Vector3.zero, Color.blue, radius: 0.5f);
Draw.Circle(Vector3.zero, Color.red, radius: 1.5f, segments: 32);
```
## Arrow

![example](../Images/draw-arrow.png)

```csharp
Draw.Arrow(Vector3.zero, direction: Vector3.up, Color.green);
Draw.Arrow(Vector3.zero, direction: Vector3.forward, Color.blue);
Draw.Arrow(Vector3.zero, direction: Vector3.right, Color.red);
```