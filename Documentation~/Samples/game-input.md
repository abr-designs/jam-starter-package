# Game Input
> _**NOTE** This is dependent on the New Input System package being imported_

This sample provides a few out of the box functionalities:
1. Pre-made Input Actions
2. `Inputs` static class: Allows global access to the Input Actions
3. `GameInputDelegator`: Command Pattern handler of inputs
4. The prefab to handle the functionality. **Add `--- INPUT DELEGATOR ---` into your scene!**

The intention is to use the GameInputDelegator within your game, where you then listen to the Action callbacks to react elsewhere.

> _**NOTE** If you want to implement a Press & Hold functionality, it's recommended to just listen for a `true` response, then repeat your
> behaviour until another callback returns with `false`_

Below is an example implementation of a listener, where the interpretation of the left click input has already been handled,
then the surface layer only needs to know if this button is pressed or not.
```csharp
private void OnEnable() 
{
    GameInputDelegator.OnLeftClick += OnLeftClick;
}

private void OnDisable() 
{
    GameInputDelegator.OnLeftClick -= OnLeftClick;
}

private void OnLeftClick(bool isPressed) 
{
    if (isPressed) 
    {
        CheckForControlInteraction();
        cameraMouseFirstPersonRotation.enabled = false;
    }
    else 
    {
        CheckForReleaseInteraction();
        cameraMouseFirstPersonRotation.enabled = true;
    }
}

```

Below is how the Left click input is managed in the background.
```csharp
public void OnMouseLeftClick(InputAction.CallbackContext context)
{
    if (context.performed == false)
        return;
    
    if (LockInputs)
    {
        OnLeftClick?.Invoke(false);
        return;
    }
    
    var pressed = context.ReadValueAsButton();
    OnLeftClick?.Invoke(pressed);
}
```