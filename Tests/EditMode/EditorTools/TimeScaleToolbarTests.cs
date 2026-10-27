// Created by Claude (claude-opus-5)
// Date: 2026-08-07
using JamStarter.Editor;
using NUnit.Framework;

namespace Tests.EditorTools
{
    public class TimeScaleToolbarTests
    {
        // --- ShouldRefresh ---

        [TestCase(1f, 1.0005f)]
        [TestCase(1f, 0.9995f)]
        [TestCase(1f, 1f)]
        public void ShouldRefresh_DriftBelowEpsilon_ReturnsFalse(float displayedValue, float runtimeValue)
        {
            Assert.IsFalse(TimeScaleToolbar.ShouldRefresh(displayedValue, runtimeValue, 0d, 100d));
        }

        [Test]
        public void ShouldRefresh_DriftAboveEpsilonInsideInterval_ReturnsFalse()
        {
            Assert.IsFalse(TimeScaleToolbar.ShouldRefresh(1f, 1.5f, 100d, 100.05d));
        }

        [Test]
        public void ShouldRefresh_DriftAboveEpsilonAfterInterval_ReturnsTrue()
        {
            Assert.IsTrue(TimeScaleToolbar.ShouldRefresh(1f, 1.5f, 100d, 100.2d));
        }

        // --- ClampToMax ---

        [TestCase(5f, 2f, 2f)]
        [TestCase(1f, 2f, 1f)]
        [TestCase(-1f, 2f, 0f)]
        [TestCase(2f, 2f, 2f)]
        public void ClampToMax_ClampsIntoRange(float timeScale, float maxScale, float expected)
        {
            Assert.AreEqual(expected, TimeScaleToolbar.ClampToMax(timeScale, maxScale));
        }
    }
}
