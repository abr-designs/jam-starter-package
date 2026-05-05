# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

A Unity 6 UPM package (`com.abrds.jam-starter`) providing reusable game systems for game jam projects. It is consumed as a package dependency, not a standalone Unity project.

## Running tests

Tests run via Unity's built-in **Test Runner** (Window > General > Test Runner). There is no local CLI test command — the CI pipeline uses GameCI to spin up a Unity instance:

- **EditMode** — pure logic tests, no scene required
- **PlayMode** — coroutine-based tests requiring a running player loop

CI triggers on pull requests (excluding docs/WebGL changes). Coverage is reported for assemblies matching `Jam-starter.*` and `FixedPaletteTool.Runtime`.

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

**HiddenSingleton\<T\>** — base class for all manager types (LevelLoader, SFXManager, MusicController, VFXManager). Exposes only a static API; the instance is private. Managers live in `Samples~`, not `Runtime`.

**ScriptableObject config** — `LevelDataDefinition` stores per-level metadata. The `FixedPaletteTool` stores palette data as a ScriptableObject written to `ProjectSettings/`.

**Extension methods** — the primary extension point. `TransformExtensions`, `CollectionExtensions`, `RaycastHitExtensions`, and `EnumExtensions` wrap common operations. Tweening is exposed entirely through `TransformExtensions.TweenTo(...)`.

**Enum-keyed lookups** — VFX, SFX, and Music types are identified by project-defined enums. The sample managers store prefab/clip arrays indexed by enum value.

**Conditional compilation** — optional features (e.g. `GameInputDelegator`) guard their implementation with `#if` defines. The `AddPackages.cs` editor script auto-adds third-party UPM packages on project open.

## Key dependencies (auto-added via AddPackages.cs)

- **NaughtyAttributes** — inspector attributes (`[ReadOnly]`, dropdowns, etc.)
- **UniTask** — async/await without allocations
- **SerializedDictionary** — serializable dictionary for the inspector
- **ZLinq** (NuGet) — allocation-free LINQ for runtime use

## Samples~

`Samples~/` contains full reference implementations: 2D/3D character controllers, sound system, VFX manager, dialog system, main menu, cinematic system, trajectory, geodesics, object recycling, and thumbnail studio. They are imported into a project manually via the Package Manager UI and compile under the `Samples.Runtime` assembly. Do not treat them as part of the core package when evaluating what the package provides.

## Documentation

All user-facing docs live in `Documentation~/` as markdown with YAML `title:` frontmatter. Images go in `Documentation~/Images/` and must be under 1 MB. The VitePress site at `WebsiteDocs~/` publishes on push to `main` or `develop/v*` branches.

## Operating Principles

**CRITICAL RULES:**

1. **Efficiency First**: Write all rules, skills, and documentation with token efficiency in mind
   - Prefer inline examples over separate files
   - Create quick references before full documentation
   - Link to existing docs rather than duplicating content

2. **Documentation Discipline**: Only add documentation when explicitly requested by the user
   - ❌ DON'T proactively create README files, guides, or documentation
   - ❌ DON'T add files to the workspace unless asked
   - ✅ DO update existing documentation when making changes
   - ✅ DO follow existing patterns and reference existing docs

3. **C# Best Practices**: All Unity performance optimization workflows must follow C# best practices
   - Check inline patterns in this file first
   - Apply zero-GC rules from unity-memory-optimization-quick.md
   - Follow guidelines in unity-android-optimization.md

4. **Interactive User Interface**: Always use interactive UI elements for user choices
   - ✅ **ALWAYS** use the AskUserQuestion tool when presenting options or choices
   - ✅ **ALWAYS** explicitly state "⏳ **Waiting for your response.**" when awaiting user input
   - ✅ **ALWAYS** mark recommended options with ⭐ and label them "(Recommended)"
   - ✅ **ALWAYS** include brief explanations in each option's description field
   - ✅ **ALWAYS** provide reasoning at the end explaining why you recommend a particular option
   - ❌ DON'T list options in plain text without using AskUserQuestion
   - ❌ DON'T leave user hanging without clear indication that you need input
   - This creates a clear, clickable interface and eliminates confusion about when input is needed

   **Recommendation Format Example:**
   ```
   Options:
   - ⭐ Option A (Recommended) - Brief explanation of what this does
   - Option B - Brief explanation of alternative approach

   Reasoning: [Why Option A is recommended for this use case]

   ⏳ **Waiting for your response.**
   ```

5. **Subagent and Skill Formatting**: Always highlight subagent and skill invocations for visibility
   - ✅ **ALWAYS** format Task tool agents as: `[AGENT: AgentName]` (e.g., `[AGENT: Explore]`, `[AGENT: Plan]`)
   - ✅ **ALWAYS** format Skill tool invocations as: `[SKILL: skillname]` (e.g., `[SKILL: diagnose]`, `[SKILL: tdd]`)
   - Use this formatting whenever mentioning that a subagent or skill is being launched
   - Example: "Let me use `[AGENT: Explore]` to search the codebase for error handling patterns"
   - Example: "I'll launch `[SKILL: diagnose]` to debug this issue systematically"

6. **Effort-Calibrated Coding**: When coding tasks have ambiguous scope/quality expectations, ask about effort level before proceeding
   - ✅ **DO** ask about effort level via AskUserQuestion when:
     - Task involves creating new files/features (not just editing existing code)
     - Task mentions "implement," "build," "create," "add feature" without qualifiers like "quick," "rough," "prototype"
     - Task affects multiple files or systems
     - Task involves architecture decisions (state management, API design, data flow)
     - User hasn't specified quality expectations
   - ❌ **DON'T** ask when:
     - Task includes explicit effort signals: "quick," "rough," "prototype," "production-ready," "just make it work"
     - Task is clearly trivial: typo fixes, one-line changes, simple edits
     - Task is exploratory: "show me," "explain," "find," "search"
     - User just invoked `/senior-dev`, `/intermediate-dev`, `/junior-dev`, `/code`, `/implement`, or `/dev` (effort is preset or will be asked by skill)
   - ⚠️ **IMPORTANT**: When unsure if a task is ambiguous or trivial, err on the side of asking - always fallback to asking if unsure
   - Can be explicitly invoked with: `/code`, `/implement`, or `/dev` skills
   - Shortcuts available: `/senior-dev` (high effort), `/intermediate-dev` (medium effort), `/junior-dev` (low effort)

   **Effort Level Mapping** (adjustable if API behavior doesn't match expectations):

   | User Level | Seniority | API Effort | Behavior |
   |------------|-----------|------------|----------|
   | Low (Prototype) | Junior | `low` | Working code, minimal error handling, fast/cheap |
   | Medium (Solid) | Intermediate | `medium` | Good structure, reasonable error handling |
   | High (Production) | Senior | `high` | Production-ready, comprehensive, edge cases |