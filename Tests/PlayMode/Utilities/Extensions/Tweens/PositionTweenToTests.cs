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

namespace Tests.Utilities.Extensions.Tweens
{
    public class PositionTweenToTests
    {
        private Transform m_transform;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            m_transform = ObjectFactory.CreatePrimitive(PrimitiveType.Cube).transform;
        }
        [SetUp]
        public void Setup()
        {
            m_transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        #region Test Position

        [UnityTest]
        public IEnumerator TweenPositionTests(
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
                    LogAssert.Expect(LogType.Error, new Regex(".*Attempting to apply.*"));

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
                    LogAssert.Expect(LogType.Error, new Regex(".*Attempting to apply.*"));

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

        [OneTimeTearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_transform.gameObject);
        }

        public static IEnumerable<Vector3> TestTargetValues()
        {
            yield return Vector3.zero;
            yield return Vector3.one;
            yield return Vector3.one * -1f;
        }
    }
}