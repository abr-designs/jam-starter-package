# Physics
## `CollisionChecks`
> _These implementations are based on the [examples provided by Jeffery Thompson](https://www.jeffreythompson.org/collision-detection/circle-circle.php)_

This static class provides simple math solutions for determining overlap of specified shapes. These will all return a 
`bool` value
- Rectangle
  - `Rect2Rect()`
- Line
  - `Line2Line()`
  - `Line2Circle()`
  - `Line2Point()`
- Circle
  - `Point2Circle()`
- Point

## `PhysicsLauncher`
> For 3D support, use the `PhysicsLauncher3D`

This **struct** is a helper that provides a random velocity based on the parameters set by you. The intention is that this is
called elsewhere to get the information

![example](../Images/physics-launcher.png)
![physics3d_example.gif](../Images/physics3d_example.gif)

- `SpawnLocation`: This can be retrieved to use as the spawn location, but is also used to display the gizmos
- `spawnAngle`: The number of degrees in either direction that the prefab can aim in
  - ```csharp
    Quaternion.Euler(0f, 0f, Random.Range(-spawnAngle, spawnAngle)) * spawnDirection.normalized;
    ```
    

### Gizmos
There is the option to display the Gizmos for this, however, it must be called outside
```csharp
private void OnDrawGizmos()
{
    launcher.DrawGizmos();
}
```

![example](../Images/physics-launcher_example.gif)


This example will launch a new rigidbody based prefab once every second.
```csharp

[SerializeField]
private Rigidbody prefab;
[SerializeField] 
private PhysicsLauncher launcher;

private float _time;

private void Update()
{
    if (_time < 1f)
    {
        _time += Time.deltaTime;
        return;
    }
    _time = 0f;
    
    var newInstance = Instantiate(prefab, transform.position, Quaternion.identity);
    newInstance.velocity = launcher.GetLaunchVelocity();
}
```