# Geodesics Samples
> [!TIP]
> These samples **_do not_** use any physics to keep the players on the surfaces, but instead use math only.

Included are some examples of utilizing [Geodesics](https://en.wikipedia.org/wiki/Geodesic) for moving players. The samples
appear together, and include an animated character and a simple vehicle.

> [!TIP]
> These samples use the old input system WASD by default, but could easily be changed to meet any other input requirements

## Sphere Movement
![sphere-movement.gif](../Images/sphere-movement.gif)

The provided example here will keep the characters up direction always pointing away from the center of the sphere.
- Ensure that you set the `SphereTransform`, `Radius` & `PlayerTransform` fields
    - Alternatively, these can be replaced with script focused versions to allow more flexibility

![sphere-move-controller.png](../Images/sphere-move-controller.png)

## Torus Movement

> [!NOTE]
> Find more information regarding the [Torus here.](../Utilities/utilities-geodesics.md)

![torus-movement.gif](../Images/torus-movement.gif)

The provided example will always have the characters up direction point away from the center co-planar axis _(The middle of the torus/donut)_.
- The character will always move along the surface at the speed specified
- The values for the `Torus` can be adjusted real-time
- A [`TorusMeshGenerator`](../../Runtime/Scripts/Utilities/Geodesics/Torus/TorusMeshGenerator.cs) component is provided!

![torus-move-controller.png](../Images/torus-move-controller.png)


