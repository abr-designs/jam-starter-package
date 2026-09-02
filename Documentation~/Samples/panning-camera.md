---
title: Panning Camera
---
# Panning Camera

A top-down camera you drag around with the mouse or a finger, and zoom with the scroll wheel or a two-finger pinch. The ground point you grab stays under the pointer for the whole drag, so the camera follows your hand rather than a fixed speed.

![panning-camera.gif](../Images/Samples/panning-camera.gif)

## Controls

| Action | Mouse & Keyboard | Touch |
|---|---|---|
| Pan | Hold left mouse button, move | Drag one finger |
| Zoom | Scroll wheel | Pinch two fingers |
| Move _(optional)_ | `WASD` / arrow keys | Not available |

A press only becomes a pan once the pointer travels past **Drag Threshold** pixels, so taps and clicks pass through untouched.

While two fingers are down, the pinch owns the gesture and panning stops. Lifting back to one finger does not resume the pan; the next press starts a fresh one. This stops the camera snapping to a stale grab point.

Keyboard movement is off by default and requires the [Game Input](game-input.md) sample.

## Setup

1. Import the sample from the **_Package Manager window → Jam Starter Kit → Samples_**.
2. Drop the `-- Panning Camera --` prefab into your scene and delete the existing Main Camera.
3. Angle the camera down towards the ground.

Everything the camera does is measured against the **ground plane at world Y = 0**. Panning raycasts the pointer onto it, and zoom moves the camera along its forward axis towards the point it is looking at.

## Inspector

![panning-camera-inspector.png](../Images/Samples/panning-camera-inspector.png)

### Camera Pointer Drag

| Field | Does |
|---|---|
| Drag Threshold | Screen pixels the pointer must travel before a press counts as a pan. |

### Camera Focus Move

| Field | Does |
|---|---|
| Look At Focus Time | Seconds the camera takes to ease onto a new focus point. |

### Zoom Bounds

| Field | Does |
|---|---|
| Zoom Smooth Time | Smoothing applied to the zoom. Lower is snappier. |
| Min Zoom Distance | Closest the camera gets to the ground. Also the starting distance. Cannot be zero, since zoom scales the distance rather than adding to it. |
| Max Zoom Distance | Furthest the camera pulls back. |
| Scroll Zoom Percent | Fraction of the current distance added or removed per scroll wheel notch. `0.1` is 10% per notch. |
| Pinch Sensitivity | How closely zoom tracks the pinch. `1` moves the camera the same percentage the fingers moved, `0.5` is softer, `2` is stronger. |

Both sources scale the zoom distance by a percentage rather than adding a fixed number of world units. A notch covers a few units up close and a large sweep far out, so crossing a wide `Min` to `Max` range takes the same handful of notches as a narrow one. At `0.1`, the default `10` to `30` range spans roughly 11 notches.

Scroll and pinch keep separate tuning values because their inputs arrive in different units. A notch is a discrete step. A pinch is read as a ratio between the fingers' current and previous separation, which keeps the gesture feeling the same on a dense phone screen as on a coarse one.

### Keyboard Movement

| Field | Does |
|---|---|
| Use Keyboard Movement | Enables `WASD` / arrow key movement. Requires the [Game Input](game-input.md) sample. |
| Move Speed | World units per second at the closest zoom. Grows with the zoom distance, by the amount Move Speed Zoom Scale sets. |
| Move Speed Zoom Scale | How strongly Move Speed follows the zoom distance. `1` holds the same screen-space speed at any range, `0.5` softens the ramp, `0` keeps Move Speed fixed. |
| Settle Time | Seconds the camera takes to reach 99% of Move Speed. Lower is snappier, and the result is framerate independent. |

Dragging needs no equivalent setting. It pins the ground point you grabbed under the pointer, so it already covers more world distance per pixel the further out the camera is.

### Bounds (Optional)

| Field | Does |
|---|---|
| Use Bounds | Keeps the ground point the camera looks at inside the X and Z range. |
| X Bounds | Minimum and maximum world X. |
| Z Bounds | Minimum and maximum world Z. |

Bounds constrain the point the camera is aimed at rather than where the camera body sits. An angled camera stands well behind that point, and the gap grows with the zoom distance, so clamping the body would drag the focus along as you pull back and leave it moved once you zoom in again.

Bounds draw as a yellow rectangle on the ground plane while the camera is selected, with a marker on the point currently being looked at.

## Input System support

The sample works with either input backend and picks one on its own:

- **[Game Input](game-input.md) sample imported** — reads zoom, press and keyboard movement through `GameInputDelegator`, covering mouse, pen and touch.
- **Not imported** — falls back to the legacy Input Manager, giving mouse and touch pan plus scroll and pinch zoom. Keyboard movement is unavailable on this path.

## Reacting to a drag

`CameraPanController` raises a static event whenever a pan starts or stops. Use it to suppress world clicks so dragging the camera does not also select whatever is under the pointer.

```csharp
private void OnEnable()
{
    CameraPanController.OnDragStateChanged += OnDragStateChanged;
}

private void OnDisable()
{
    CameraPanController.OnDragStateChanged -= OnDragStateChanged;
}

private void OnDragStateChanged(bool isDragging)
{
    selectionHandler.enabled = !isDragging;
}
```
