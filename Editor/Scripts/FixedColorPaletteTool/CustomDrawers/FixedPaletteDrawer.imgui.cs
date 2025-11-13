using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace FixedColorPaletteTool
{

    public partial class FixedPaletteDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            /*var myAttr = (FixedPaletteAttribute)attribute;
            if (myAttr is { DefaultInspector: true })
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }*/
            
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

            // Get palette
            List<ColorData> colorOptions = FixedPaletteSettings.Instance.selectedPalette.colors;

            // Resolve current color
            Color currentColor = property.colorValue;
            int foundIndex = colorOptions.FindIndex(x => x.color == currentColor);
            ColorData temp;

            if (foundIndex < 0)
                temp = new ColorData { name = "Color not found", color = Color.magenta };
            else
                temp = colorOptions[foundIndex];

            // Layout: color box | label | dropdown
            float colorBoxSize = 16f;
            float buttonWidth = 24f;
            float spacing = 6f;

            Rect colorRect = new Rect(position.x, position.y + 2, colorBoxSize, colorBoxSize);
            Rect labelRect = new Rect(position.x + colorBoxSize + spacing, position.y,
                position.width - colorBoxSize - spacing - buttonWidth, EditorGUIUtility.singleLineHeight);
            Rect buttonRect = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth,
                EditorGUIUtility.singleLineHeight);

            // Draw color box
            EditorGUI.DrawRect(colorRect, temp.color);
            GUI.Box(colorRect, GUIContent.none); // border

            // Draw label
            EditorGUI.LabelField(labelRect, temp.name);

            // Dropdown button
            if (GUI.Button(buttonRect, "▼"))
            {
                ElementDropdownWindow window = ScriptableObject.CreateInstance<ElementDropdownWindow>();

                ColorData current = new ColorData
                {
                    name = temp.name,
                    color = temp.color
                };

                window.Init(
                    FixedPaletteSettings.Instance.selectedPalette.colors,
                    current,
                    (index, selected) =>
                    {
                        temp.name = selected.name;
                        temp.color = selected.color;

                        property.colorValue = selected.color;
                        property.serializedObject.ApplyModifiedProperties();
                    },
                    GetColorDataName,
                    GetColorDataColor
                );

                // Show dropdown near mouse position (since IMGUI doesn’t have VisualElement rects)
                Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                Rect rect = new Rect(mousePos.x - 180, mousePos.y - 90, 180, FixedPaletteSettings.Instance.selectedPalette.colors.Count * 22 + 8);
                window.ShowAsDropDown(rect, new Vector2(180, FixedPaletteSettings.Instance.selectedPalette.colors.Count * 22 + 8));
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 8;
        }
    }

}