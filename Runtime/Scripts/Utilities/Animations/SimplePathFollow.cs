using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utilities.Animations
{
    public class SimplePathFollow : MonoBehaviour
    {
        //FIXME Does Bezier even make sense for this? It feels that having extra handles is just annoying
        private enum MOTION
        {
            LINEAR,
            CATMULLROM,
            HERMITE
        }

        [SerializeField]
        private bool looping;

        [SerializeField]
        private MOTION motion;

        [SerializeField]
        //This could be in reverse, so no Min() required
        private float speed;

        [SerializeField]
        private List<Transform> pathPoints;

        private void Start()
        {
            Assert.IsNotNull(pathPoints);
            Assert.IsTrue(pathPoints.Count >= 2);
        }

        //Curve Functions
        //================================================================================================================//

        private Vector3 GetCatmullPoint(int index)
        {
            if (looping)
            {
                int wrappedIndex = (index % pathPoints.Count + pathPoints.Count) % pathPoints.Count;
                return pathPoints[wrappedIndex].position;
            }

            if (index < 0)
            {
                Vector3 first = pathPoints[0].position;
                Vector3 second = pathPoints[1].position;
                return first + (first - second);
            }

            if (index >= pathPoints.Count)
            {
                Vector3 last = pathPoints[^1].position;
                Vector3 beforeLast = pathPoints[^2].position;
                return last + (last - beforeLast);
            }

            return pathPoints[index].position;
        }

        private static Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        //Unity Editor Functions
        //================================================================================================================//
#if UNITY_EDITOR

        [SerializeField]
        private int catmullResolution = 12;
        
        [Button]
        private void AddPoint()
        {
            if(pathPoints == null)
                pathPoints = new List<Transform>();
            
            var pointObject = new GameObject($"PathPoint[{pathPoints.Count}]");
            pointObject.transform.SetParent(gameObject.transform, false);
            pathPoints.Add(pointObject.transform);
            
            //If the points changed, we need to make sure we properly update the inspector
            EditorUtility.SetDirty(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            if (pathPoints?.Count < 2)
                return;
            
            Gizmos.color = Color.yellow;
            
            switch (motion)
            {
                case MOTION.LINEAR:
                    DrawLinearPath();
                    break;
                case MOTION.CATMULLROM:
                    DrawCatmullPath();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Gizmos.color = Color.white;
            for (int i = 0; i < pathPoints.Count; i++)
            {
                var point = pathPoints[i];
                Gizmos.DrawSphere(point.position, 0.1f);
            }
        }

        private void DrawLinearPath()
        {
            for (int i = 1; i < pathPoints.Count; i++)
            {
                Gizmos.DrawLine(pathPoints[i-1].position, pathPoints[i].position);
            }
            
            if(looping)
                Gizmos.DrawLine(pathPoints[^1].position, pathPoints[0].position);
        }
        
        private void DrawCatmullPath()
        {
            int segmentCount = looping ? pathPoints.Count : pathPoints.Count - 1;

            for (int i = 0; i < segmentCount; i++)
            {
                Vector3 p0 = GetCatmullPoint(i - 1);
                Vector3 p1 = GetCatmullPoint(i);
                Vector3 p2 = GetCatmullPoint(i + 1);
                Vector3 p3 = GetCatmullPoint(i + 2);

                Vector3 previousPoint = p1;

                for (int step = 1; step <= catmullResolution; step++)
                {
                    float t = step / (float)catmullResolution;
                    Vector3 currentPoint = GetCatmullRomPosition(t, p0, p1, p2, p3);
                    Gizmos.DrawLine(previousPoint, currentPoint);
                    previousPoint = currentPoint;
                }
            }
            
        }
#endif
        //================================================================================================================//
    }
}