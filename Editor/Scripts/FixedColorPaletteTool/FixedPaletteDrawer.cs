using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

namespace FixedColorPaletteTool
{
    [CustomPropertyDrawer(typeof(FixedPaletteAttribute), true)]
    public class FixedPaletteDrawer : PropertyDrawer
    {
        //private int m_selectedIndex = -1;
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var colorOptions = FixedPaletteSettings.Instance.selectedPalette.colors;
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

            
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 4
                }
            };

            ColorData temp = null;
            var color = property.colorValue;
            var foundDataIndex = colorOptions.FindIndex(x => x.color == color);
            if (foundDataIndex < 0)
            {
                temp ??= new ColorData
                {
                    name = "Color not found",
                    color = Color.magenta
                };
            }
            else
            {
                temp = colorOptions[foundDataIndex];
            }

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
                    backgroundColor = new StyleColor(temp.color)
                }
            };
            container.Add(colorBox);

            // UI: label for name
            var label = new Label(temp.name)
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
                    name = temp.name,
                    color = temp.color
                };
                window.Init(colorOptions, current, (index, selected) =>
                {
                    temp.name = selected.name;
                    temp.color = selected.color;

                    //FIXME I want to be able to hotswap palettes without breaking refs. I assumed that index would be the ideal replacement
                    //m_selectedIndex = index;
                    
                    label.text = $"[{index}] {selected.name}";
                    colorBox.style.backgroundColor = new StyleColor(selected.color);
                    property.colorValue = selected.color;
                    property.serializedObject.ApplyModifiedProperties();
                }, GetColorDataName, GetColorDataColor);

                var rect = dropdownButton.worldBound;
                window.ShowAsDropDown(rect, new Vector2(180, colorOptions.Count * 22 + 8));
            };

            container.Add(dropdownButton);

            return container;
        }

        private static string GetColorDataName(ColorData colorData) => colorData.name;

        private static Color GetColorDataColor(ColorData colorData) => colorData.color;
    }

}