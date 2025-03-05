# Trajectory

## Sample Scene
A sample scene is included that demonstrates how to work with the trajectory preview components.

## Thrower Prefab

This prefab contains all the components necessary for a small throwing example where a projectile will be thrown according to where the left mouse button is clicked.  It consistents of several components.

- `TrajectoryLine`

    This component requires a `LineRenderer` component to be on the same GameObject.  The line will be configured according to the `LaunchVelocity`, `LineResolution`, and `LinePreviewTime` properties.

- `MouseCaster`

    This component detects mouse intersections with physics colliders based on the mask provided.  The active object and hit location can be queried from the component.

- `MouseTrajectoryExample`

    This is an example component that glues together `TrajectoryLine` and `MouseCaster` components.  Based on the mouse position a velocity vectory will be calculated using `ProjectileMath.LaunchAngle`.

    Since a given point can be reached via two angles, `alwaysLob` determines whether the maximum or minimum <i>(default)</i> angle will be used.

- `ProjectileSpawnerExample`

    This is an example component that maintains a pool of projectile objects (spawned from a prefab).  Left mouse button down events will cause a projectile to be fired based on the current `TrajectoryLine.LaunchVelocity`.
