using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Utilities.Tweening;
using Object = UnityEngine.Object;

public class TweenToTests
{
    private Transform m_transform;

    [SetUp]
    public void Setup()
    {
        m_transform = ObjectFactory.CreatePrimitive(PrimitiveType.Cube).transform;
    }


    #region Test Scale

    [UnityTest]
    public IEnumerator TweenScaleTests(
        [Values(0f, 0.5f, 1f)] float time,
        [Values(CURVE.LINEAR, CURVE.EASE_IN, CURVE.EASE_OUT, CURVE.EASE_IN_OUT)]
        CURVE curve,
        [ValueSource(nameof(TestTargetValues))]
        Vector3 target)
    {
        bool hasCompleted = false;
        try
        {
            if (time == 0f)
                LogAssert.Expect(LogType.Error, new Regex("Attempting to apply.*"));

            m_transform.TweenScaleTo(target, time, curve, () => { hasCompleted = true; });
        }
        catch (Exception e)
        {
            hasCompleted = true;
            Console.WriteLine(e);
            throw;
        }

        yield return new WaitUntil(() => hasCompleted);

        Assert.AreEqual(m_transform.localScale, target);
    }

    #endregion

    #region Test Position

    [UnityTest]
    public IEnumerator TweenPositionTests(
        [Values(0f, 0.5f, 1f)] float time,
        [Values(CURVE.LINEAR, CURVE.EASE_IN, CURVE.EASE_OUT, CURVE.EASE_IN_OUT)]
        CURVE curve,
        [ValueSource(nameof(TestTargetValues))]
        Vector3 target)
    {
        yield return null;
        bool hasCompleted = false;
        try
        {
            if (time == 0f)
                LogAssert.Expect(LogType.Error, new Regex("Attempting to apply.*"));

            m_transform.TweenTo(target, time, curve, () => { hasCompleted = true; });
        }
        catch (Exception e)
        {
            hasCompleted = true;
            Console.WriteLine(e);
            throw;
        }

        yield return new WaitUntil(() => hasCompleted);

        Assert.AreEqual(m_transform.position, target);
    }

    [UnityTest]
    public IEnumerator TweenLocalPositionTests(
        [Values(0f, 0.5f, 1f)] float time,
        [Values(CURVE.LINEAR, CURVE.EASE_IN, CURVE.EASE_OUT, CURVE.EASE_IN_OUT)]
        CURVE curve,
        [ValueSource(nameof(TestTargetValues))]
        Vector3 target)
    {
        yield return null;
        bool hasCompleted = false;
        try
        {
            if (time == 0f)
                LogAssert.Expect(LogType.Error, new Regex("Attempting to apply.*"));

            m_transform.TweenToLocal(target, time, curve, () => { hasCompleted = true; });
        }
        catch (Exception e)
        {
            hasCompleted = true;
            Console.WriteLine(e);
            throw;
        }

        yield return new WaitUntil(() => hasCompleted);

        Assert.AreEqual(m_transform.localPosition, target);
    }

    #endregion

    #region Test Rotation

    [UnityTest]
    public IEnumerator TweenRotationTests(
        [Values(0f, 0.5f, 1f)] float time,
        [Values(CURVE.LINEAR, CURVE.EASE_IN, CURVE.EASE_OUT, CURVE.EASE_IN_OUT)]
        CURVE curve,
        [ValueSource(nameof(TestTargetRotationValues))]
        Quaternion target)
    {
        yield return null;
        bool hasCompleted = false;
        try
        {
            if (time == 0f)
                LogAssert.Expect(LogType.Error, new Regex("Attempting to apply.*"));

            m_transform.TweenTo(target, time, curve, () => { hasCompleted = true; });
        }
        catch (Exception e)
        {
            hasCompleted = true;
            Console.WriteLine(e);
            throw;
        }

        yield return new WaitUntil(() => hasCompleted);

        Assert.AreEqual(m_transform.rotation, target);
    }

    [UnityTest]
    public IEnumerator TweenLocalPositionTests(
        [Values(0f, 0.5f, 1f)] float time,
        [Values(CURVE.LINEAR, CURVE.EASE_IN, CURVE.EASE_OUT, CURVE.EASE_IN_OUT)]
        CURVE curve,
        [ValueSource(nameof(TestTargetRotationValues))]
        Quaternion target)
    {
        yield return null;
        bool hasCompleted = false;
        try
        {
            if (time == 0f)
                LogAssert.Expect(LogType.Error, new Regex("Attempting to apply.*"));

            m_transform.TweenToLocal(target, time, curve, () => { hasCompleted = true; });
        }
        catch (Exception e)
        {
            hasCompleted = true;
            Console.WriteLine(e);
            throw;
        }

        yield return new WaitUntil(() => hasCompleted);

        Assert.AreEqual(m_transform.localRotation, target);
    }

    #endregion

    [TearDown]
    public void TearDown()
    {
        if (m_transform == null)
            return;

        Object.Destroy(m_transform.gameObject);
    }

    public static IEnumerable<Vector3> TestTargetValues()
    {
        yield return Vector3.zero;
        yield return Vector3.one;
        yield return Vector3.one * -1f;
    }

    public static IEnumerable<Quaternion> TestTargetRotationValues()
    {
        yield return Quaternion.identity;
        yield return Quaternion.Euler(90, 90, 90);
        yield return Quaternion.Inverse(Quaternion.Euler(90, 90, 90));
    }
}
