// Created by Claude (claude-opus-4-8)
// Date: 2026-06-24

using System;
using NUnit.Framework;
using Utilities.TextAnimation;

namespace Tests.Utilities.TextAnimation
{
    public class TextEffectRegistryTests
    {
        [TestCase("shake", typeof(ShakeMotionEffect))]
        [TestCase("wave", typeof(WaveMotionEffect))]
        [TestCase("jitter", typeof(JitterMotionEffect))]
        [TestCase("pulse", typeof(PulseMotionEffect))]
        public void GetMotion_BuiltInKey_ResolvesToCorrectType(string key, Type expected)
        {
            var effect = TextEffectRegistry.GetMotion(key);

            Assert.IsNotNull(effect, $"Expected built-in motion effect for key '{key}'.");
            Assert.AreEqual(expected, effect.GetType());
        }

        [TestCase("rainbow", typeof(RainbowColorEffect))]
        [TestCase("gradient", typeof(GradientColorEffect))]
        [TestCase("fade", typeof(FadeColorEffect))]
        [TestCase("flash", typeof(FlashColorEffect))]
        public void GetColor_BuiltInKey_ResolvesToCorrectType(string key, Type expected)
        {
            var effect = TextEffectRegistry.GetColor(key);

            Assert.IsNotNull(effect, $"Expected built-in color effect for key '{key}'.");
            Assert.AreEqual(expected, effect.GetType());
        }

        [Test]
        public void GetMotion_ColorKey_ReturnsNull()
        {
            // Channels are separate: a color key must not resolve on the motion table.
            Assert.IsNull(TextEffectRegistry.GetMotion("rainbow"));
        }

        [Test]
        public void GetMotion_UnknownKey_ReturnsNull()
        {
            Assert.IsNull(TextEffectRegistry.GetMotion("__definitely_not_an_effect__"));
        }

        [Test]
        public void GetMotion_CustomEffectInTestAssembly_IsDiscovered()
        {
            // Proves the reflection scan reaches effects declared outside the runtime assembly.
            var effect = TextEffectRegistry.GetMotion("__test_marker");

            Assert.IsNotNull(effect, "Custom effect in the test assembly should be discovered by reflection.");
            Assert.AreEqual(typeof(TestMarkerEffect), effect.GetType());
        }
    }
}
