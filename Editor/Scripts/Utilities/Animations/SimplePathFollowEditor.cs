using UnityEngine;
using UnityEditor;
using Utilities.Animations;

namespace JamStarter.Editor.Scripts.Utilities.Animations
{
    [CustomEditor(typeof(SimplePathFollow))]
    public class SimplePathFollowEditor : UnityEditor.Editor
    {
        private Tool previousTool;

        private void OnEnable()
        {
            previousTool = Tools.current;
            Tools.current = Tool.None;
        }

        private void OnDisable()
        {
            Tools.current = previousTool;
        }
        
        private void OnSceneGUI()
        {
            SimplePathFollow path = (SimplePathFollow)target;
            if (path == null) return;

            SerializedProperty pointsProp = serializedObject.FindProperty("pathPoints");
            if (pointsProp == null || !pointsProp.isArray) return;

            for (int i = 0; i < pointsProp.arraySize; i++)
            {
                var element = pointsProp.GetArrayElementAtIndex(i);
                Transform point = element.objectReferenceValue as Transform;

                if (point == null) continue;

                EditorGUI.BeginChangeCheck();

                Vector3 newPos = Handles.PositionHandle(point.position, point.rotation);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(point, "Move Path Point");
                    point.position = newPos;
                }
            }
        }
    }
}