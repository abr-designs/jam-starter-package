using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Scripts.Utilities.Extensions;

namespace FixedColorPaletteTool
{
    public partial class ElementDropdownWindow : EditorWindow
    {
        private const float COLOR_BOX_SIZE = 20;

        private COLOR_SELECT m_colorSelectType;
        private List<ColorData> m_options;
        private ColorData m_current;
        private Action<int, ColorData> m_onSelect;
        
        private Func<ColorData, string> m_getName;
        private Func<ColorData, Color> m_getColor;

        public void Init(COLOR_SELECT colorSelectType, 
            List<ColorData> options, ColorData current,
            Action<int, ColorData> onSelect, 
            Func<ColorData, string> getName, 
            Func<ColorData, Color> getColor)
        {
            m_colorSelectType = colorSelectType;
            m_getName = getName;
            m_getColor = getColor;
            
            m_options = options;
            m_current = current;
            m_onSelect = onSelect;
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.SetPadding(6, 4);
            root.style.SetBorderColor(Color.grey);
            root.style.SetBorderWidth(0.5f);

            switch (m_colorSelectType)
            {
                case COLOR_SELECT.DEFAULT:
                {
                    DrawAsListDefault(root);
                    break;
                }
                case COLOR_SELECT.GRID:
                {
                    DrawAsGridDefault(root);
                    break;
                }
                case COLOR_SELECT.SHADES:
                {
                    DrawAsListShades(root);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //============================================================================================================//
        private void DrawAsListDefault(VisualElement root)
        {
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
                        height = COLOR_BOX_SIZE * 1.3f,
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

                var colorBox = DrawColorBox(colorOption);
                colorBox.style.SetPadding(6,4);
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
        
        //============================================================================================================//

        private VisualElement DrawColorBox(ColorData colorOption)
        {
            var colorBox = new VisualElement
            {
                style =
                {
                    width = COLOR_BOX_SIZE,
                    height = COLOR_BOX_SIZE,
                    backgroundColor = new StyleColor(m_getColor(colorOption)),
                    flexShrink = 0
                }
            };
            
            colorBox.style.SetBorderRadius(COLOR_BOX_SIZE / 2f);
            colorBox.style.SetBorderWidth(0.5f);
            colorBox.style.SetBorderColor(new Color32(30,30,30,255));

            return colorBox;
        }

        internal static void GetExpectedSize(COLOR_SELECT colorSelect, out float width, out float height)
        {
            switch (colorSelect)
            {
                case COLOR_SELECT.DEFAULT:
                    width = GetExpectedDefaultWidth();
                    height = GetExpectedDefaultHeight();
                    return;
                case COLOR_SELECT.GRID:
                    width =  GetExpectedGridWidth();
                    height =  GetExpectedGridHeight();
                    return;
                case COLOR_SELECT.SHADES:
                    width =  GetExpectedShadesWidth();
                    height = GetExpectedShadesHeight();
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(colorSelect), colorSelect, null);
            }
        }
        
        //============================================================================================================//
        
        private static float GetExpectedDefaultWidth()
        {
            const int DEFAULT_WIDTH = 180;
            
            return DEFAULT_WIDTH;
        }
        private static float GetExpectedDefaultHeight()
        {
            const int LINE_HEIGHT = 22;
            const int LINE_PADDING = 8;
            
            var itemCount = FixedPaletteSettings.Instance.selectedPalette.colors.Count;

            return itemCount * (LINE_HEIGHT+ 2) + LINE_PADDING;
        }

        
        //============================================================================================================//
    }
}