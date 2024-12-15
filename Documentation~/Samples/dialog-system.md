# Dialog System

![dialog-system_example.gif](../Images/dialog-system_example.gif)

This class is an example of how you may want to create a Dialog System using the [IDisplayDialog](../Interfaces.md)

DialogSystem does not need to be static, and can live within a scene as an instance, or theoretically, you can utilize 
multiple instances.

Below is an example of how the DialogSystem can be called:
```csharp
yield return StartCoroutine(_displayDialog.DisplayDialogCoroutine(CurrentLevel.levelScript));
```

### Displaying Text

We use a coroutine called `WriteText()` which displays text character by character. We've also connected a [TransformAnimator](../Utilities/utilities-animators.md)
to have the speech bubble move while the text is writing.

```csharp
private IEnumerator WriteText(string text, float waitTime)
{
    var wait = new WaitForSeconds(waitTime);
    var textLength = text.Length;
    
    sourceAnimator?.Loop();
    for (int i = 0; i <= textLength; i++)
    {
        TMPText.maxVisibleCharacters = i;
        soundToPlay.PlaySound();
        yield return wait;
    }
    sourceAnimator?.Stop();
}
```