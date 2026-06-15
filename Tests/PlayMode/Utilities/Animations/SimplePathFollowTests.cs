// Created by Claude (claude-sonnet-4-6)
// Date: 2026-05-18
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Utilities.Animations;
using Assert = UnityEngine.Assertions.Assert;

namespace Tests.Utilities.Animations
{
    public class SimplePathFollowTests
    {
        private static readonly FieldInfo SpeedField =
            typeof(SimplePathFollow).GetField("speed", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo StartingPositionField =
            typeof(SimplePathFollow).GetField("startingPosition", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo DistanceTravelledField =
            typeof(SimplePathFollow).GetField("m_distanceTravelled", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo PingPongForwardField =
            typeof(SimplePathFollow).GetField("m_pingPongForward", BindingFlags.NonPublic | BindingFlags.Instance);

        private GameObject _pathGo;
        private GameObject _targetGo;
        private TestablePathFollow _follow;

        [SetUp]
        public void SetUp()
        {
            _pathGo = new GameObject("TestPath");
            _targetGo = new GameObject("Target");
            _follow = _pathGo.AddComponent<TestablePathFollow>();
            _follow.pathPoints = new List<Vector3> { Vector3.zero, new Vector3(0, 0, 2) };
            _follow.SetTarget(_targetGo.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_pathGo);
            Object.Destroy(_targetGo);
        }

        // --- Position advances ---

        [UnityTest]
        public IEnumerator Follow_PositiveSpeed_AdvancesTargetPosition()
        {
            SetSpeed(1f);
            yield return null; // Start() runs

            var startZ = _targetGo.transform.position.z;

            yield return null; // Update() runs

            Assert.IsTrue(_targetGo.transform.position.z > startZ,
                $"Expected forward movement. start={startZ} current={_targetGo.transform.position.z}");
        }

        // --- Ping-pong ---

        [UnityTest]
        public IEnumerator Follow_NonLooping_BouncesBoundary_ReversesDirection()
        {
            yield return null; // Start() fires with speed=0 → m_pingPongForward = true
            // High speed guarantees traversal past totalLength in first Update() frame
            SetSpeed(10000f);
            yield return null; // Update(): distance > totalLength → clamped, m_pingPongForward flips to false

            Assert.IsFalse(GetPingPongForward(), "Expected direction reversal after reaching end");
        }

        [UnityTest]
        public IEnumerator Follow_NonLooping_BouncesAtStart_ReversesDirectionAgain()
        {
            yield return null; // Start() fires with speed=0 → m_pingPongForward = true
            // High speed: frame 1 bounces at end (false), frame 2 travels back and bounces at start (true)
            SetSpeed(10000f);
            yield return null; // Update() 1: bounces at end → m_pingPongForward = false
            yield return null; // Update() 2: bounces at start → m_pingPongForward = true

            Assert.IsTrue(GetPingPongForward(), "Expected direction to reverse again after bouncing at start");
        }

        // --- Looping ---

        [UnityTest]
        public IEnumerator Follow_Looping_DoesNotReverse()
        {
            _follow.looping = true;
            SetSpeed(10000f);
            yield return null; // Start(): m_pingPongForward = true
            yield return null; // Update(): wraps via %, never flips direction
            yield return null;

            Assert.IsTrue(GetPingPongForward(), "Looping mode should never reverse direction");
        }

        // --- Negative speed ---

        [UnityTest]
        public IEnumerator Follow_NegativeSpeed_Looping_MovesBackwardAlongPath()
        {
            _follow.looping = true;
            SetStartingPosition(0.5f); // start at midpoint (z=1)
            SetSpeed(-1f);
            yield return null; // Start(): distanceTravelled = 1.0

            var posAfterStart = _targetGo.transform.position.z;

            yield return new WaitForSeconds(0.2f); // 0.2s at speed -1 = -0.2 units → z ≈ 0.8

            Assert.IsTrue(_targetGo.transform.position.z < posAfterStart,
                $"Expected backward movement. start={posAfterStart} current={_targetGo.transform.position.z}");
        }

        // --- FaceDirection ---

        [UnityTest]
        public IEnumerator Follow_FaceDirection_InstantSnap_AlignsToTangent()
        {
            _follow.SetFaceDirection(true);
            _follow.SetRotationSpeed(0f); // instant snap
            _targetGo.transform.rotation = Quaternion.Euler(0, 90, 0); // start facing sideways
            SetSpeed(1f);
            yield return null; // Start()
            yield return null; // Update(): ApplyPathTransform snaps rotation to path tangent

            // Path is along Z axis — tangent = forward — rotation should be near identity
            var angle = Quaternion.Angle(_targetGo.transform.rotation, Quaternion.LookRotation(Vector3.forward));
            Assert.IsTrue(angle < 1f, $"Expected near-identity rotation, got angle={angle}");
        }

        [UnityTest]
        public IEnumerator Follow_FaceDirection_WithRotationSpeed_RotatesGradually()
        {
            _follow.SetFaceDirection(true);
            _follow.SetRotationSpeed(1f); // 1 degree/second — very slow
            _targetGo.transform.rotation = Quaternion.Euler(0, 180, 0); // start facing backward
            SetSpeed(1f);
            yield return null; // Start()
            yield return null; // Update(): RotateTowards by ~1 * deltaTime degrees

            // After one frame at 1 deg/sec, still nearly facing backward
            var angle = Quaternion.Angle(_targetGo.transform.rotation, Quaternion.LookRotation(Vector3.forward));
            Assert.IsTrue(angle > 90f, $"Expected gradual rotation, angle should be > 90 but was {angle}");
        }

        // --- Helpers ---

        private void SetSpeed(float value) => SpeedField.SetValue(_follow, value);
        private void SetStartingPosition(float value) => StartingPositionField.SetValue(_follow, value);
        private bool GetPingPongForward() => (bool)PingPongForwardField.GetValue(_follow);

        private class TestablePathFollow : SimplePathFollow
        {
            public void SetTarget(Transform t) => targetMoveTransform = t;
            public void SetFaceDirection(bool v) => faceDirection = v;
            public void SetRotationSpeed(float v) => rotationSpeed = v;
        }
    }
}
