// Created by Claude (claude-opus-4-8)
// Date: 2026-06-24

using System;
using NUnit.Framework;
using Utilities.TextAnimation;

namespace Tests.Utilities.TextAnimation
{
    public class TextEffectRegistryTests
    {
        [TestCase("shake", typeof(ShakeEffect))]
        [TestCase("wave", typeof(WaveEffect))]
        [TestCase("jitter", typeof(JitterEffect))]
        [TestCase("pulse", typeof(PulseEffect))]
        public void Get_BuiltInKey_ResolvesToCorrectType(string key, Type expected)
        {
            var effect = TextEffectRegistry.Get(key);

            Assert.IsNotNull(effect, $"Expected built-in effect for key '{key}'.");
            Assert.AreEqual(expected, effect.GetType());
        }

        [Test]
        public void Get_UnknownKey_ReturnsNull()
        {
            Assert.IsNull(TextEffectRegistry.Get("__definitely_not_an_effect__"));
        }

        [Test]
        public void Get_CustomEffectInTestAssembly_IsDiscovered()
        {
            // Proves the reflection scan reaches effects declared outside the runtime assembly.
            var effect = TextEffectRegistry.Get("__test_marker");

            Assert.IsNotNull(effect, "Custom effect in the test assembly should be discovered by reflection.");
            Assert.AreEqual(typeof(TestMarkerEffect), effect.GetType());
        }
    }
}
