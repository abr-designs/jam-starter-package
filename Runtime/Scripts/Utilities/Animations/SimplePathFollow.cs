using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[assembly: InternalsVisibleTo("Jam-starter.Editor")]

namespace Utilities.Animations
{
    public class SimplePathFollow : MonoBehaviour
    {
        //FIXME Does Bezier even make sense for this? It feels that having extra handles is just annoying
        internal enum MOTION
        {
            LINEAR,
            SMOOTH
        }

        [SerializeField]
        internal bool looping;

        [SerializeField]
        internal MOTION motion;

        [SerializeField]
        //This could be in reverse, so no Min() required
        private float speed;
        
        [SerializeField]
        //This could be in reverse, so no Min() required
        private bool faceDirection;

        [SerializeField]
        internal List<Vector3> pathPoints = new()
        {
            Vector3.zero,
            Vector3.forward,
        };
        
        [SerializeField, Min(3), ShowIf(nameof(SimplePathFollow.motion), MOTION.LINEAR)]
        internal int catmullResolution = 12;
        
        [SerializeField]
        private Transform targetMoveTransform;

        // Arc-length table: maps sample index → cumulative world distance
        private float[] m_arcLengthTable;
        private float m_totalLength;
        private float m_distanceTravelled;
        private bool m_pingPongForward = true;

        //UnityFunctions
        //================================================================================================================//

        private void Start()
        {
            Assert.IsNotNull(pathPoints);
            Assert.IsTrue(pathPoints.Count >= 2);
            
            BakeArcLengthTable();
        }
        
        private void Update()
        {
            if (targetMoveTransform == null) 
                return;
            if (m_totalLength <= 0f) 
                return;

            float delta = speed * Time.deltaTime;

            if (looping)
            {
                m_distanceTravelled = (m_distanceTravelled + delta) % m_totalLength;
                
                if (m_distanceTravelled < 0f) 
                    m_distanceTravelled += m_totalLength;
            }
            else
            {
                m_distanceTravelled += m_pingPongForward ? delta : -delta;

                if (m_distanceTravelled >= m_totalLength)
                {
                    m_distanceTravelled = m_totalLength;
                    m_pingPongForward = false;
                }
                else if (m_distanceTravelled <= 0f)
                {
                    m_distanceTravelled = 0f;
                    m_pingPongForward = true;
                }
            }

            targetMoveTransform.position = SamplePath(m_distanceTravelled, out var tangent);

            if (faceDirection)
                targetMoveTransform.forward = tangent;
        }
        
        // Arc-length Baking
        //================================================================================================================//

        private void BakeArcLengthTable()
        {
            int totalSamples = motion == MOTION.LINEAR
                ? pathPoints.Count + (looping ? 1 : 0)
                : looping
                    ? pathPoints.Count * catmullResolution + 1
                    : (pathPoints.Count - 1) * catmullResolution + 1;

            m_arcLengthTable = new float[totalSamples];
            m_arcLengthTable[0] = 0f;

            Vector3 previous = SamplePathByIndex(0, totalSamples);

            for (int i = 1; i < totalSamples; i++)
            {
                Vector3 current = SamplePathByIndex(i, totalSamples);
                m_arcLengthTable[i] = m_arcLengthTable[i - 1] + Vector3.Distance(previous, current);
                previous = current;
            }

            m_totalLength = m_arcLengthTable[totalSamples - 1];
        }

        // Returns a world-space point at sample index i out of totalSamples
        private Vector3 SamplePathByIndex(int i, int totalSamples)
        {
            if (motion == MOTION.LINEAR)
            {
                if (!looping)
                    return transform.TransformPoint(pathPoints[Mathf.Clamp(i, 0, pathPoints.Count - 1)]);

                int idx = i % pathPoints.Count;
                return transform.TransformPoint(pathPoints[idx]);
            }

            // SMOOTH (Catmull-Rom)
            // For the looping closing sample, explicitly return point[0] to guarantee no float precision gap
            if (looping && i == totalSamples - 1)
                return transform.TransformPoint(pathPoints[0]);

            int segCount = looping ? pathPoints.Count : pathPoints.Count - 1;

            float globalT = (float)i / (totalSamples - 1);
            float scaledT = globalT * segCount;
            int seg = Mathf.FloorToInt(scaledT);
            float localT = scaledT - seg;

            // When globalT == 1.0 (closing sample), seg == segCount; wrap it back
            if (looping)
                seg %= pathPoints.Count;
            else
                seg = Mathf.Clamp(seg, 0, pathPoints.Count - 2);

            Vector3 p0 = GetCatmullPoint(seg - 1);
            Vector3 p1 = GetCatmullPoint(seg);
            Vector3 p2 = GetCatmullPoint(seg + 1);
            Vector3 p3 = GetCatmullPoint(seg + 2);

            return LerpFunctions.CatmullRom(localT, p0, p1, p2, p3);
        }

        // Returns a world-space point at a given cumulative arc distance
        private Vector3 SamplePath(float distance, out Vector3 tangent)
        {
            tangent = Vector3.zero;
            
            if (m_arcLengthTable == null) 
                return transform.TransformPoint(pathPoints[0]);

            distance = Mathf.Clamp(distance, 0f, m_totalLength);

            int totalSamples = m_arcLengthTable.Length;

            // Binary search for the two surrounding samples
            int lo = 0, hi = totalSamples - 1;
            while (lo < hi - 1)
            {
                int mid = (lo + hi) / 2;
                if (m_arcLengthTable[mid] < distance) 
                    lo = mid;
                else 
                    hi = mid;
            }

            float segStart = m_arcLengthTable[lo];
            float segEnd = m_arcLengthTable[hi];
            float segLength = segEnd - segStart;

            float localT = segLength > 0f ? (distance - segStart) / segLength : 0f;

            Vector3 a = SamplePathByIndex(lo, totalSamples);
            Vector3 b = SamplePathByIndex(hi, totalSamples);
            
            tangent = ((b - a) * speed).normalized;
            
            return Vector3.Lerp(a, b, localT);
        }

        //Curve Functions
        //================================================================================================================//

        #region Curve Functions

        internal Vector3 GetCatmullPoint(int index)
        {
            if (looping)
            {
                int wrappedIndex = (index % pathPoints.Count + pathPoints.Count) % pathPoints.Count;
                return transform.TransformPoint(pathPoints[wrappedIndex]);
            }

            if (index < 0)
            {
                Vector3 first = transform.TransformPoint(pathPoints[0]);
                Vector3 second = transform.TransformPoint(pathPoints[1]);
                return first + (first - second);
            }

            if (index >= pathPoints.Count)
            {
                Vector3 last = transform.TransformPoint(pathPoints[^1]);
                Vector3 beforeLast = transform.TransformPoint(pathPoints[^2]);
                return last + (last - beforeLast);
            }

            return transform.TransformPoint(pathPoints[index]);
        }

        #endregion //Curve Functions

        //Unity Editor Functions
        //================================================================================================================//
#if UNITY_EDITOR
        
        internal void AddPoint()
        {
            const float DEFAULT_DISTANCE = 2f;
            if(pathPoints == null)
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
            
            //If the points changed, we need to make sure we properly update the inspector
            EditorUtility.SetDirty(gameObject);
        }

        
#endif
        //================================================================================================================//
    }
}