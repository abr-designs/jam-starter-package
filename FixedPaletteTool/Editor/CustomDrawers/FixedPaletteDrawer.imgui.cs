using UnityEditor;
using UnityEngine;

namespace FixedColorPaletteTool
{
    internal partial class FixedPaletteDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = true;
            
            //Legal type safety
            //------------------------------------------------------------------//
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
            //------------------------------------------------------------------//
            
            var fixedPaletteAttribute = (FixedPaletteAttribute)attribute;
            var colorSelectType = fixedPaletteAttribute.ColorSelect;

            // Draw label
            position = EditorGUI.PrefixLabel(position, label);

            // Resolve current color
            var currentColorData = new ColorData(property.colorValue);

            // Layout: color box | dropdown
            float buttonWidth = 24f;
            float spacing = 6f;
            
            Rect buttonRect = new Rect(position.x + position.width - buttonWidth, 
                position.y, 
                buttonWidth, 
                EditorGUIUtility.singleLineHeight);

            Rect colorRect = new Rect(position.x, 
                position.y + 2, 
                position.width - buttonWidth - spacing, 
                EditorGUIUtility.singleLineHeight);

            // Draw color box
            EditorGUI.DrawRect(colorRect, currentColorData.color);

            // Dropdown button
            if (!GUI.Button(buttonRect, "▼")) 
                return;
            
            var window = ScriptableObject.CreateInstance<ColorSelectDropdownWindow>();

            window.Init(
                colorSelectType,
                FixedPaletteSettings.Instance.selectedPalette.colors, currentColorData,
                (selected) =>
                {
                    currentColorData.name = selected.name;
                    currentColorData.color = selected.color;

                    property.colorValue = selected.color;
                    property.serializedObject.ApplyModifiedProperties();
                }
            );

            //FIXME We need to adjust the positioning of the window
            //Show dropdown near mouse position (since IMGUI doesn’t have VisualElement rects)
            Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            ColorSelectDropdownWindow.GetExpectedSize(colorSelectType, out var width, out var height);

            Rect rect = new Rect(mousePos.x - width * 1.5f, mousePos.y - height, width, height);
            window.ShowAsDropDown(rect, new Vector2(width, height));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4;
        }
    }

}