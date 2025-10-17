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
    public class RotationTweenToTests
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

            CustomAssert(m_transform.rotation, target);
        }

        [UnityTest]
        public IEnumerator TweenLocalRotationTests(
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

            CustomAssert(m_transform.localRotation, target);
        }

        #endregion

        [OneTimeTearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_transform.gameObject);
        }

        public static IEnumerable<Quaternion> TestTargetRotationValues()
        {
            yield return Quaternion.identity;
            yield return Quaternion.Euler(90, 90, 90);
            yield return Quaternion.Inverse(Quaternion.Euler(90, 90, 90));
        }

        private void CustomAssert(Quaternion a, Quaternion b)
        {
            const float tolerance = 0.001f;
            
            Assert.LessOrEqual(Quaternion.Angle(a,b), tolerance);
        }
    }
}