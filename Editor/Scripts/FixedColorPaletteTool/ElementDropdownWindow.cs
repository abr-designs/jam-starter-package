using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Scripts.Utilities.Extensions;
using Unity.Plastic.Antlr3.Runtime.Misc;

namespace FixedColorPaletteTool
{
    public class ElementDropdownWindow : EditorWindow
    {
        private List<ColorData> m_options;
        private ColorData m_current;
        private System.Action<int, ColorData> m_onSelect;
        
        private Func<ColorData, string> m_getName;
        private Func<ColorData, Color> m_getColor;

        public void Init(List<ColorData> options, ColorData current, System.Action<int, ColorData> onSelect, Func<ColorData, string> getName, Func<ColorData, Color> getColor)
        {
            m_getName = getName;
            m_getColor = getColor;
            
            m_options = options;
            m_current = current;
            m_onSelect = onSelect;
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingTop = 4;
            root.style.paddingBottom = 4;
            root.style.paddingLeft = 6;
            root.style.paddingRight = 6;
            root.style.SetBorderColor(Color.grey);
            root.style.SetBorderWidth(0.5f);

            for (var i = 0; i < m_options.Count; i++)
            {
                var index = i;
                
                var colorOption = m_options[i];
                var optionName = m_getName(colorOption);
                var row = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        marginBottom = 2,
                        paddingLeft = 2,
                        paddingRight = 2,
                        height = 20,
                    }
                    
                };
                var color = row.style.backgroundColor;
                row.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    row.style.backgroundColor = new StyleColor(Color.gray);
                });

                row.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    row.style.backgroundColor = color;
                });

                var colorBox = new VisualElement
                {
                    style =
                    {
                        width = 14,
                        height = 14,
                        marginRight = 6,
                        backgroundColor = new StyleColor(m_getColor(colorOption)),
                        borderTopLeftRadius = 2,
                        borderTopRightRadius = 2,
                        borderBottomLeftRadius = 2,
                        borderBottomRightRadius = 2
                    }
                };
                row.Add(colorBox);

                var label = new Label($"{optionName}")
                {
                    style =
                    {
                        flexGrow = 1
                    }
                };
                row.Add(label);

                // Highlight current
                if (colorOption.Equals(m_current))
                    row.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.2f));

                row.RegisterCallback<ClickEvent>(_ =>
                {
                    m_onSelect?.Invoke(index, colorOption);
                    Close();
                });

                root.Add(row);
            }
        }
    }
}