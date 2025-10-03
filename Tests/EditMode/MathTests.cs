using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Utilities;

public class MathTests
{
    public static IEnumerable<TestCaseData> Vector2TestCases()
    {
        yield return new TestCaseData(new Vector2(1, 1), new Vector2(1, 1));
        yield return new TestCaseData(new Vector2(0, 0), new Vector2(0, 0));
        yield return new TestCaseData(new Vector2(-1, -1), new Vector2(1, 1));
        yield return new TestCaseData(new Vector2(-1, 1), new Vector2(1, 1));
    }

    public static IEnumerable<TestCaseData> Vector3TestCases()
    {
        yield return new TestCaseData(new Vector3(1, 1, 1), new Vector3(1, 1, 1));
        yield return new TestCaseData(new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        yield return new TestCaseData(new Vector3(-1, -1, 1), new Vector3(1, 1, 1));
        yield return new TestCaseData(new Vector3(-1, 1, -1), new Vector3(1, 1, 1));
    }

    [TestCaseSource(nameof(Vector2TestCases))]
    public void Vector2AbsTest(Vector2 value, Vector2 expectedResult)
    {
        var result = value.Abs();
        Assert.AreEqual(result, expectedResult);
    }

    [TestCaseSource(nameof(Vector3TestCases))]
    public void Vector3AbsTest(Vector3 value, Vector3 expectedResult)
    {
        var result = value.Abs();
        Assert.AreEqual(result, expectedResult);
    }

    [Test]
    [TestCase(0, 0)]
    [TestCase(360, 0)]
    [TestCase(180, 180)]
    [TestCase(-180, 180)]
    [TestCase(720, 0)]
    [TestCase(450, 90)]
    public void ClampAngleTest(float angle, float expectedResult)
    {
        var result = JMath.ClampAngle(angle);
        Assert.AreEqual(result, expectedResult);
    }
}
