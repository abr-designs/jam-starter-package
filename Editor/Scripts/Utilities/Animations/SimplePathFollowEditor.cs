using System;
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
        private SerializedProperty m_currentPoint;

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

            for (int i = 0; i < m_pathPointsProp.arraySize; i++)
            {
                if(i == m_selectedIndex)
                    continue;
                
                var element = m_pathPointsProp.GetArrayElementAtIndex(i);
                Vector3 point = m_simplePathFollow.transform.TransformPoint(element.vector3Value);
                
                //var handleSize = HandleUtility.GetHandleSize(point) * 0.2f;
                
                Handles.color = Color.red;
                if (Handles.Button(point, Quaternion.identity, 0.5f, 0.5f * 1.5f, Handles.SphereHandleCap))
                {
                    SelectPoint(i);
                }
                
                DrawLabel(point, $"Element {i}");
            }

            if (m_selectedIndex == -1)
                return;
            if (m_currentPoint == null)
            {
                m_selectedIndex = -1;
                return;
            }
                
            EditorGUI.BeginChangeCheck();
            var currentPointPosition = m_simplePathFollow.transform.TransformPoint(m_currentPoint.vector3Value);
            var newPos = Handles.PositionHandle(currentPointPosition, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.Update();
                m_currentPoint.vector3Value = m_simplePathFollow.transform.InverseTransformPoint(newPos);

                serializedObject.ApplyModifiedProperties();
            }
        }

        private void SelectPoint(int index)
        {
            m_selectedIndex = index;
            m_currentPoint = m_pathPointsProp.GetArrayElementAtIndex(m_selectedIndex);
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

        //================================================================================================================//

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
                        GetWorldPathPoint(i-1), 
                        GetWorldPathPoint(i));
                }
            
                if(looping)
                    Handles.DrawLine(GetWorldPathPoint(arraySize-1), GetWorldPathPoint(0));
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
                return invert ? 
                    simplePathFollow.transform.InverseTransformPoint(point) : 
                    simplePathFollow.transform.TransformPoint(point);
            }
        }
    }
}