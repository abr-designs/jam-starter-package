# Utilities

- ### [Animators](Utilities/utilities-animators.md)
- ### [Draw.cs](Utilities/utilities-draw.md)
- ### [Extensions](Utilities/utilities-extensions.md)

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

