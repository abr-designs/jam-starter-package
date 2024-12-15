# Interfaces

These interfaces allow for open-ended implementations of systems, where while the logic may not yet be defined, the backing
systems are able to anticipate its implementation.

## `IDisplayDialog`
This interface allows you to handle displaying dialog, but leaving the waiting logic to whoever has implemented it.

```csharp
public interface IDisplayDialog
{
    public IEnumerator DisplayDialogCoroutine(string text);
}
```

See [DialogSystem.cs]() for an example of an implementation:
```csharp
public IEnumerator DisplayDialogCoroutine(string text)
{
    if(string.IsNullOrWhiteSpace(text))
        yield break;
    
    speechBubble.SetActive(true);
    PlayAnimations();
    yield return new WaitForSeconds(1f);
    
    //Optional inclusion
    /*TMPText.text = "Hey You!";
    PlayAnimations();
    yield return StartCoroutine(WriteText(TMPText.text, waitTime));
    yield return new WaitForSeconds(1f);*/
    
    TMPText.text = text;
    PlayAnimations();
    yield return StartCoroutine(WriteText(TMPText.text, waitTime));
    yield return new WaitForSeconds(2f);
    
    TMPText.text = string.Empty;
    PlayAnimations();
    yield return new WaitForSeconds(0.5f);
    
    speechBubble.SetActive(false);
}
```

## `IDisplayResults`

This interface allows for any inheriting class to display results.
```csharp
public interface IDisplayResults
{
    Coroutine Display(Action uiDisplayReady);
}
```