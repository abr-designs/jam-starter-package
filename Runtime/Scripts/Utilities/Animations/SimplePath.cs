// Created by Claude (claude-sonnet-4-6)
// Date: 2026-05-17
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

[assembly: InternalsVisibleTo("Jam-starter.Editor")]
[assembly: InternalsVisibleTo("com.abrds.jam-starter.Editor.Tests")]
[assembly: InternalsVisibleTo("com.abrds.jam-starter.Tests")]

namespace Utilities.Animations
{
    public class SimplePath : MonoBehaviour
    {
        internal enum MOTION
        {
            LINEAR,
            SMOOTH
        }

        //Fields
        //================================================================================================================//

        #region Fields

        [SerializeField, Space(10f)]
        internal bool looping;

        [SerializeField]
        internal MOTION motion;

        [SerializeField]
        internal List<Vector3> pathPoints = new()
        {
            Vector3.zero,
            Vector3.forward,
        };

        [SerializeField, Min(3)]
        internal int catmullResolution = 12;

        private float[] m_arcLengthTable;
        protected float m_totalLength;

        #endregion // Fields

        //Unity Functions
        //================================================================================================================//

        #region Unity Functions

        protected virtual void Start()
        {
            Assert.IsNotNull(pathPoints);
            Assert.IsTrue(pathPoints.Count >= 2);

            BakeArcLengthTable();
        }

        #endregion // Unity Functions

        //Public Methods
        //================================================================================================================//

        #region Public Methods

        /// <summary>
        /// Returns the world-space position at normalized path position t (0–1). Tangent points forward along the path.
        /// </summary>
        /// <remarks>Created by Claude (claude-sonnet-4-6) — 2026-05-17</remarks>
        public Vector3 Evaluate(float t, out Vector3 tangent)
        {
            var distance = Mathf.Clamp01(t) * m_totalLength;
            return SamplePath(distance, out tangent);
        }

        /// <summary>
        /// Returns the normalized path position t (0–1) and world-space point on the path closest to <paramref name="worldPosition"/>.
        /// </summary>
        /// <remarks>Created by Claude (claude-sonnet-4-6) — 2026-05-18</remarks>
        public float GetClosestT(Vector3 worldPosition, out Vector3 closestPoint)
        {
            closestPoint = transform.TransformPoint(pathPoints[0]);

            if (m_arcLengthTable == null)
                return 0f;

            if (motion == MOTION.LINEAR)
                return GetClosestTLinear(worldPosition, out closestPoint);

            var totalSamples = m_arcLengthTable.Length;
            var bestDistanceSq = float.MaxValue;
            var bestIndex = 0;

            for (var i = 0; i < totalSamples; i++)
            {
                var candidate = SamplePathByIndex(i, totalSamples);
                var distanceSq = (candidate - worldPosition).sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestIndex = i;
                closestPoint = candidate;
            }

            return m_arcLengthTable[bestIndex] / m_totalLength;
        }

        private float GetClosestTLinear(Vector3 worldPosition, out Vector3 closestPoint)
        {
            closestPoint = transform.TransformPoint(pathPoints[0]);
            var segmentCount = looping ? pathPoints.Count : pathPoints.Count - 1;
            var bestDistanceSquared = float.MaxValue;
            var bestArcDistance = 0f;

            for (var i = 0; i < segmentCount; i++)
            {
                var startPoint = transform.TransformPoint(pathPoints[i]);
                var endPoint = transform.TransformPoint(pathPoints[(i + 1) % pathPoints.Count]);
                var direction = endPoint - startPoint;
                var directionLengthSquared = direction.sqrMagnitude;
                var localT = directionLengthSquared > 0f
                    ? Mathf.Clamp01(Vector3.Dot(worldPosition - startPoint, direction) / directionLengthSquared)
                    : 0f;
                var candidate = startPoint + direction * localT;
                var distanceSquared = (candidate - worldPosition).sqrMagnitude;

                if (distanceSquared >= bestDistanceSquared)
                    continue;

                bestDistanceSquared = distanceSquared;
                closestPoint = candidate;
                var segmentLength = m_arcLengthTable[i + 1] - m_arcLengthTable[i];
                bestArcDistance = m_arcLengthTable[i] + localT * segmentLength;
            }

            return bestArcDistance / m_totalLength;
        }

        internal Vector3 GetCatmullPoint(int index)
        {
            if (looping)
            {
                var wrappedIndex = (index % pathPoints.Count + pathPoints.Count) % pathPoints.Count;
                return transform.TransformPoint(pathPoints[wrappedIndex]);
            }

            if (index < 0)
            {
                var first = transform.TransformPoint(pathPoints[0]);
                var second = transform.TransformPoint(pathPoints[1]);
                return first + (first - second);
            }

            if (index >= pathPoints.Count)
            {
                var last = transform.TransformPoint(pathPoints[^1]);
                var beforeLast = transform.TransformPoint(pathPoints[^2]);
                return last + (last - beforeLast);
            }

            return transform.TransformPoint(pathPoints[index]);
        }

        #endregion // Public Methods

        //Private Methods
        //================================================================================================================//

        #region Private Methods

        //Arc-length Baking
        //================================================================================================================//

        #region Arc-length Baking

        protected void BakeArcLengthTable()
        {
            var totalSamples = motion == MOTION.LINEAR
                ? pathPoints.Count + (looping ? 1 : 0)
                : looping
                    ? pathPoints.Count * catmullResolution + 1
                    : (pathPoints.Count - 1) * catmullResolution + 1;

            m_arcLengthTable = new float[totalSamples];
            m_arcLengthTable[0] = 0f;

            var previous = SamplePathByIndex(0, totalSamples);

            for (var i = 1; i < totalSamples; i++)
            {
                var current = SamplePathByIndex(i, totalSamples);
                m_arcLengthTable[i] = m_arcLengthTable[i - 1] + Vector3.Distance(previous, current);
                previous = current;
            }

            m_totalLength = m_arcLengthTable[totalSamples - 1];
        }

        private Vector3 SamplePathByIndex(int sampleIndex, int totalSamples)
        {
            if (motion == MOTION.LINEAR)
            {
                if (!looping)
                    return transform.TransformPoint(pathPoints[Mathf.Clamp(sampleIndex, 0, pathPoints.Count - 1)]);

                var wrappedIndex = sampleIndex % pathPoints.Count;
                return transform.TransformPoint(pathPoints[wrappedIndex]);
            }

            // For the looping closing sample, explicitly return point[0] to guarantee no float precision gap
            if (looping && sampleIndex == totalSamples - 1)
                return transform.TransformPoint(pathPoints[0]);

            if (!looping && sampleIndex == totalSamples - 1)
                return transform.TransformPoint(pathPoints[^1]);

            var segmentCount = looping ? pathPoints.Count : pathPoints.Count - 1;

            var globalT = (float)sampleIndex / (totalSamples - 1);
            var scaledT = globalT * segmentCount;
            var segment = Mathf.FloorToInt(scaledT);
            var localT = scaledT - segment;

            // When globalT == 1.0 (closing sample), segment == segmentCount; wrap it back
            if (looping)
                segment %= pathPoints.Count;
            else
                segment = Mathf.Clamp(segment, 0, pathPoints.Count - 2);

            var previousPoint = GetCatmullPoint(segment - 1);
            var startPoint = GetCatmullPoint(segment);
            var endPoint = GetCatmullPoint(segment + 1);
            var nextPoint = GetCatmullPoint(segment + 2);

            return LerpFunctions.CatmullRom(localT, previousPoint, startPoint, endPoint, nextPoint);
        }

        private Vector3 SamplePath(float distance, out Vector3 tangent)
        {
            tangent = Vector3.zero;

            if (m_arcLengthTable == null)
                return transform.TransformPoint(pathPoints[0]);

            distance = Mathf.Clamp(distance, 0f, m_totalLength);

            var totalSamples = m_arcLengthTable.Length;

            var lowerIndex = 0;
            var upperIndex = totalSamples - 1;
            while (lowerIndex < upperIndex - 1)
            {
                var middleIndex = (lowerIndex + upperIndex) / 2;
                if (m_arcLengthTable[middleIndex] < distance)
                    lowerIndex = middleIndex;
                else
                    upperIndex = middleIndex;
            }

            var segmentStart = m_arcLengthTable[lowerIndex];
            var segmentEnd = m_arcLengthTable[upperIndex];
            var segmentLength = segmentEnd - segmentStart;

            var localT = segmentLength > 0f ? (distance - segmentStart) / segmentLength : 0f;

            var startSample = SamplePathByIndex(lowerIndex, totalSamples);
            var endSample = SamplePathByIndex(upperIndex, totalSamples);

            tangent = (endSample - startSample).normalized;

            return Vector3.Lerp(startSample, endSample, localT);
        }

        #endregion // Arc-length Baking

        #endregion // Private Methods

        //Unity Editor Functions
        //================================================================================================================//
#if UNITY_EDITOR

        #region Unity Editor Functions

        internal void AddPoint()
        {
            const float DEFAULT_DISTANCE = 2f;
            if (pathPoints == null)
                pathPoints = new List<Vector3>();

            Vector3 localPosition;

            switch (pathPoints.Count)
            {
                case >= 2:
                {
                    var previousPointA = pathPoints[^2];
                    var previousPointB = pathPoints[^1];

                    var tangent = previousPointB - previousPointA;

                    localPosition = previousPointB + tangent.normalized * tangent.magnitude;
                    break;
                }
                case 1:
                    localPosition = pathPoints[^1] + transform.forward.normalized * DEFAULT_DISTANCE;
                    break;
                default:
                    localPosition = Vector3.zero;
                    break;
            }

            pathPoints.Add(localPosition);

            UnityEditor.EditorUtility.SetDirty(gameObject);
        }

        #endregion // Unity Editor Functions

#endif
        //================================================================================================================//
    }
}
