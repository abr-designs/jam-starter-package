# PLAN: JMath.Multiply(float, float)

**Date:** 2026-06-03
**Status:** Ready to implement

## Summary

Add a public overload of `Multiply` to `JMath` that takes two `float` parameters. Complements the existing private `Multiply(Quaternion, float)` used by `ShortestRotation`.

## Target File

`Runtime/Scripts/Utilities/JMath.cs`

## Design

```csharp
public static float Multiply(float a, float b) => a * b;
```

Placement: after the private `Multiply(Quaternion, float)` at line ~85, inside the `JMath` static class.

## Interface

| | |
|---|---|
| Inputs | `float a`, `float b` |
| Output | `float` product |
| Side effects | None |
| Edge cases | None (float handles overflow/NaN/Inf per IEEE 754) |

## Out of Scope

- Extension method variant
- Generic / double overloads
- Tests (no test infra change needed for a trivial wrapper)
