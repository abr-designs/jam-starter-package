# CLAUDE.md

Guidance for Claude Code when working in this repo.

## What this repo is

Unity 6 UPM package (`com.abrds.jam-starter`). Reusable game systems for game jam projects. Package dependency — not standalone Unity project.

## Running tests

No CLI test command. Use Unity **Test Runner** (Window > General > Test Runner). CI uses GameCI to spin up Unity instance:

- **EditMode** — pure logic, no scene required
- **PlayMode** — coroutine tests, requires running player loop

CI triggers on PRs (excludes docs/WebGL changes). Coverage: assemblies matching `Jam-starter.*` and `FixedPaletteTool.Runtime`.

## Package structure

```
Runtime/         Core gameplay systems (logic, utilities, level loading)
Editor/          Editor-only tools, auto-package installer, kenny sprite slicer
Tests/
  EditMode/      NUnit tests for pure logic (extensions, math, physics)
  PlayMode/      UnityTest coroutine tests (tweening, level loading, screen fader)
FixedPaletteTool/ Self-contained subsystem for color palette management
CastVisualizer/  Physics debug visualization (bundled dependency)
Samples~/        15 optional reference implementations (not part of core package)
Documentation~/  Markdown docs source (published via VitePress to GitHub Pages)
WebsiteDocs~/    VitePress site config and content
```

## Assembly layout

| Assembly | Platform | Contains |
|---|---|---|
| `Jam-starter.Runtime` | All | Core systems |
| `Jam-starter.Editor` | Editor | Auto-package setup, editor tools |
| `FixedPaletteTool.Runtime` | All | Palette lookup at runtime |
| `FixedPaletteTool.Editor` | Editor | PNG/HEX importers, Settings UI |
| `com.abrds.jam-starter.Editor.Tests` | Editor | EditMode tests |
| `com.abrds.jam-starter.Tests` | All | PlayMode tests |

## Core architecture patterns

**HiddenSingleton\<T\>** — base for all managers (LevelLoader, SFXManager, MusicController, VFXManager). Static API only; instance private. Managers live in `Samples~`, not `Runtime`.

**ScriptableObject config** — `LevelDataDefinition` stores per-level metadata. `FixedPaletteTool` stores palette as ScriptableObject in `ProjectSettings/`.

**Extension methods** — primary extension point. `TransformExtensions`, `CollectionExtensions`, `RaycastHitExtensions`, `EnumExtensions`. Tweening via `TransformExtensions.TweenTo(...)`.

**Enum-keyed lookups** — VFX, SFX, Music identified by project enums. Sample managers store prefab/clip arrays indexed by enum value.

**Conditional compilation** — optional features (e.g. `GameInputDelegator`) use `#if` guards. `AddPackages.cs` auto-adds third-party UPM packages on project open.

## Key dependencies (auto-added via AddPackages.cs)

- **NaughtyAttributes** — inspector attributes (`[ReadOnly]`, dropdowns, etc.)
- **UniTask** — async/await without allocations
- **SerializedDictionary** — serializable dictionary for inspector
- **ZLinq** (NuGet) — allocation-free LINQ for runtime use

## Samples~

Full reference implementations: 2D/3D character controllers, sound system, VFX manager, dialog system, main menu, cinematic system, trajectory, geodesics, object recycling, thumbnail studio. Imported via Package Manager UI. Compile under `Samples.Runtime`. Not part of core package.

## Documentation

Docs in `Documentation~/` as markdown with YAML `title:` frontmatter. Images in `Documentation~/Images/`, max 1 MB. VitePress site at `WebsiteDocs~/` publishes on push to `main` or `develop/v*`.
