# ADR 0004: TimeScale Toolbar on the MainToolbar API

**Date:** 2026-08-07
**Status:** Accepted

## Context

The package bundles a third-party TimeScale Toolbar (MIT, Paul Berne) that adds a `Time.timeScale` slider to the Unity main toolbar. It reached the toolbar through a vendored copy of Marijn Zwemmer's `ToolbarExtender`, which uses reflection to locate `UnityEditor.Toolbar`, walk its private `m_Root` visual tree, and splice an `IMGUIContainer` into the `ToolbarZoneLeftAlign` and `ToolbarZoneRightAlign` elements.

That reflection stopped working around Unity 6.4 (issue #138). Unity 6.3 introduced a supported replacement: `UnityEditor.Toolbars.MainToolbarElementAttribute`, which registers a static factory method returning `MainToolbarElement` descriptors.

Three facts framed the port:

1. `package.json` declares `"unity": "6000.5"` and the README states 6000.5+ support. Every officially supported version therefore has the new API.
2. Pre-Unity-6 users are served by the `unity/version-support/pre-6000` branch, which has been frozen since 2025-02-24 and has never received a merge from `main`.
3. `MainToolbarElement` is a descriptor, not a live widget. `MainToolbarSlider` stores `value`, `minValue`, and `maxValue` in readonly fields and builds its `EditorToolbarSlider` from them inside `CreateElement()`. `displayed`, `enabled`, and `content` do have setters, but they are only read during `Rebuild()`. Either way the only way to change what the toolbar shows is `MainToolbar.Refresh(path)`, which re-invokes the factory method.

## Decision

Rewrite the tool against `MainToolbarElement` and delete the legacy path entirely. Three linked choices follow.

### No version fallback

`ToolbarExtender` and its `#if UNITY_2019_1_OR_NEWER` ladder are deleted rather than kept behind a `UNITY_6000_3_OR_NEWER` guard. Under the declared 6000.5 floor a legacy branch can never compile in, and the frozen `pre-6000` branch does not inherit code from `main`, so guarded legacy code would be unreachable in both places it could plausibly serve.

Deleting `ToolbarExtender` also removes the `TimeScaleToolbar.Editor` assembly, which would otherwise hold one file and an empty reference list. The tool folds into `Jam-starter.Editor` at `Editor/TimeScaleToolbar/`, matching the `Editor/KennySpriteSlicer/` precedent.

### Configuration lives in the element context menu

The `SettingsProvider` at `Project/TimeScaleToolbarSettingsProvider` is deleted. `Forced Override` and `Max Scale` move to `MainToolbarElement.populateContextMenu`, reached by right-clicking the element.

Three of its five settings had no destination. `Toolbar Position` mapped to `defaultDockPosition`, which is an attribute argument fixed at compile time. `Position Offset` was pixel spacing that means nothing once the toolbar owns layout. `Enabled` controlled visibility, and Unity now injects its own hide item into every element's context menu.

That left two settings, which is thin justification for a Project Settings page on a tool whose value is being one click away. `Max Scale` becomes a preset submenu (2, 5, 10, 100) rather than an arbitrary float.

### State changes flow through Refresh

Because element values are construction-only, the factory method is the single place state is read, and every state change ends in `MainToolbar.Refresh(path)`.

The old code polled inside `OnToolbarGUI`, comparing `Time.timeScale` against its stored value on every IMGUI repaint. Retained-mode elements have no repaint hook, so that poll moves to `EditorApplication.update`.

This makes drift adoption expensive in a way it was not before. An IMGUI redraw was free; a `Refresh` rebuilds a `VisualElement`. Game logic that animates `Time.timeScale` (a hit-stop lerp, for example) would otherwise rebuild the toolbar every frame. Adoption is therefore gated on both a drift epsilon of `0.001` and a minimum interval of 100ms between refreshes.

## Alternatives Considered

### A. Keep both paths behind `UNITY_6000_3_OR_NEWER`
The shape the issue originally implied. Rejected: the legacy half is unreachable at the declared support floor, and the one branch that could use it does not merge from `main`. Keeping it would preserve two code paths to reason about in exchange for no reachable behaviour.

### B. Lower the package floor to 6000.0 so the legacy path is reachable
Would make the guard honest. Rejected: it reverses the dependency work in commit `11eafda` that deliberately moved the package to 6.5.

### C. Move the legacy tool onto the `pre-6000` branch
Clean separation. Rejected as out of scope: that branch is 18 months stale and would need its own maintenance pass to accept the code.

### D. Keep the SettingsProvider for the two surviving settings
Preserves an arbitrary float for `Max Scale`. Rejected: a two-row settings page three clicks deep, when `populateContextMenu` is the API's own answer for per-element configuration.

### E. Refresh on every detected drift, unthrottled
Simplest and always exact. Rejected: converts a free redraw into a per-frame `VisualElement` rebuild for any project that animates `Time.timeScale`.

### F. Make the toolbar always authoritative, removing the follow behaviour
No `Refresh` would ever be needed for drift. Rejected: it fights any game that scripts its own pause or slow motion, which is a normal thing for a jam project to do.

## Consequences

### Positive
- The tool uses a supported API instead of reflection into Unity internals, so it stops breaking on editor updates.
- Two vendored assemblies and roughly 280 lines of version-ladder reflection are gone.
- Configuration sits on the element it configures.
- The refresh gate and the max-scale clamp are pure functions, covered by EditMode tests in `Tests/EditMode/EditorTools/`.

### Negative
- The tool is unavailable below Unity 6.3. There is no fallback in the package.
- `Toolbar Position`, `Position Offset`, and `Enabled` are removed. Users who set them lose those preferences, and their `EditorPrefs` keys are left orphaned on disk.
- `Max Scale` accepts four presets instead of any value between 1 and 100.
- Drift tracking is capped at roughly ten updates per second, so the slider lags a fast `Time.timeScale` animation by up to 100ms.

### Neutral
- `EditorPrefs` keys for the three surviving settings are unchanged, so existing values carry across the upgrade.
- Paul Berne's MIT LICENSE stays in the folder and is credited in the source header, since the feature design and prefs keys are inherited even though the implementation is new.
- Zwemmer's `ToolbarExtender` LICENSE is removed along with its code.

## Appearance Is Not Configurable

`MainToolbarSlider` renders as a filled bar rather than a track with a draggable handle. This was raised as a look-and-feel concern and closed as out of our control.

`MainToolbarSlider.CreateElement()` builds an `EditorToolbarSlider`, whose constructor sets `fill = true` unconditionally. The instance is stored in a private field, `MainToolbar` exposes only `Refresh(string)`, and there is no styling hook. The API also has no custom-element route: `MainToolbarCustom(Func<VisualElement>)` is internal, and `MainToolbarElement.CreateElement()` is `internal abstract`, so the base type cannot be subclassed from a package. All of this still holds on Unity's `master` branch.

Two alternatives were weighed and rejected. Replacing the slider with a `MainToolbarDropdown` whose popup we own would allow any appearance, at the cost of a click before every drag, which undercuts the point of a toolbar tool. Reflecting into the toolbar's visual tree to flip `fill` would restore exactly the fragility this ADR removes.

Accepting the built-in look also keeps the tool consistent with Unity's own toolbar sliders.

## Follow-Up Decisions Deferred

- Migrating `Documentation~/WebGLTemplates/` into `Documentation~/EditorTools/` and documenting `KennySpriteSlicer`, so the section holds more than one page. Tracked separately.
