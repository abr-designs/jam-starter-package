---
name: unity-tests
description: Write Unity Test Framework tests (EditMode or PlayMode) for game logic in C#. Focus is on testing logic within scripts, not MonoBehaviour lifecycle. Use when user says "write a test for", "I need a test", "add tests to", or "cover X with tests".
---

# Unity Tests

## Trigger flow

When invoked, ask two questions before writing anything:

**Q1 — Mode** (if not already specified):
> "Should this be an EditMode or PlayMode test?
> - **EditMode** — pure logic, no scene or GameObject needed
> - **PlayMode** — requires a scene, physics, coroutines, timing, or MonoBehaviour lifecycle
> - **Not sure** — describe the feature and I'll recommend"

**Q2 — Depth** (always ask):
> "Should I write a stub (empty test methods with TODO comments) or a fully implemented test?"

## Deciding EditMode vs PlayMode

Recommend **EditMode** when:
- Testing pure C# logic, math, or algorithms
- Testing static utility methods or extensions
- No `GameObject`, `MonoBehaviour`, or scene needed
- No coroutines, physics, or time-dependent behavior

Recommend **PlayMode** when:
- Code instantiates `GameObject`s or uses `MonoBehaviour` lifecycle
- Test needs `WaitUntil`, `WaitForSeconds`, or real-time passage
- Code uses physics, raycasts, or coroutines
- Testing systems that depend on scene state

## File placement

- EditMode tests → `Tests/EditMode/{Category}/` (mirror source folder structure)
- PlayMode tests → `Tests/PlayMode/{Category}/`
- File name: `{ClassName}Tests.cs`
- Namespace: `Tests.{Category}.{Subcategory}` — mirror source namespace structure

## Conventions

See [REFERENCE.md](REFERENCE.md) for full patterns with examples.

**Naming**:
- Class: `{ClassName}Tests`
- Method: `{MethodName}{Scenario}Test` (e.g., `PickRandomElementEmptyListTest`)
- Data provider: static `IEnumerable<TestCaseData>` method, same name as test + `Data`

**Key rules**:
- No base test classes — inherit nothing
- Use `[TestCaseSource(nameof(...))]` for parameterized tests; keep test data in a static provider method
- PlayMode async tests return `IEnumerator` and use `[UnityTest]`
- Use `WaitUntil(() => condition)` rather than `WaitForSeconds` where possible
- Custom assertions go in private helper methods at the bottom of the class
- Focus on game logic only — don't test that `Start()` / `Update()` ran
