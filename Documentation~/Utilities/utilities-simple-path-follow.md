# Simple Path Follow
The SimplePathFollow component is a utility script that allows you to connect Transforms, to animate an object along a path.

![path-smooth-animation.gif](../Images/path-smooth-animation.gif)

| Linear                                        | Smooth                                                     |
|-----------------------------------------------|------------------------------------------------------------|
| The path is traversed linearly.               | The path is traversed with a smooth curve _(Catmull-Rom)_. |
| ![path-linear.png](../Images/path-linear.png) | ![path-smooth.png](../Images/path-smooth.png)              |

### Speed & Direction
You are able to control the speed of the path traversal, but also the direction, by using either a negative or positive speed.

### Looping
There is also the option to loop the path, using the `looping` toggle on the inspector. Looping changes how the 
traversal happens by default, as the non-looping option will have the transform ping-ponging back and forth between the start and end points.

![path-linear-animation.gif](../Images/path-linear-animation.gif)

### Editing the path
> [!NOTE]
> On the inspector, you are able to add points to the path by clicking `Add Point`.

The path uses a list of `Vector3` values. These values are converted from local space to world space, so that the rotation
and position of the parent object is taken into account. When the component object is selected, handles will appear on the 
path where the points are located. You are able to drag these handles to change the position of the points.

![path-handles.gif](../Images/path-handles.gif)