---
title: Singletons
---
# Singletons
The [Singleton pattern](https://refactoring.guru/design-patterns/singleton) solves two problems at the same time, violating the Single Responsibility Principle:
1. Ensure that a class has just a single instance.
2. Provide a global access point to that instance.

Our implementation comes in two flavours:

### `Singleton<T>`
> [!CAUTION]
> Be aware that if your implementation of `Singleton<T>` overrides `Awake()` you will have issues!

This implementation is the most traditional, where the instance is generated & is publicly retrievable.
- [`[DefaultExecutionOrder(-10000)]`](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/DefaultExecutionOrder.html) ensures that this is executing with a higher priority

```csharp
[DefaultExecutionOrder(-10000)]
public class Singleton<T> : MonoBehaviour where T : Object
{
    public static T Instance 
    { 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get; 
        private set; 
    }
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"Attempted to create Multiple instances of {typeof(T).Name}");
            Destroy(gameObject);
            return;
        }
        Instance = this as T;
    }
}
```

### `HiddenSingleton<T>`
This is my preferred implementation of a Singleton. This allows the behaviour to remain intact, but reduces outside access as much as possible.

This is most useful when you just want to call a classes static function, but it still requires a scene instance to be
present to maintain inspector references. _I do believe that a singleton pattern is not a great approach, but sometimes it
is required._

#### Example `SFXManager.cs`
```csharp
//This function is only visible to users!
public static void PlaySound(SFX sfx, float volume = 1f)
{
    Assert.IsNotNull(Instance, $"Missing the {nameof(SFXManager)} in the Scene!!");
    //Travels through this static function to call _PlaySound()
    Instance._PlaySound(sfx, volume);
}

//private function only on the instance itself for data access
private void _PlaySound(SFX sfx, float volume)
{
    //[...]    
}
```