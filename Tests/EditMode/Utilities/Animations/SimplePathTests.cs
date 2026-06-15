// Created by Claude (claude-sonnet-4-6)
// Date: 2026-05-18
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Utilities.Animations;

namespace Tests.Utilities.Animations
{
    public class SimplePathTests
    {
        private const float TOLERANCE = 0.001f;

        private readonly List<GameObject> _created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created)
                Object.DestroyImmediate(go);
            _created.Clear();
        }

        private TestablePath CreatePath(SimplePath.MOTION motion, bool looping, params Vector3[] points)
        {
            var go = new GameObject("TestPath");
            _created.Add(go);
            var path = go.AddComponent<TestablePath>();
            path.motion = motion;
            path.looping = looping;
            path.catmullResolution = 12;
            path.pathPoints = new List<Vector3>(points);
            path.Bake();
            return path;
        }

        // --- Evaluate ---

        [TestCase(false)]
        [TestCase(true)]
        public void Evaluate_Linear_AtZero_ReturnsFirstPoint(bool looping)
        {
            var path = CreatePath(SimplePath.MOTION.LINEAR, looping, Vector3.zero, new Vector3(0, 0, 2));

            var pos = path.Evaluate(0f, out _);

            Assert.AreEqual(Vector3.zero, pos);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Evaluate_Linear_AtOne_ReturnsExpectedEndpoint(bool looping)
        {
            var path = CreatePath(SimplePath.MOTION.LINEAR, looping, Vector3.zero, new Vector3(0, 0, 2));

            var pos = path.Evaluate(1f, out _);

            var expectedZ = looping ? 0f : 2f;
            Assert.That(pos.z, Is.EqualTo(expectedZ).Within(TOLERANCE));
        }

        [Test]
        public void Evaluate_Linear_AtMidpoint_ReturnsMidpoint()
        {
            var path = CreatePath(SimplePath.MOTION.LINEAR, false, Vector3.zero, new Vector3(0, 0, 4));

            var pos = path.Evaluate(0.5f, out _);

            Assert.That(pos.z, Is.EqualTo(2f).Within(TOLERANCE));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Evaluate_Smooth_AtZero_ReturnsFirstPoint(bool looping)
        {
            var path = CreatePath(SimplePath.MOTION.SMOOTH, looping,
                Vector3.zero, new Vector3(0, 0, 2), new Vector3(0, 0, 4), new Vector3(0, 0, 6));

            var pos = path.Evaluate(0f, out _);

            Assert.That(pos.z, Is.EqualTo(0f).Within(TOLERANCE));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Evaluate_Smooth_AtOne_ReturnsExpectedEndpoint(bool looping)
        {
            var path = CreatePath(SimplePath.MOTION.SMOOTH, looping,
                Vector3.zero, new Vector3(0, 0, 2), new Vector3(0, 0, 4), new Vector3(0, 0, 6));

            var pos = path.Evaluate(1f, out _);

            var expectedZ = looping ? 0f : 6f;
            Assert.That(pos.z, Is.EqualTo(expectedZ).Within(TOLERANCE));
        }

        [Test]
        public void Evaluate_Tangent_Linear_PointsAlongPath()
        {
            var path = CreatePath(SimplePath.MOTION.LINEAR, false, Vector3.zero, new Vector3(0, 0, 4));

            path.Evaluate(0.5f, out var tangent);

            Assert.That(tangent.z, Is.EqualTo(1f).Within(TOLERANCE));
            Assert.That(tangent.x, Is.EqualTo(0f).Within(TOLERANCE));
            Assert.That(tangent.y, Is.EqualTo(0f).Within(TOLERANCE));
        }

        // --- GetClosestT ---

        [Test]
        public void GetClosestT_PointAtStart_ReturnsZero()
        {
            var path = CreatePath(SimplePath.MOTION.LINEAR, false, Vector3.zero, new Vector3(0, 0, 4));

            var t = path.GetClosestT(Vector3.zero, out _);

            Assert.That(t, Is.EqualTo(0f).Within(TOLERANCE));
        }

        [Test]
        public void GetClosestT_PointAtEnd_ReturnsOne()
        {
            var path = CreatePath(SimplePath.MOTION.LINEAR, false, Vector3.zero, new Vector3(0, 0, 4));

            var t = path.GetClosestT(new Vector3(0, 0, 4), out _);

            Assert.That(t, Is.EqualTo(1f).Within(TOLERANCE));
        }

        [Test]
        public void GetClosestT_PointBesidePath_ReturnsNearestOnPath()
        {
            var path = CreatePath(SimplePath.MOTION.LINEAR, false, Vector3.zero, new Vector3(0, 0, 4));

            path.GetClosestT(new Vector3(99f, 0, 2), out var closest);

            // Closest point on the Z-axis path should be near z=2, x=0
            Assert.That(closest.x, Is.EqualTo(0f).Within(TOLERANCE));
            Assert.That(closest.z, Is.EqualTo(2f).Within(0.5f)); // arc-length sample resolution
        }

        // --- GetCatmullPoint ---

        [Test]
        public void GetCatmullPoint_NegativeIndex_NonLooping_ExtrapolatesBackward()
        {
            // Phantom = first + (first - second) = (0,0,0) + ((0,0,0) - (0,0,2)) = (0,0,-2)
            var path = CreatePath(SimplePath.MOTION.SMOOTH, false,
                Vector3.zero, new Vector3(0, 0, 2), new Vector3(0, 0, 4));

            var phantom = path.GetCatmullPoint(-1);

            Assert.That(phantom.z, Is.EqualTo(-2f).Within(TOLERANCE));
        }

        [Test]
        public void GetCatmullPoint_BeyondCount_NonLooping_ExtrapolatesForward()
        {
            // Phantom = last + (last - beforeLast) = (0,0,4) + ((0,0,4) - (0,0,2)) = (0,0,6)
            var path = CreatePath(SimplePath.MOTION.SMOOTH, false,
                Vector3.zero, new Vector3(0, 0, 2), new Vector3(0, 0, 4));

            var phantom = path.GetCatmullPoint(3);

            Assert.That(phantom.z, Is.EqualTo(6f).Within(TOLERANCE));
        }

        [Test]
        public void GetCatmullPoint_NegativeIndex_Looping_WrapsToLast()
        {
            // Looping: index -1 wraps to last point (index 2 = (0,0,4))
            var path = CreatePath(SimplePath.MOTION.SMOOTH, true,
                Vector3.zero, new Vector3(0, 0, 2), new Vector3(0, 0, 4));

            var wrapped = path.GetCatmullPoint(-1);
            var last = path.GetCatmullPoint(2);

            Assert.That(wrapped.z, Is.EqualTo(last.z).Within(TOLERANCE));
        }

        // --- Helper ---

        private class TestablePath : SimplePath
        {
            public void Bake() => BakeArcLengthTable();
        }
    }
}
