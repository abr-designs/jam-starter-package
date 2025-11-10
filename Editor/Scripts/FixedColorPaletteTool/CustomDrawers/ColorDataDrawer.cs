using UnityEditor;
using UnityEngine;
namespace FixedColorPaletteTool.CustomDrawers
{
    [CustomPropertyDrawer(typeof(ColorData))]
    public class ColorDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Remove indent so it lines up nicely
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Get sub-properties
            var nameProp = property.FindPropertyRelative(nameof(ColorData.name));
            var colorProp = property.FindPropertyRelative(nameof(ColorData.color));

            // Calculate field widths
            float colorWidth = 120f;
            float spacing = 5f;
            float nameWidth = position.width - colorWidth - spacing;

            // Draw inline fields
            Rect colorRect = new Rect(position.x, position.y, colorWidth, position.height);
            Rect nameRect = new Rect(position.x + colorWidth + spacing, position.y, nameWidth, position.height);

            EditorGUI.PropertyField(colorRect, colorProp, GUIContent.none);
            EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);

            EditorGUI.indentLevel = oldIndent;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}