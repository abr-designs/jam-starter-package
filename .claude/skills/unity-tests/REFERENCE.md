# Unity Tests Reference

> Unity Test Framework is built on a custom NUnit 3.5 base. Standard NUnit attributes work alongside Unity-specific ones, but only Unity-specific attributes gain frame-aware coroutine capabilities.

## EditMode vs PlayMode — how they run

- **EditMode** tests run inside the `EditorApplication.update` callback loop. No scene is required; use for pure logic.
- **PlayMode** tests run as coroutines during normal game execution, giving access to frame timing, physics, and `MonoBehaviour` lifecycle.

---

## EditMode test template

```csharp
using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Utilities.Extensions
{
    public class CollectionExtensionTests
    {
        [TestCaseSource(nameof(PickRandomElementData))]
        public void PickRandomElementTest(List<int> list, bool shouldThrow)
        {
            if (shouldThrow)
            {
                Assert.Throws<InvalidOperationException>(() => list.PickRandomElement());
                return;
            }

            var result = list.PickRandomElement();
            Assert.IsNotNull(result);
        }

        private static IEnumerable<TestCaseData> PickRandomElementData()
        {
            yield return new TestCaseData(new List<int> { 1, 2, 3 }, false)
                .SetName("Valid list returns element");
            yield return new TestCaseData(new List<int>(), true)
                .SetName("Empty list throws");
        }
    }
}
```

### [Test] with ExpectedResult (EditMode shorthand)

When a method returns a value, use `ExpectedResult` instead of an explicit `Assert.AreEqual`. NUnit checks the return value automatically:

```csharp
[Test(ExpectedResult = 4)]
public int AddTwoPlusTwoTest() => 2 + 2;

// Combine with TestCase for parameterized return-value tests:
[TestCase(2, 3, ExpectedResult = 5)]
[TestCase(-1, 1, ExpectedResult = 0)]
public int AddTest(int a, int b) => a + b;
```

Use `Description` on `[Test]` to document non-obvious intent:

```csharp
[Test(Description = "Ensures the pool never returns the same instance twice consecutively")]
public void PoolNeverRepeatsTest() { ... }
```

---

## PlayMode test template

```csharp
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Utilities.Extensions.Tweens
{
    public class PositionTweenToTests
    {
        private Transform m_transform;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            m_transform = new GameObject("TweenTarget").transform;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Object.Destroy(m_transform.gameObject);
        }

        [SetUp]
        public void TestSetup()
        {
            m_transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        [UnityTest]
        public IEnumerator TweenPositionTest([Values(Space.World, Space.Self)] Space space,
                                             [ValueSource(nameof(TargetPositions))] Vector3 target)
        {
            bool completed = false;

            m_transform.TweenTo(space, target, 0.2f, null, () => completed = true);

            yield return new WaitUntil(() => completed);

            var actual = space == Space.World ? m_transform.position : m_transform.localPosition;
            Assert.AreEqual(target, actual);
        }

        private static IEnumerable<Vector3> TargetPositions()
        {
            yield return Vector3.one;
            yield return new Vector3(2f, 0f, -1f);
        }
    }
}
```

### Yielding in [UnityTest]

`[UnityTest]` runs as a coroutine. Common yield patterns:

```csharp
yield return null;                          // defer to next frame
yield return new WaitUntil(() => flag);     // wait for condition (preferred)
yield return new WaitForSeconds(0.5f);      // wait real time (avoid if WaitUntil works)
yield return new WaitForFixedUpdate();      // wait for next physics step
```

### [UnitySetUp] and [UnityTearDown]

Use these instead of `[SetUp]`/`[TearDown]` when the setup itself needs to yield:

```csharp
[UnitySetUp]
public IEnumerator Setup()
{
    yield return new WaitForSeconds(0.1f);
    m_subject.ResetState();
}

[UnityTearDown]
public IEnumerator TearDown()
{
    yield return null;
    Object.Destroy(m_subject.gameObject);
}
```

---

## PlayMode with async one-time setup

Use `[UnityOneTimeSetUp]` when the one-time setup needs to yield (e.g. loading a scene):

```csharp
[UnityOneTimeSetUp]
public IEnumerator OneTimeSetup()
{
    yield return SceneManager.LoadSceneAsync("TestScene", LoadSceneMode.Additive);
    m_subject = GameObject.Find("Subject").GetComponent<MySystem>();
}
```

---

## PlayMode lifecycle timing — Start() fires before the test body

In Unity's PlayMode test runner, `AddComponent<T>()` calls `Awake()` and `OnEnable()` immediately. **`Start()` fires on the first frame** — which is the first `yield return null` if `[SetUp]` is synchronous. This means any field set via reflection or direct assignment *after* `AddComponent` (still in `[SetUp]`) is **not visible to `Start()`**.

### What this means in practice

If a MonoBehaviour reads a field in `Start()` (e.g. `speed`, `looping`) and the test sets that field after `AddComponent`, `Start()` will see the **default value**, not the test value.

```csharp
// [SetUp] — synchronous
_follow = go.AddComponent<SimplePathFollow>(); // Awake/OnEnable run
_follow.speed = 10f;                           // ← Start() hasn't run yet, so this IS seen...
                                               //   only if Start() defers to first yield
```

This is safe if all setup happens before the first `yield return null`. But **reflection-set fields behave identically** — set them before the first yield and they will be visible to `Start()`.

### The timing trap: setting values after Start() has run

The trap appears when `Start()` initializes state (e.g. `m_pingPongForward = speed > 0f`) and the test assumes a particular value, but `Start()` ran with the default `speed = 0`.

**Wrong** — Start() sees speed=0, m_pingPongForward ends up false:
```csharp
[UnityTest]
public IEnumerator Bounces_ReversesAtEnd()
{
    SetSpeed(10000f);
    yield return null; // Start() + Update() — but did Start() already fire?
    yield return null;
    Assert.IsFalse(GetPingPongForward()); // may pass or fail depending on timing
}
```

**Correct** — yield first so Start() fires with defaults, then set test-specific values:
```csharp
[UnityTest]
public IEnumerator Bounces_ReversesAtEnd()
{
    yield return null;   // Start() fires with default speed=0 → known initial state
    SetSpeed(10000f);    // set after Start(); only affects Update()
    yield return null;   // Update(): high speed hits end → direction flips
    Assert.IsFalse(GetPingPongForward());
}
```

### When to use [UnitySetUp] instead

If *all* tests in a class need `Start()` to run before doing anything, promote `[SetUp]` to `[UnitySetUp]` and yield at the end:

```csharp
[UnitySetUp]
public IEnumerator SetUp()
{
    _go = new GameObject();
    _follow = _go.AddComponent<SimplePathFollow>();
    _follow.pathPoints = new List<Vector3> { Vector3.zero, Vector3.forward * 2f };
    yield return null; // Start() runs here with the above values already set
}

[UnityTest]
public IEnumerator SomeTest()
{
    // Start() has already run — safe to set test-specific overrides
    SetSpeed(10000f);
    yield return null; // first Update()
    ...
}
```

### Watch for coincidental test passes

A test that passes only because a wrong initial state + N bounces/flips = the expected final state is fragile. If you fix an unrelated initialization bug, the coincidental pass turns into a failure. When tests check state after N frames of bounce/toggle logic, verify that the initial state is explicitly controlled, not assumed.

---

## Accessing private members in tests

Use reflection sparingly — only when the private state is the only way to verify behavior:

```csharp
var field = typeof(MyClass).GetField("m_privateField",
    BindingFlags.NonPublic | BindingFlags.Instance);
var value = field.GetValue(instance);
Assert.AreEqual(expected, value);
```

---

## Log assertions (PlayMode)

When code intentionally logs a warning or error under certain conditions:

```csharp
LogAssert.Expect(LogType.Warning, "Expected warning message");
mySystem.DoThingThatWarns();
```

---

## Custom assertion helpers

Add private methods at the bottom of the class for repeated assertion logic:

```csharp
private static void AssertQuaternionEqual(Quaternion a, Quaternion b)
{
    const float tolerance = 0.001f;
    Assert.LessOrEqual(Quaternion.Angle(a, b), tolerance);
}

private static void AssertWithinTimeThreshold(float expectedTime)
{
    const float threshold = 0.05f;
    Assert.IsTrue(Time.time - expectedTime <= threshold,
        $"Expected ~{expectedTime}s but got {Time.time}s");
}
```

---

## Attribute cheat sheet

| Attribute | Mode | When to use |
|---|---|---|
| `[Test]` | Both | Synchronous test |
| `[Test(ExpectedResult = X)]` | Both | Test returns a value; NUnit checks it equals X |
| `[Test(Description = "...")]` | Both | Document non-obvious test intent |
| `[UnityTest]` | Both | Async test returning `IEnumerator`; runs as coroutine |
| `[TestCase(...)]` | Both | Inline parameters for simple cases |
| `[TestCase(..., ExpectedResult = X)]` | Both | Inline params + return-value assertion |
| `[TestCaseSource(nameof(...))]` | Both | External static data provider (preferred for complex data) |
| `[Values(...)]` | Both | Inline multi-value for a single parameter |
| `[ValueSource(nameof(...))]` | Both | External static values for a single parameter |
| `[SetUp]` | Both | Runs before each test method |
| `[TearDown]` | Both | Runs after each test method |
| `[UnitySetUp]` | Both | Coroutine version of SetUp (can yield) |
| `[UnityTearDown]` | Both | Coroutine version of TearDown (can yield) |
| `[OneTimeSetUp]` | Both | Runs once before all tests in the class |
| `[OneTimeTearDown]` | Both | Cleanup after all tests (destroy GameObjects, etc.) |
| `[UnityOneTimeSetUp]` | PlayMode | Async OneTimeSetUp (returns IEnumerator) |
| `[UnityPlatform(...)]` | Both | Restrict test to specific runtime platforms |
| `[PrebuildSetup]` | PlayMode | Runs before the test build |
| `[PostBuildCleanup]` | PlayMode | Runs after the test build |
