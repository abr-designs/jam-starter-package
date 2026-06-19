// Created by Claude (claude-opus-4-8)
// Date: 2026-06-14

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Utilities.Enums;
using Utilities.Tweening;
using Object = UnityEngine.Object;

namespace Tests.Utilities.Extensions.Tweens
{
    public class TransformTweenExtensionsAsyncTests
    {
        private Transform m_transform;

        [SetUp]
        public void Setup()
        {
            m_transform = ObjectFactory.CreatePrimitive(PrimitiveType.Cube).transform;
            m_transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            m_transform.localScale = Vector3.one;
        }

        [TearDown]
        public void TearDown()
        {
            if (m_transform != null)
                Object.DestroyImmediate(m_transform.gameObject);
        }

        [UnityTest]
        public IEnumerator TweenToAsync_WhenAwaited_MovesTransformToTargetPosition() => UniTask.ToCoroutine(async () =>
        {
            var target = new Vector3(5f, 0f, 0f);

            await m_transform.TweenToAsync(SPACE.WORLD, target, 0.2f);

            Assert.AreEqual(target, m_transform.position);
        });

        [UnityTest]
        public IEnumerator TweenToAsync_WhenTokenCancelled_ThrowsAndFreezesAtCurrentValue() => UniTask.ToCoroutine(async () =>
        {
            var target = new Vector3(10f, 0f, 0f);
            var cts = new CancellationTokenSource();

            var tween = m_transform.TweenToAsync(SPACE.WORLD, target, 1f, CURVE.LINEAR, PlayerLoopTiming.Update, cts.Token);

            await UniTask.DelayFrame(5);
            cts.Cancel();

            var threw = false;
            try
            {
                await tween;
            }
            catch (OperationCanceledException)
            {
                threw = true;
            }

            var frozen = m_transform.position;

            Assert.IsTrue(threw, "Cancelling the token should throw OperationCanceledException.");
            Assert.AreNotEqual(target, frozen, "A cancelled tween should not reach its target.");
            Assert.AreNotEqual(Vector3.zero, frozen, "A cancelled tween should freeze after having moved from the start.");

            cts.Dispose();
        });

        [UnityTest]
        public IEnumerator TweenToAsync_WhenSecondTweenStartsOnSameProperty_CancelsTheFirst() => UniTask.ToCoroutine(async () =>
        {
            var targetA = new Vector3(10f, 0f, 0f);
            var targetB = new Vector3(0f, 10f, 0f);

            var tweenA = m_transform.TweenToAsync(SPACE.WORLD, targetA, 1f);
            await UniTask.DelayFrame(3);

            //Starting a second tween on the same (Transform, MOVE) key cancels the first via the registry.
            var tweenB = m_transform.TweenToAsync(SPACE.WORLD, targetB, 0.2f);

            var aCancelled = false;
            try
            {
                await tweenA;
            }
            catch (OperationCanceledException)
            {
                aCancelled = true;
            }

            await tweenB;

            Assert.IsTrue(aCancelled, "The prior tween should be cancelled when a new one starts on the same key.");
            Assert.AreEqual(targetB, m_transform.position);
        });

        [UnityTest]
        public IEnumerator TweenToAsync_WhenTransformDestroyedMidTween_CompletesWithoutThrowing() => UniTask.ToCoroutine(async () =>
        {
            var tween = m_transform.TweenToAsync(SPACE.WORLD, new Vector3(5f, 0f, 0f), 1f);

            await UniTask.DelayFrame(3);
            Object.DestroyImmediate(m_transform.gameObject);

            //Should unwind through the destroyed-transform guard without throwing.
            await tween;

            Assert.IsTrue(m_transform == null);
        });

        [UnityTest]
        public IEnumerator TweenToAsync_WhenSyncTweenAlreadyActive_FiresDebugAssertion() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.Expect(LogType.Assert, new Regex(".*Mixing sync & async tweens.*"));

            //A sync tween registers an active MOVE tween on the controller.
            m_transform.TweenTo(SPACE.WORLD, new Vector3(5f, 0f, 0f), 1f);

            //Starting an async tween on the same key trips the dev-time conflict assertion.
            await m_transform.TweenToAsync(SPACE.WORLD, new Vector3(-5f, 0f, 0f), 0.2f);
        });
    }
}
