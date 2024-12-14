# Extensions
## Collections Extensions
### `PickRandomElement()`
This function lets you select a random element from a provided list or array. It is not limited by type, so can return any
entry from a provided list.
```csharp
void GetElementFromList()
{
    List<int> numberList = new List<int>()
    {
        2, 7, 4, 10, 22
    };
    int randomNumber = numberList.PickRandomElement();
}
```
```csharp
void GetElementFromArray()
{
    int[] numberArray = new int[]
    {
        2, 7, 4, 10, 22
    };
    
    int randomNumber = numberArray.PickRandomElement();
}
```