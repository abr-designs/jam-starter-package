# Utilities

- ### [Animators](Utilities/utilities-animators.md)
- ### [Wait-For Animations](Utilities/utilities-wait-animation.md)
- ### [Physics](Utilities/utilities-physics.md)
- ### [Extensions](Utilities/utilities-extensions.md)
- ### [Debugging - Draw.cs](Utilities/utilities-draw.md)

### `HiddenSingleton`
This class is used for any object that should act as a singleton but you may want to limit access to. For example, the
[LevelLoader]() uses the `HiddenSingleton`

Example of how you would use the Hidden Singleton. Instance is accessible from the class itself, but hidden outside.
```csharp
public class LevelLoader : HiddenSingleton<LevelLoader>
{
    public static bool LoadNextLevel() => Instance.TryLoadNextLevel();
    public static void Restart() => Instance.RestartLevel();
    public static void LoadFirstLevel() => Instance.LoadLevel(0);
}
```

### `ScreenFader`

This class is used to fade the screen in&out, such as when you're wanting to change scenes. 

> _**NOTE** It should be attached to a `Canvas`, that contains an `Image`. Ideally, you would want it to remain in the 
> scene at all times using `DontDestroyOnLoad()`_

This operates with Coroutines, and can be awaited. If not called from a Coroutine, you can use the `OnComplete` callbacks.
- `FadeOut()`: Call this coroutine if you want to fade from Clear to Black
- `FadeIn()`: Call this coroutine if you want to fade from Black to Clear
- `ForceSetColorBlack()`: Force sets the color of the image
- `ForceSetColorClear()`: Force sets the color of the image

### `[ReadOnly]` Attribute
You can add this attribute to any serialized field _(For non-custom structs)_ and it will prevent the value from being edited
while keeping it visible, useful for debugger, or allowing designers to see what is happening.

![readonly attribute](Images/readonly.PNG)
```csharp
[SerializeField, ReadOnly, Min(0)]
private float speed;
private float _current;

[ReadOnly]
public int test;
```

### `MouseCaster`
> _**NOTE** This is for 3D space only currently._

The `MouseCaster` provides a simple utility to cast a ray from the camera position out, based on a distance & layermask 
provided.
![mouse-caster_example.gif](Images/mouse-caster_example.gif)

You are able to access `MouseCaster.HitObject` to read what the mouse is currently hovering

![mouse-caster.PNG](Images/mouse-caster.PNG)
