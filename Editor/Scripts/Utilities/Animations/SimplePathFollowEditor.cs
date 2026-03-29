using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Utilities;
using Utilities.Animations;

namespace JamStarter.Editor.Scripts.Utilities.Animations
{
    [CustomEditor(typeof(SimplePathFollow))]
    public class SimplePathFollowEditor : UnityEditor.Editor
    {
        private SimplePathFollow m_simplePathFollow;
        private SerializedProperty m_pathPointsProp;

        private int m_selectedIndex = -1;

        private void OnEnable()
        {
            m_simplePathFollow = (SimplePathFollow)target;
            m_pathPointsProp = serializedObject.FindProperty(nameof(SimplePathFollow.pathPoints));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Add Point"))
            {
                m_simplePathFollow.AddPoint();
            }
        }

        private void OnSceneGUI()
        {
            if (m_simplePathFollow == null)
                return;

            if (m_pathPointsProp == null || !m_pathPointsProp.isArray)
                return;

            var pathPoints = m_simplePathFollow.pathPoints;


            for (var i = pathPoints.Count - 1; i >= 0; i--)
            {
                if (i == m_selectedIndex)
                    continue;

                //Exit early if the counts no longer match
                if (m_pathPointsProp.arraySize != pathPoints.Count)
                    return;

                var element = m_pathPointsProp.GetArrayElementAtIndex(i);
                Vector3 point = m_simplePathFollow.transform.TransformPoint(element.vector3Value);

                Handles.color = Color.red;
                if (Handles.Button(point, Quaternion.identity, 0.5f, 0.5f * 1.5f, Handles.SphereHandleCap))
                {
                    SelectPoint(i);
                    return;
                }

                DrawLabel(point, $"Element {i}");
            }
            
            DrawNewPointPreview();
            
            //Checks to ensure that the element selected is still alive & available
            //----------------------------------------------------------//
            if (m_selectedIndex == -1)
                return;
            if (m_selectedIndex >= m_pathPointsProp.arraySize)
            {
                m_selectedIndex = -1;
                return;
            }

            DrawWindow();

            serializedObject.Update();

            var currentPoint = m_pathPointsProp.GetArrayElementAtIndex(m_selectedIndex);

            if (currentPoint == null)
            {
                m_selectedIndex = -1;
                return;
            }

            //----------------------------------------------------------//

            EditorGUI.BeginChangeCheck();
            var currentPointPosition = m_simplePathFollow.transform.TransformPoint(currentPoint.vector3Value);

            Handles.color = Color.white;
            Handles.SphereHandleCap(0, currentPointPosition, Quaternion.identity, 0.5f, EventType.Repaint);

            var newPos = Handles.PositionHandle(currentPointPosition, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.Update();
                currentPoint.vector3Value = m_simplePathFollow.transform.InverseTransformPoint(newPos);

                serializedObject.ApplyModifiedProperties();
            }
        }

        private void SelectPoint(int index)
        {
            m_selectedIndex = index;
        }

        private static void DrawLabel(Vector3 position, string text)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                normal =
                {
                    textColor = Color.white
                },
                alignment = TextAnchor.MiddleCenter
            };

            Vector2 size = style.CalcSize(new GUIContent(text));

            Handles.BeginGUI();

            Vector2 screenPos = HandleUtility.WorldToGUIPoint(position);

            screenPos.x += (size.x / 2f) * 1.3f;

            Rect rect = new Rect(
                screenPos.x - size.x / 2,
                screenPos.y - size.y / 2,
                size.x,
                size.y
            );

            // Draw background
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.6f));

            // Draw label
            GUI.Label(rect, text, style);

            Handles.EndGUI();
        }

        //Drawing Floating Window
        //================================================================================================================//

        private Rect m_buttonWindowRect;

        private void DrawWindow()
        {
            if (m_simplePathFollow.pathPoints.Count < 2)
                return;

            var point = m_simplePathFollow.transform.TransformPoint(m_simplePathFollow.pathPoints[m_selectedIndex]);
            Vector2 guiPos = HandleUtility.WorldToGUIPoint(point);
            var width = m_selectedIndex == m_simplePathFollow.pathPoints.Count - 1 && !m_simplePathFollow.looping ? 60 : 30;

            Handles.BeginGUI();

            m_buttonWindowRect = new Rect(guiPos.x + 15, guiPos.y - 20, width, 30);
            GUILayout.BeginArea(m_buttonWindowRect /*, GUI.skin.window*/);

            GUILayout.BeginHorizontal();

            var removeIcon = EditorGUIUtility.IconContent("TreeEditor.Trash" /*"Toolbar Minus"*/);
            removeIcon.tooltip = "Delete Path Point";

            if (!m_simplePathFollow.looping && m_selectedIndex == m_simplePathFollow.pathPoints.Count - 1)
            {
                var addIcon = EditorGUIUtility.IconContent("Toolbar Plus");

                if (GUILayout.Button(addIcon))
                    AddPoint(m_simplePathFollow, m_selectedIndex + 1);
            }

            /*if (GUILayout.Button("Insert Before"))
                AddPoint(m_simplePathFollow, m_selectedIndex);*/

            Color oldColor = GUI.color;

            GUI.color = new Color(0.8588f, 0.2431f, 0.1137f);
            if (GUILayout.Button(removeIcon))
                DeletePoint(m_simplePathFollow, m_selectedIndex);
            GUI.color = oldColor;

            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            Handles.EndGUI();
        }

        private void AddPoint(SimplePathFollow path, int index)
        {
            Undo.RecordObject(path, "Add Path Point");

            Vector3 newPoint;
            // Parent it
            // Position it (simple example)
            if (path.pathPoints.Count >= 2)
            {

                var previousPointA = path.pathPoints[^2];
                var previousPointB = path.pathPoints[^1];

                var tangent = previousPointB - previousPointA;

                newPoint = previousPointB + tangent.normalized * tangent.magnitude;

                //newPoint = path.pathPoints[prevIndex] + path.transform.forward;
            }
            else
            {
                newPoint = path.transform.position;
            }

            path.pathPoints.Insert(index, newPoint);
            SelectPoint(index);
            EditorUtility.SetDirty(path);
        }
        
        private void InsertPoint(Vector3 position, int index)
        {
            Undo.RegisterCompleteObjectUndo(m_simplePathFollow, "Insert Path Point");

            var localPosition = m_simplePathFollow.transform.InverseTransformPoint(position);
            m_simplePathFollow.pathPoints.Insert(index + 1, localPosition);

            EditorUtility.SetDirty(m_simplePathFollow);
            
            SelectPoint(index + 1);
        }

        private void DeletePoint(SimplePathFollow path, int index)
        {
            if (path.pathPoints.Count <= 1)
                return;

            Undo.RecordObject(path, "Delete Path Point");

            path.pathPoints.RemoveAt(index);

            m_selectedIndex = Mathf.Clamp(m_selectedIndex - 1, 0, path.pathPoints.Count - 1);

            EditorUtility.SetDirty(path);
        }

        //================================================================================================================//

        #region OnDrawGizmos

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void OnSceneGUIAlways(SimplePathFollow simplePathFollow, GizmoType gizmoType)
        {
            var arraySize = simplePathFollow.pathPoints.Count;
            var looping = simplePathFollow.looping;

            if (arraySize < 2)
                return;

            Handles.color = Color.yellow;

            switch (simplePathFollow.motion)
            {
                case SimplePathFollow.MOTION.LINEAR:
                    DrawLinearPath();
                    break;
                case SimplePathFollow.MOTION.SMOOTH:
                    DrawCatmullPath();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Handles.color = Color.white;
            for (int i = 0; i < arraySize; i++)
            {
                var point = GetWorldPathPoint(i);
                var handleSize = HandleUtility.GetHandleSize(point) * 0.075f;
                Handles.SphereHandleCap(0, point, Quaternion.identity, handleSize, EventType.Repaint);
            }

            return;

            void DrawLinearPath()
            {
                for (int i = 1; i < arraySize; i++)
                {
                    Handles.DrawLine(
                        GetWorldPathPoint(i - 1),
                        GetWorldPathPoint(i));
                }

                if (looping)
                    Handles.DrawLine(GetWorldPathPoint(arraySize - 1), GetWorldPathPoint(0));
            }

            void DrawCatmullPath()
            {
                var catmullResolution = simplePathFollow.catmullResolution;
                int segmentCount = looping ? arraySize : arraySize - 1;

                for (int i = 0; i < segmentCount; i++)
                {
                    Vector3 p0 = simplePathFollow.GetCatmullPoint(i - 1);
                    Vector3 p1 = simplePathFollow.GetCatmullPoint(i);
                    Vector3 p2 = simplePathFollow.GetCatmullPoint(i + 1);
                    Vector3 p3 = simplePathFollow.GetCatmullPoint(i + 2);

                    Vector3 previousPoint = p1;

                    for (int step = 1; step <= catmullResolution; step++)
                    {
                        float t = step / (float)catmullResolution;
                        Vector3 currentPoint = LerpFunctions.CatmullRom(t, p0, p1, p2, p3);
                        Handles.DrawLine(previousPoint, currentPoint);
                        previousPoint = currentPoint;
                    }
                }
            }

            Vector3 GetWorldPathPoint(int index, bool invert = false)
            {
                var point = simplePathFollow.pathPoints[index];
                return invert
                    ? simplePathFollow.transform.InverseTransformPoint(point)
                    : simplePathFollow.transform.TransformPoint(point);
            }
        }

        #endregion //OnDrawGizmos

        #region Add Point Preview

        private class Segment
        {
            public readonly Vector3 A;
            public readonly Vector3 B;
            public readonly int Index;

            public Segment(Vector3 a, Vector3 b, int index)
            {
                A = a;
                B = b;
                Index = index;
            }
        }
        
        private readonly Segment[] m_segments = new Segment[200];

        private void DrawNewPointPreview()
        {
            if (m_simplePathFollow.pathPoints.Count < 2)
                return;

            Event e = Event.current;

            float minDist = float.MaxValue;
            Vector3 bestPoint = Vector3.zero;
            int bestSegmentIndex = -1;

            var segmentCount = BuildSegmentsNonAlloc(m_segments);

            if (m_buttonWindowRect.Contains(Event.current.mousePosition))
                return;

            for (int i = 0; i < segmentCount; i++)
            {
                var seg = m_segments[i];

                float dist = DistanceMouseToSegment(
                    Event.current.mousePosition,
                    seg.A,
                    seg.B,
                    out Vector3 point
                );

                if (dist < minDist)
                {
                    minDist = dist;
                    bestPoint = point;
                    bestSegmentIndex = seg.Index;
                }
            }
            
            if (m_selectedIndex >= 0 && m_selectedIndex < m_simplePathFollow.pathPoints.Count)
            {
                var selectedWorldPos = m_simplePathFollow.transform.TransformPoint(
                    m_simplePathFollow.pathPoints[m_selectedIndex]);

                if (PointWithinThreshold(selectedWorldPos, bestPoint))
                    return;
            }

            float size = HandleUtility.GetHandleSize(bestPoint) * 0.1f;
            float threshold = 10f;//HandleUtility.GetHandleSize(bestPoint) * 0.2f;

            if (minDist < threshold)
            {
                Handles.color = Color.green;

                Handles.SphereHandleCap(
                    0,
                    bestPoint,
                    Quaternion.identity,
                    size,
                    EventType.Repaint
                );

                HandleUtility.Repaint();

                if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
                {
                    InsertPoint(bestPoint, bestSegmentIndex);
                    e.Use();
                }
            }

            bool PointWithinThreshold(Vector3 pointA, Vector3 pointB, float threshold = 20f)
            {
                var guiPointA = HandleUtility.WorldToGUIPoint(pointA);
                var guiPointB = HandleUtility.WorldToGUIPoint(pointB);

                return Vector2.Distance(guiPointA, guiPointB) < threshold;
            }
        }

        private int BuildSegmentsNonAlloc(Segment[] segments)
        {
            if (segments == null)
                return 0;

            var outCount = 0;

            switch (m_simplePathFollow.motion)
            {
                case SimplePathFollow.MOTION.LINEAR:
                    for (int i = 0; i < m_simplePathFollow.pathPoints.Count - 1 || outCount == segments.Length - 1; i++)
                    {
                        segments[outCount++] = new Segment(
                                GetPoint(i),
                                GetPoint(i + 1),
                                i); /*.Add(
                                (
                                GetPoint(i),
                                GetPoint(i + 1),
                                i
                            ));*/
                    }

                    if (m_simplePathFollow.looping)
                    {
                        segments[outCount++] = new Segment(
                            GetPoint(m_simplePathFollow.pathPoints.Count - 1),
                            GetPoint(0),
                            m_simplePathFollow.pathPoints.Count);
                    }
                    break;
                case SimplePathFollow.MOTION.SMOOTH:
                    int segmentCount = m_simplePathFollow.looping
                        ? m_simplePathFollow.pathPoints.Count
                        : m_simplePathFollow.pathPoints.Count - 1;

                    for (int i = 0; i < segmentCount && outCount < segments.Length; i++)
                    {
                        Vector3 p0 = m_simplePathFollow.GetCatmullPoint(i - 1);
                        Vector3 p1 = m_simplePathFollow.GetCatmullPoint(i);
                        Vector3 p2 = m_simplePathFollow.GetCatmullPoint(i + 1);
                        Vector3 p3 = m_simplePathFollow.GetCatmullPoint(i + 2);

                        for (int j = 0; j < m_simplePathFollow.catmullResolution && outCount < segments.Length; j++)
                        {
                            float t0 = j / (float)m_simplePathFollow.catmullResolution;
                            float t1 = (j + 1) / (float)m_simplePathFollow.catmullResolution;

                            Vector3 a = LerpFunctions.CatmullRom(t0, p0, p1, p2, p3);
                            Vector3 b = LerpFunctions.CatmullRom(t1, p0, p1, p2, p3);

                            segments[outCount++] = new Segment(a, b, i);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return outCount;
        }

        private Vector3 GetPoint(int index)
        {
            index = Mathf.Clamp(index, 0, m_simplePathFollow.pathPoints.Count - 1);
            return m_simplePathFollow.transform.TransformPoint(m_simplePathFollow.pathPoints[index]);
        }
        
        private float DistanceMouseToSegment(Vector2 mousePos, Vector3 worldA, Vector3 worldB, out Vector3 closestWorld)
        {
            closestWorld = Vector3.zero;
            
            Vector2 a = HandleUtility.WorldToGUIPoint(worldA);
            Vector2 b = HandleUtility.WorldToGUIPoint(worldB);
            
            if(Vector2.Distance(mousePos, a) < 20f || Vector2.Distance(mousePos, b) < 20f)
                return float.MaxValue;

            Vector2 ab = b - a;
            float length = ab.sqrMagnitude;

            float t = 0f;
            if (length > 0.0001f)
                t = Vector2.Dot(mousePos - a, ab) / length;

            t = Mathf.Clamp01(t);

            Vector2 closest2D = a + ab * t;
            closestWorld = Vector3.Lerp(worldA, worldB, t);
            
            return Vector2.Distance(mousePos, closest2D);
        }

        #endregion //Testing Point Add Preview
    }
}