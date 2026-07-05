// Created by Claude (claude-opus-4-8)
// Date: 2026-06-24

using NUnit.Framework;
using UnityEngine;
using Utilities.TextAnimation;

namespace Tests.Utilities.TextAnimation
{
    public class EffectMathTests
    {
        private const float TOLERANCE = 0.0001f;

        // --- CharMod ---

        [Test]
        public void CharMod_Identity_IsNeutral()
        {
            var mod = CharMod.Identity;

            Assert.AreEqual(Vector3.zero, mod.Offset);
            Assert.AreEqual(0f, mod.RotationDeg);
            Assert.AreEqual(1f, mod.Scale);
            Assert.AreEqual(255, mod.Color.r);
            Assert.AreEqual(255, mod.Color.g);
            Assert.AreEqual(255, mod.Color.b);
            Assert.AreEqual(255, mod.Color.a);
        }

        // --- Wave ---

        [Test]
        public void Wave_Offset_IsVerticalAndOscillates()
        {
            var effect = new TestableWave();

            var minY = float.MaxValue;
            var maxY = float.MinValue;

            for (int i = 0; i <= 100; i++)
            {
                var time = i * 0.05f;
                var mod = CharMod.Identity;
                effect.Apply(ref mod, 0, 1, time, default);

                Assert.AreEqual(0f, mod.Offset.x, TOLERANCE);
                Assert.AreEqual(0f, mod.Offset.z, TOLERANCE);
                Assert.LessOrEqual(Mathf.Abs(mod.Offset.y), effect.Amplitude + TOLERANCE);

                minY = Mathf.Min(minY, mod.Offset.y);
                maxY = Mathf.Max(maxY, mod.Offset.y);
            }

            Assert.Greater(maxY, 0f, "Wave should push characters up at some point.");
            Assert.Less(minY, 0f, "Wave should push characters down at some point.");
        }

        // --- Pulse ---

        [Test]
        public void Pulse_Scale_OscillatesAroundOne()
        {
            var effect = new TestablePulse();

            var minScale = float.MaxValue;
            var maxScale = float.MinValue;

            for (int i = 0; i <= 100; i++)
            {
                var time = i * 0.05f;
                var mod = CharMod.Identity;
                effect.Apply(ref mod, 0, 1, time, default);

                Assert.LessOrEqual(Mathf.Abs(mod.Scale - 1f), effect.Amplitude + TOLERANCE);
                Assert.AreEqual(Vector3.zero, mod.Offset);
                Assert.AreEqual(0f, mod.RotationDeg);

                minScale = Mathf.Min(minScale, mod.Scale);
                maxScale = Mathf.Max(maxScale, mod.Scale);
            }

            Assert.Greater(maxScale, 1f, "Pulse should grow characters above unit scale.");
            Assert.Less(minScale, 1f, "Pulse should shrink characters below unit scale.");
        }

        // --- Shake ---

        [Test]
        public void Shake_Offset_DeterministicAndBounded()
        {
            var effect = new TestableShake();

            // Same time yields the same offset (Perlin noise, no randomness).
            var first = CharMod.Identity;
            var second = CharMod.Identity;
            effect.Apply(ref first, 0, 1, 0.37f, default);
            effect.Apply(ref second, 0, 1, 0.37f, default);
            Assert.AreEqual(first.Offset, second.Offset);

            // Perlin output can drift slightly outside [0,1], so allow a margin on the amplitude bound.
            var bound = effect.Amplitude * 1.25f;
            var anyMovement = false;

            for (int i = 0; i <= 100; i++)
            {
                var time = i * 0.03f;
                var mod = CharMod.Identity;
                effect.Apply(ref mod, 0, 1, time, default);

                Assert.LessOrEqual(Mathf.Abs(mod.Offset.x), bound);
                Assert.LessOrEqual(Mathf.Abs(mod.Offset.y), bound);
                Assert.AreEqual(0f, mod.Offset.z, TOLERANCE);

                if (mod.Offset.sqrMagnitude > TOLERANCE)
                    anyMovement = true;
            }

            Assert.IsTrue(anyMovement, "Shake should displace characters across the sampled times.");
        }

        // --- Jitter ---

        [Test]
        public void Jitter_OffsetAndRotation_WithinBounds()
        {
            var effect = new TestableJitter();

            for (int i = 0; i < 1000; i++)
            {
                var mod = CharMod.Identity;
                effect.Apply(ref mod, 0, 1, i * 0.01f, default);

                Assert.LessOrEqual(Mathf.Abs(mod.Offset.x), effect.PositionAmount + TOLERANCE);
                Assert.LessOrEqual(Mathf.Abs(mod.Offset.y), effect.PositionAmount + TOLERANCE);
                Assert.AreEqual(0f, mod.Offset.z, TOLERANCE);
                Assert.LessOrEqual(Mathf.Abs(mod.RotationDeg), effect.RotationAmount + TOLERANCE);
            }
        }

        // --- Testable subclasses exposing the protected tuning fields ---
        // These carry no [TextEffect] attribute, so the registry scan ignores them.

        private sealed class TestableWave : WaveMotionEffect
        {
            public float Amplitude => m_amplitude;
        }

        private sealed class TestablePulse : PulseMotionEffect
        {
            public float Amplitude => m_amplitude;
        }

        private sealed class TestableShake : ShakeMotionEffect
        {
            public float Amplitude => m_amplitude;
        }

        private sealed class TestableJitter : JitterMotionEffect
        {
            public float PositionAmount => m_positionAmount;
            public float RotationAmount => m_rotationAmount;
        }
    }
}
