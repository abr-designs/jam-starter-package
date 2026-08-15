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
| Min Zoom Distance | Closest the camera gets to the ground. Also the starting distance. |
| Max Zoom Distance | Furthest the camera pulls back. |
| Scroll Zoom Sensitivity | World units of zoom per scroll wheel notch. |
| Pinch Zoom Sensitivity | World units of zoom per screen pixel of pinch. |

Scroll and pinch are tuned separately because their inputs arrive in different units. A notch is one step; a pinch is measured in pixels of finger separation.

### Keyboard Movement

| Field | Does |
|---|---|
| Use Keyboard Movement | Enables `WASD` / arrow key movement. Requires the [Game Input](game-input.md) sample. |
| Move Speed | World units per second at full stick or key press. |
| Smoothing | How quickly the camera reaches Move Speed. Higher is snappier, and the result is framerate independent. |

### Bounds (Optional)

| Field | Does |
|---|---|
| Use Bounds | Clamps the camera position on the X and Z axes. |
| X Bounds | Minimum and maximum world X. |
| Z Bounds | Minimum and maximum world Z. |

Bounds draw as a yellow rectangle in the Scene view while the camera is selected.

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
