using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Utilities;
using Utilities.Animations;
using Object = UnityEngine.Object;

namespace JamStarter.Editor.Scripts.Utilities.Animations
{
    [CustomEditor(typeof(SimplePathFollow))]
    public class SimplePathFollowEditor : UnityEditor.Editor
    {
        private SimplePathFollow m_simplePathFollow;
        private SerializedProperty m_pathPointsProp;

        private int m_selectedPathPointIndex = -1;

        private static readonly Color LabelTextColor = Color.white;
        private static readonly Color LabelBackgroundColor = new Color(0, 0, 0, 0.6f);
        private static readonly Color DeleteButtonColor = new Color(0.8588f, 0.2431f, 0.1137f);
        private static readonly Color InsertPointColor = new Color32(90, 184, 92, 255);
        
        private Rect m_buttonWindowRect;
        

        //Unity Editor Functions
        //================================================================================================================//

        private void OnEnable()
        {
            m_simplePathFollow = (SimplePathFollow)target;
            m_pathPointsProp = serializedObject.FindProperty(nameof(SimplePathFollow.pathPoints));
            
            if(PathHandleGlobal.SelectedPathIndex >= 0)
            {
                SelectPoint(PathHandleGlobal.SelectedPathIndex);
                PathHandleGlobal.SelectedPathIndex = -1;
            }
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
                if (i == m_selectedPathPointIndex)
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
            if (m_selectedPathPointIndex == -1)
                return;
            if (m_selectedPathPointIndex >= pathPoints.Count)
            {
                m_selectedPathPointIndex = -1;
                return;
            }

            DrawUtilityWindow();

            serializedObject.Update();

            var currentPoint = m_pathPointsProp.GetArrayElementAtIndex(m_selectedPathPointIndex);

            if (currentPoint == null)
            {
                m_selectedPathPointIndex = -1;
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

        //================================================================================================================//

        private void SelectPoint(int index)
        {
            m_selectedPathPointIndex = index;
        }

        private static void DrawLabel(Vector3 position, string text)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                normal =
                {
                    textColor = LabelTextColor
                },
                alignment = TextAnchor.MiddleCenter
            };

            var size = style.CalcSize(new GUIContent(text));

            Handles.BeginGUI();

            var screenPos = HandleUtility.WorldToGUIPoint(position);

            screenPos.x += (size.x / 2f) * 1.3f;

            var rect = new Rect(
                screenPos.x - size.x / 2,
                screenPos.y - size.y / 2,
                size.x,
                size.y
            );

            // Draw background
            EditorGUI.DrawRect(rect, LabelBackgroundColor);

            // Draw label
            GUI.Label(rect, text, style);

            Handles.EndGUI();
        }

        //Drawing Floating Window
        //================================================================================================================//

        private void DrawUtilityWindow()
        {
            const string DELETE_ICON ="TreeEditor.Trash";
            const string ADD_ICON = "Toolbar Plus";
            
            const int BUTTON_WIDTH = 30;
            const int BUTTON_HEIGHT = 30;
            
            if (m_simplePathFollow.pathPoints.Count < 2)
                return;

            var deleteOnly = m_selectedPathPointIndex != m_simplePathFollow.pathPoints.Count - 1 || m_simplePathFollow.looping;
            var worldPathPoint = m_simplePathFollow.transform.TransformPoint(m_simplePathFollow.pathPoints[m_selectedPathPointIndex]);
            var guiPos = HandleUtility.WorldToGUIPoint(worldPathPoint);
            var rectWidth = deleteOnly ? BUTTON_WIDTH : BUTTON_WIDTH * 2;

            Handles.BeginGUI();

            m_buttonWindowRect = new Rect(guiPos.x + 15, guiPos.y - 20, rectWidth, BUTTON_HEIGHT);
            GUILayout.BeginArea(m_buttonWindowRect);

            GUILayout.BeginHorizontal();

            var removeIcon = EditorGUIUtility.IconContent(DELETE_ICON);
            removeIcon.tooltip = "Delete Path Point";

            //We only want to show the Add button when we're not looping & it's an end point
            //----------------------------------------------------------//
            if (!deleteOnly)
            {
                var addIcon = EditorGUIUtility.IconContent(ADD_ICON);

                if (GUILayout.Button(addIcon))
                    AddPoint(m_simplePathFollow, m_selectedPathPointIndex + 1);
            }
            //----------------------------------------------------------//


            var oldColor = GUI.color;

            GUI.color = DeleteButtonColor;
            
            //Delete Point
            //----------------------------------------------------------//
            if (GUILayout.Button(removeIcon))
                DeletePoint(m_simplePathFollow, m_selectedPathPointIndex);
            //----------------------------------------------------------//

            GUI.color = oldColor;

            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            Handles.EndGUI();
        }

        //Adding Points
        //================================================================================================================//

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

            m_selectedPathPointIndex = Mathf.Clamp(m_selectedPathPointIndex - 1, 0, path.pathPoints.Count - 1);

            EditorUtility.SetDirty(path);
        }

        //Add Point Preview
        //================================================================================================================//

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

            var e = Event.current;

            var minDist = float.MaxValue;
            var bestPoint = Vector3.zero;
            var bestSegmentIndex = -1;

            //We don't want to add points when attempting to select a path point
            if (IsMouseCloseToPathPoint(
                    m_simplePathFollow.transform, 
                    m_simplePathFollow.pathPoints,
                    Event.current.mousePosition))
                return;

            var segmentCount = BuildSegmentsNonAlloc(m_segments);

            if (segmentCount == 0)
                return;

            if (m_buttonWindowRect.Contains(Event.current.mousePosition))
                return;

            //Find the best segment, closest to mouse point
            //----------------------------------------------------------//
            for (var i = 0; i < segmentCount; i++)
            {
                var seg = m_segments[i];

                var dist = DistanceMouseToSegment(
                    Event.current.mousePosition,
                    seg.A,
                    seg.B,
                    out var point
                );

                if (dist >= minDist)
                    continue;
                
                minDist = dist;
                bestPoint = point;
                bestSegmentIndex = seg.Index;
            }

            //Give the currently selected point UX priority, so that a new point isn't created when trying to move
            //----------------------------------------------------------//
            if (m_selectedPathPointIndex >= 0 && m_selectedPathPointIndex < m_simplePathFollow.pathPoints.Count)
            {
                var selectedWorldPos = m_simplePathFollow.transform.TransformPoint(
                    m_simplePathFollow.pathPoints[m_selectedPathPointIndex]);

                if (PointWithinThreshold(selectedWorldPos, bestPoint))
                    return;
            }

            //Determine if the best segment option is within a certain distance of the mouse point
            //----------------------------------------------------------//

            var size = HandleUtility.GetHandleSize(bestPoint) * 0.2f;
            var threshold = 10f;

            if (minDist < threshold)
            {
                Handles.color = InsertPointColor;
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
                    for (var i = 0; i < m_simplePathFollow.pathPoints.Count - 1 || outCount == segments.Length - 1; i++)
                    {
                        segments[outCount++] = new Segment(
                                GetPoint(i),
                                GetPoint(i + 1),
                                i);
                    }

                    if (m_simplePathFollow.looping)
                    {
                        segments[outCount++] = new Segment(
                            GetPoint(m_simplePathFollow.pathPoints.Count - 1),
                            GetPoint(0),
                            m_simplePathFollow.pathPoints.Count - 1);
                    }
                    break;
                case SimplePathFollow.MOTION.SMOOTH:
                    var segmentCount = m_simplePathFollow.looping
                        ? m_simplePathFollow.pathPoints.Count
                        : m_simplePathFollow.pathPoints.Count - 1;

                    for (var i = 0; i < segmentCount && outCount < segments.Length; i++)
                    {
                        var p0 = m_simplePathFollow.GetCatmullPoint(i - 1);
                        var p1 = m_simplePathFollow.GetCatmullPoint(i);
                        var p2 = m_simplePathFollow.GetCatmullPoint(i + 1);
                        var p3 = m_simplePathFollow.GetCatmullPoint(i + 2);

                        for (var j = 0; j < m_simplePathFollow.catmullResolution && outCount < segments.Length; j++)
                        {
                            var t0 = j / (float)m_simplePathFollow.catmullResolution;
                            var t1 = (j + 1) / (float)m_simplePathFollow.catmullResolution;

                            var a = LerpFunctions.CatmullRom(t0, p0, p1, p2, p3);
                            var b = LerpFunctions.CatmullRom(t1, p0, p1, p2, p3);

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
        
        private static float DistanceMouseToSegment(Vector2 mousePos, Vector3 worldPointA, Vector3 worldPointB, out Vector3 closestWorldPoint)
        {
            closestWorldPoint = Vector3.zero;
            
            var a = HandleUtility.WorldToGUIPoint(worldPointA);
            var b = HandleUtility.WorldToGUIPoint(worldPointB);

            var ab = b - a;
            var lengthSqr = ab.sqrMagnitude;

            var t = 0f;
            if (lengthSqr > 0.0001f)
                t = Vector2.Dot(mousePos - a, ab) / lengthSqr;

            t = Mathf.Clamp01(t);

            var closest2D = a + ab * t;
            closestWorldPoint = Vector3.Lerp(worldPointA, worldPointB, t);
            
            return Vector2.Distance(mousePos, closest2D);
        }

        private static bool IsMouseCloseToPathPoint(Transform transform, IEnumerable<Vector3> pathPoints, Vector3 mousePos)
        {
            foreach (var pathPoint in pathPoints)
            {
                var worldPoint = transform.TransformPoint(pathPoint);
                var guiPoint = HandleUtility.WorldToGUIPoint(worldPoint);
                
                if (Vector2.Distance(mousePos, guiPoint) < 20f)
                    return true;
                
            }

            return false;
        }

        #endregion //Testing Point Add Preview

        //OnDrawGizmos
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

        //================================================================================================================//

    }
    
    [InitializeOnLoad]
    internal static class PathRegistry
    {
        private static List<SimplePathFollow> s_paths = new();
        private static bool s_dirty = true;

        static PathRegistry()
        {
            EditorApplication.hierarchyChanged += () => s_dirty = true;
            Undo.undoRedoPerformed += () => s_dirty = true;
        }

        public static List<SimplePathFollow> Paths
        {
            get
            {
                if (!s_dirty) 
                    return s_paths;
                
                s_paths = Object.FindObjectsByType<SimplePathFollow>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID).ToList();
                s_dirty = false;
                return s_paths;
            }
        }
    }
    
    [InitializeOnLoad]
    internal static class PathHandleGlobal
    {
        internal static int SelectedPathIndex { get; set; } = -1;
        static PathHandleGlobal()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            foreach (var simplePathFollow in PathRegistry.Paths)
            {
                if(Selection.activeObject == simplePathFollow)
                    continue;
                
                if (simplePathFollow.pathPoints == null) 
                    continue;

                for (var i = 0; i < simplePathFollow.pathPoints.Count; i++)
                {
                    var point = simplePathFollow.pathPoints[i];
                    var worldPoint = simplePathFollow.transform.TransformPoint(point);
                    float size = HandleUtility.GetHandleSize(worldPoint) * 0.1f;

                    int controlID = GUIUtility.GetControlID(FocusType.Passive);
                    float distance = HandleUtility.DistanceToCircle(worldPoint, size);

                    HandleUtility.AddControl(controlID, distance);

                    if (HandleUtility.nearestControl == controlID && Event.current.type == EventType.MouseDown)
                    {
                        GUIUtility.hotControl = controlID;

                        // Handle click
                        Selection.activeObject = simplePathFollow;

                        Event.current.Use();
                        SelectedPathIndex = i;
                    }
                }
            }
        }
    }
}