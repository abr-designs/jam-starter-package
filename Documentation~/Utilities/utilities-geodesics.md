---
title: Geodesics
---
# Geodesics Utilities
These utilities provide the math functions required to work with Geodesic maths & utilize them for different character 
controllers.

## Sphere Maths

> [!TIP]
> If you wanted to see the sphere math in action, you can add the **_Geodesics Movement Examples_** sample, which includes
> a character controller that uses the `SphereMaths`!

The [`SphereMaths`](../../Runtime/Scripts/Utilities/Geodesics/SphereMaths.cs) class provides a single function for moving
across the surface of a sphere.

## Torus Maths

> [!TIP]
> If you wanted to see the torus math in action, you can add the **_Geodesics Movement Examples_** sample, which includes
> a character controller that uses the Torus maths!

The [Torus](https://en.wikipedia.org/wiki/Torus) is a complex shape requiring complex maths to navigate! 

Some important concepts to understand when working with a Torus are:
- **Major Radius** _(`R`)_: The distance from the center of the tube to the center of the torus.
- **Minor Radius** _(`r`)_: The radius of the tube.
- **`(U, V)`**: This becomes the X, Y position on a torus. This is represented by a `Vector2` or two `floats`, where similar to a `Vector2`, can be both a position or direction.
  - An example use is `TorusMaths.WorldPointToTorusUV()`

Included with the math functions is a helper class [`Torus.cs`](../../Runtime/Scripts/Utilities/Geodesics/Torus/Torus.cs) 
which allows you the ability to contain the values as well as call the math functions found in `TorusMaths.cs` more 
easily _(as there are provided overloads that will use `Torus.cs`, see [`TorusMaths.extensions.cs`](../../Runtime/Scripts/Utilities/Geodesics/Torus/TorusMaths.extensions.cs))_

### `TorusMeshGenerator`

![torus-mesh-generator.png](../Images/Utilities/torus-mesh-generator.png)

You have the ability to generate a Torus shape using the [`TorusMeshGenerator.cs`](../../Runtime/Scripts/Utilities/Geodesics/Torus/TorusMeshGenerator.cs)
which is **Editor-Only** code that will create & save a torus mesh where you specify.