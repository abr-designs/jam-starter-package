# Recycling

This utility allows you to Recycle/Pool objects that may be instantiated many times while in game.

This can include things like:
- Bullets
- Particle Effects
- Enemies

The Recycling system will be automatically generated its parent object _(Where the GameObjects will be parented when recycled)_
so once you call `Recycler.Recycle()`, it will be setup for you.

To do so, there are two different strategies that you can choose from

### IRecyclable

This strategy allows you to attach an interface onto a GameObject, then by calling `Recycler.Recycle(this);` instead of 
`Destroy()` your object will be pooled.

`Recycler.Recycle<T>()` where the `<T>` is `IRecyclable`

Then to retrieve an object from the pool you can call:
```csharp
if (Recycler.TryGrab<MyCube>(null, Vector3.zero, Quaternion.identity, out var newCube) == f
    newCube = Instantiate(prefab);
```
> **_NOTE_** A `bool` will be returned to indicate if it was possible to retrieve a `GameObject` of the specified type.

The benefits of using `IRecyclable` is that it will track the following _(code below)_, where the `OnRecycled()` will be called
right before the `GameObject` is disabled, providing the opportunity to clean the object state.
```csharp
public interface IRecyclable
{
    GameObject gameObject { get; }
    Transform transform { get; }
    bool IsRecycled { get; set; }
    void OnRecycled();
}
```

### Recycle by Enum

> **_NOTE_** Because the `enum` approach does not use `IRecyclable` there is no tracking of objects Recycled states, nor any callbacks
> on those objects.

There may be instances where there are nested objects, or simple GameObjects that may not need a class attached.
This means that you are able to still recycle something without incorporating `IRecyclable`.

By calling the code below, we're able to recycle the specified `GameObject`:
```csharp
Recycler.RecycleEnum(TEST.OBJECT_1, gameObject);
```

Then retrieving the object looks like this:
```csharp
if (Recycler.TryGrabEnum(TEST.OBJECT_1, null, Vector3.zero, Quaternion.identity, out GameObject myObject) == false)
    myObject = Instantiate(prefab);
```

