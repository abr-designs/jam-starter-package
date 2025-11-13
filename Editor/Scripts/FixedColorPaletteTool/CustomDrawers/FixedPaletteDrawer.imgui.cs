using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace FixedColorPaletteTool
{

    public partial class FixedPaletteDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = true;
            
            if (property.propertyType != SerializedPropertyType.Color)
            {
                var errorRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

                // Draw red background
                EditorGUI.DrawRect(errorRect, Color.red);

                // White bold italic label
                var style = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = Color.white },
                    fontStyle = FontStyle.BoldAndItalic,
                    alignment = TextAnchor.MiddleLeft
                };

                EditorGUI.LabelField(errorRect, "[FixedPalette] only works on Color or Color32 fields.", style);
                return;
            }

            // Draw label
            position = EditorGUI.PrefixLabel(position, label);

            // Resolve current color
            var currentColorData = new ColorData(property.colorValue);

            // Layout: color box | label | dropdown
            float colorBoxSize = 16f;
            float buttonWidth = 24f;
            float spacing = 6f;

            Rect colorRect = new Rect(position.x, position.y + 2, colorBoxSize, colorBoxSize);
            Rect labelRect = new Rect(position.x + colorBoxSize + spacing, position.y, position.width - colorBoxSize - spacing - buttonWidth, EditorGUIUtility.singleLineHeight);
            Rect buttonRect = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);

            // Draw color box
            EditorGUI.DrawRect(colorRect, currentColorData.color);
            //GUI.Box(colorRect, GUIContent.none); // border

            // Draw label
            EditorGUI.LabelField(labelRect, currentColorData.name);

            // Dropdown button
            if (!GUI.Button(buttonRect, "▼")) 
                return;
            
            var window = ScriptableObject.CreateInstance<ElementDropdownWindow>();

            window.Init(FixedPaletteSettings.Instance.selectedPalette.colors, currentColorData,
                (index, selected) =>
                {
                    currentColorData.name = selected.name;
                    currentColorData.color = selected.color;

                    property.colorValue = selected.color;
                    property.serializedObject.ApplyModifiedProperties();
                },
                GetColorDataName,
                GetColorDataColor
            );

            // Show dropdown near mouse position (since IMGUI doesn’t have VisualElement rects)
            Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            var width = 180;
            var height = ElementDropdownWindow.GetExpectedHeight(width, FixedPaletteSettings.Instance.dropdownAsGrid);
            Rect rect = new Rect(mousePos.x - width, mousePos.y - height, width, height);
            window.ShowAsDropDown(rect, new Vector2(width, height));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4;
        }
    }

}