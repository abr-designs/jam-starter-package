using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

namespace FixedColorPaletteTool
{
#if FIXED_COLOR_INSPECTOR
    [CustomPropertyDrawer(typeof(Color), true)]
    [CustomPropertyDrawer(typeof(Color32), true)]
#endif
    [CustomPropertyDrawer(typeof(FixedPaletteAttribute), true)]
    public partial class FixedPaletteDrawer : PropertyDrawer
    {
        //private int m_selectedIndex = -1;
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Ensure we’re working with a Color or Color32
            if (property.propertyType != SerializedPropertyType.Color)
            {
                var error = new Label("[FixedPalette] only works on Color or Color32 fields.")
                {
                    style =
                    {
                        backgroundColor = new StyleColor(Color.red),
                        color = new StyleColor(Color.white),
                        unityFontStyleAndWeight = FontStyle.BoldAndItalic
                    }
                };
                return error;
            }
            
            var fixedPaletteAttribute = (FixedPaletteAttribute)attribute;
            var colorSelectType = fixedPaletteAttribute.ColorSelect;
            
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };

            var currentColorData = new ColorData(property.colorValue);

            // UI: color square
            var colorBox = new VisualElement
            {
                style =
                {
                    width = 16,
                    height = 16,
                    marginRight = 6,
                    borderTopLeftRadius = 2,
                    borderTopRightRadius = 2,
                    borderBottomLeftRadius = 2,
                    borderBottomRightRadius = 2,
                    backgroundColor = new StyleColor(currentColorData.color)
                }
            };
            container.Add(colorBox);

            // UI: label for name
            var label = new Label(currentColorData.name)
            {
                style = { flexGrow = 1 }
            };
            container.Add(label);

            // Dropdown button
            var dropdownButton = new Button()
            {
                text = "▼",
                style =
                {
                    width = 24
                }
            };

            dropdownButton.clicked += () =>
            {
                var window = ScriptableObject.CreateInstance<ElementDropdownWindow>();
                var current = new ColorData
                {
                    name = currentColorData.name,
                    color = currentColorData.color
                };
                window.Init(
                    colorSelectType,
                    FixedPaletteSettings.Instance.selectedPalette.colors, 
                    current, 
                    (index, selected) =>
                {
                    currentColorData.name = selected.name;
                    currentColorData.color = selected.color;

                    //FIXME I want to be able to hotswap palettes without breaking refs. I assumed that index would be the ideal replacement
                    //m_selectedIndex = index;
                    
                    label.text = $"{selected.name}";
                    colorBox.style.backgroundColor = new StyleColor(selected.color);
                    property.colorValue = selected.color;
                    property.serializedObject.ApplyModifiedProperties();
                }, GetColorDataName, GetColorDataColor);

                ElementDropdownWindow.GetExpectedSize(colorSelectType, out var windowWidth, out var windowHeight);
                
                var rect = dropdownButton.GetScreenBound();
                rect.x -= windowWidth * 1.5f;
                
                window.ShowAsDropDown(rect, new Vector2(windowWidth, windowHeight));
                window.position = rect;
            };

            container.Add(dropdownButton);

            return container;
        }

        private static string GetColorDataName(ColorData colorData) => colorData.name;

        private static Color GetColorDataColor(ColorData colorData) => colorData.color;
    }

}