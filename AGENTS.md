# AGENTS.md

Guidance for AI coding agents working in this repo. This is the single source of truth; tool-specific files (`CLAUDE.md`, `.gemini/settings.json`) point here.

## Critical Operating Instructions

1. All changes made MUST be reflected with simple language into the CHANGELOG.md file
2. Working/internal docs (ADRs, plans, reviews) live in `docs~/`. The `~` suffix keeps Unity from importing the folder, so do not generate `.meta` files for it and do not rename it back to `docs/`.

## Skills

This repo ships reusable skills in `.claude/skills/`. Each is a markdown workflow you can read and follow, regardless of which agent you are. If a row below matches the user's intent, open that `SKILL.md` and follow it.

Skill bodies may name Claude Code tools (e.g. `AskUserQuestion`). If your agent lacks the named tool, perform the plain-language equivalent (just ask the user). In Claude Code these also run as slash commands.

| Skill | When | Follow |
|---|---|---|
| new-sample | User wants to add a new Sample to the package | `.claude/skills/new-sample/SKILL.md` |
| unity-tests | User wants EditMode or PlayMode tests for game logic | `.claude/skills/unity-tests/SKILL.md` |
| write-pr | User wants to draft a pull request body | `.claude/skills/write-pr/SKILL.md` |

Each skill's `SKILL.md` frontmatter holds its full trigger phrases. When you add or rename a skill, update this table to match.

## What this repo is

Unity 6 UPM package (`com.abrds.jam-starter`). Reusable game systems for game jam projects. Package dependency, not a standalone Unity project.

## Running tests

No CLI test command. Use Unity **Test Runner** (Window > General > Test Runner). CI uses GameCI to spin up Unity instance:

- **EditMode**: pure logic, no scene required
- **PlayMode**: coroutine tests, requires running player loop

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

**HiddenSingleton\<T\>**: base for all managers (LevelLoader, SFXManager, MusicController, VFXManager). Static API only; instance private. Managers live in `Samples~`, not `Runtime`.

**ScriptableObject config**: `LevelDataDefinition` stores per-level metadata. `FixedPaletteTool` stores palette as ScriptableObject in `ProjectSettings/`.

**Extension methods**: primary extension point. `TransformExtensions`, `CollectionExtensions`, `RaycastHitExtensions`, `EnumExtensions`. Tweening via `TransformExtensions.TweenTo(...)`.

**Enum-keyed lookups**: VFX, SFX, Music identified by project enums. Sample managers store prefab/clip arrays indexed by enum value.

**Conditional compilation**: optional features (e.g. `GameInputDelegator`) use `#if` guards. `AddPackages.cs` auto-adds third-party UPM packages on project open.

## Key dependencies (auto-added via AddPackages.cs)

- **NaughtyAttributes**: inspector attributes (`[ReadOnly]`, dropdowns, etc.)
- **UniTask**: async/await without allocations
- **SerializedDictionary**: serializable dictionary for inspector
- **ZLinq** (NuGet): allocation-free LINQ for runtime use

## Samples~

Full reference implementations: 2D/3D character controllers, sound system, VFX manager, dialog system, main menu, cinematic system, trajectory, geodesics, object recycling, thumbnail studio. Imported via Package Manager UI. Compile under `Samples.Runtime`. Not part of core package.

## Documentation

Docs in `Documentation~/` as markdown with YAML `title:` frontmatter. Images in `Documentation~/Images/`, max 1 MB. VitePress site at `WebsiteDocs~/` publishes on push to `main` or `develop/v*`.
