using System.Diagnostics;
using UnityEngine;

namespace Utilities.Debugging
{
    public static class Draw
    {
        const float GIZMOS_RADIUS = 0.025f;
        private static Camera _currentCamera;

        [Conditional("UNITY_EDITOR")]
        public static void Label(Vector3 position, string label)
        {
            Label(position, label, /*Color.white, */GIZMOS_RADIUS * 2f);
        }
        [Conditional("UNITY_EDITOR")]
        public static void Label(Vector3 position, string label, /*Color color, */float offset)
        {
#if UNITY_EDITOR
            if(Camera.current != null && _currentCamera != Camera.current)
                _currentCamera = Camera.current;
            
            var currentCameraTransform = _currentCamera.transform;
            var camRight = currentCameraTransform.right.normalized;
            
            var camPositionOffset = camRight * offset;
            
            UnityEditor.Handles.Label(position + camPositionOffset, label );
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void Circle(Vector3 position,Color color, float radius = 1f, int segments = 12)
        {
            // If either radius or number of segments are less or equal to 0, skip drawing
            if (radius <= 0.0f || segments <= 0)
            {
                return;
            }
 
            // Single segment of the circle covers (360 / number of segments) degrees
            float angleStep = (360.0f / segments);
 
            // Result is multiplied by Mathf.Deg2Rad constant which transforms degrees to radians
            // which are required by Unity's Mathf class trigonometry methods
 
            angleStep *= Mathf.Deg2Rad;
 
            // lineStart and lineEnd variables are declared outside of the following for loop
            Vector3 lineStart = Vector3.zero;
            Vector3 lineEnd = Vector3.zero;
 
            for (int i = 0; i < segments; i++)
            {
                // Line start is defined as starting angle of the current segment (i)
                lineStart.x = Mathf.Cos(angleStep * i) ;
                lineStart.y = Mathf.Sin(angleStep * i);
 
                // Line end is defined by the angle of the next segment (i+1)
                lineEnd.x = Mathf.Cos(angleStep * (i + 1));
                lineEnd.y = Mathf.Sin(angleStep * (i + 1));
 
                // Results are multiplied so they match the desired radius
                lineStart *= radius;
                lineEnd *= radius;
 
                // Results are offset by the desired position/origin 
                //lineStart += position;
                //lineEnd += position;
 
                // Points are connected using DrawLine method and using the passed color
                UnityEngine.Debug.DrawLine(lineStart + position, lineEnd + position, color);
            }
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void Circle(Vector3 position, Vector3 normal, Color color, float radius = 1f, int segments = 12)
        {
            // If either radius or number of segments are less or equal to 0, skip drawing
            if (radius <= 0.0f || segments <= 0)
            {
                return;
            }
 
            // Single segment of the circle covers (360 / number of segments) degrees
            float angleStep = (360.0f / segments);
 
            // Result is multiplied by Mathf.Deg2Rad constant which transforms degrees to radians
            // which are required by Unity's Mathf class trigonometry methods
 
            angleStep *= Mathf.Deg2Rad;
 
            // lineStart and lineEnd variables are declared outside of the following for loop
            Vector3 lineStart = Vector3.zero;
            Vector3 lineEnd = Vector3.zero;
            
            var rot = Quaternion.LookRotation(normal);
            
            for (int i = 0; i < segments; i++)
            {
                // Line start is defined as starting angle of the current segment (i)
                lineStart.x = Mathf.Cos(angleStep * i) ;
                lineStart.y = Mathf.Sin(angleStep * i);
 
                // Line end is defined by the angle of the next segment (i+1)
                lineEnd.x = Mathf.Cos(angleStep * (i + 1));
                lineEnd.y = Mathf.Sin(angleStep * (i + 1));

                var newStart = rot * lineStart;
                var newEnd = rot * lineEnd;
 
                // Results are multiplied so they match the desired radius
                newStart *= radius;
                newEnd *= radius;

                // Results are offset by the desired position/origin 
                //newStart += position;
                //newEnd += position;

                // Points are connected using DrawLine method and using the passed color
                UnityEngine.Debug.DrawLine(newStart + position, newEnd + position, color);
            }
            
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void Arrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            UnityEngine.Debug.DrawRay(pos, direction, color);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
                            new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
                           new Vector3(0, 0, 1);
            UnityEngine.Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            UnityEngine.Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
        }
    }
}