---
title: TimeScale Toolbar
---
# TimeScale Toolbar

> **WARNING This requires Unity 6.3 or higher!**

Adds a `Time.timeScale` slider and a reset button to the Unity main toolbar, so you can slow down or speed up play mode without leaving the editor. The elements dock to the middle of the toolbar, beside the play controls.

Built on Unity's [`MainToolbarElement`](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Toolbars.MainToolbarElementAttribute.html) API.

## Using the slider

Drag the slider to set `Time.timeScale`. The current value is printed at the right end of the slider. To type an exact value, double-click the slider or pick `Edit` from its context menu.

The value persists between editor sessions and is reapplied when you enter play mode.

Select `Reset` to return `Time.timeScale` to `1`.

The slider is drawn by Unity as a filled bar rather than a track with a handle. That appearance comes from the toolbar API and cannot be changed by the package.

## Configuration

Right-click either element to open its context menu.

| Item | Effect |
|---|---|
| `Forced Override` | While checked, the toolbar value takes priority and is written back whenever game logic changes `Time.timeScale` |
| `Max Scale` | Upper bound of the slider. Choose `2`, `5`, `10`, or `100` |

Unity supplies its own entries in the same menu: `Edit`, `Copy`, and `Paste` on the slider, and `Hide` on both elements.

Lowering `Max Scale` below the current value clamps the value down to the new maximum.

## Following game logic

While `Forced Override` is off, changes made to `Time.timeScale` from game logic take priority, and the slider updates to follow them. That tracking is throttled to roughly ten updates per second, since each one rebuilds the toolbar element.

## Settings storage

Values are kept in `EditorPrefs` under the keys `TimeScaleToolbar_TimeScale`, `TimeScaleToolbar_ForcedOverride`, and `TimeScaleToolbar_Max`. `EditorPrefs` is shared across every Unity project on the machine, so these act as personal preferences rather than per-project settings.

## Credits

Based on [TimeScale Toolbar](https://assetstore.unity.com/packages/tools/utilities/timescale-toolbar-291564) by Paul Berne, MIT licensed.
