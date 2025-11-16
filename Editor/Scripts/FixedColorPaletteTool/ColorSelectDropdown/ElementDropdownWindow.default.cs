using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Scripts.Utilities.Extensions;

namespace FixedColorPaletteTool
{
    internal partial class ColorSelectDropdownWindow : EditorWindow
    {
        private const float COLOR_BOX_SIZE = 20;
        private const int DEFAULT_WIDTH = 180;
        private const int LINE_PADDING = 8;
        
        private static readonly Color HIGHLIGHT_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.2f);

        private COLOR_SELECT m_colorSelectType;
        private List<ColorData> m_options;
        private ColorData m_current;
        private Action<ColorData> m_onSelect;
        
        internal void Init(COLOR_SELECT colorSelectType, 
            List<ColorData> options, 
            ColorData current,
            Action<ColorData> onSelect)
        {
            m_colorSelectType = colorSelectType;
            
            m_options = options;
            m_current = current;
            m_onSelect = onSelect;
        }

        private void CreateGUI()
        {
            //Create the window container
            var root = rootVisualElement;
            root.style.SetPadding(6, 4);
            root.style.SetBorderColor(Color.grey);
            root.style.SetBorderWidth(0.5f);

            //Fill the container with color options
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

        /// <summary>
        /// Creates a Circle VisualElement with a const size & uses the specified color
        /// </summary>
        /// <param name="colorOption"></param>
        /// <returns></returns>
        private static VisualElement DrawColorIcon(ColorData colorOption)
        {
            var colorBox = new VisualElement
            {
                style =
                {
                    width = COLOR_BOX_SIZE,
                    height = COLOR_BOX_SIZE,
                    backgroundColor = new StyleColor(colorOption.color),
                    flexShrink = 0
                }
            };
            
            colorBox.style.SetBorderRadius(COLOR_BOX_SIZE / 2f);
            colorBox.style.SetBorderWidth(0.5f);
            colorBox.style.SetBorderColor(new Color32(30,30,30,255));

            return colorBox;
        }

        /// <summary>
        /// Used to determine the size of the window based on the display style & the number of anticipated elements
        /// </summary>
        /// <param name="colorSelect"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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

        #region Default List Draw

        /// <summary>
        /// Draws a vertical list of all the palettes colors, with the user specified names
        /// </summary>
        /// <param name="root"></param>
        private void DrawAsListDefault(VisualElement root)
        {
            for (var i = 0; i < m_options.Count; i++)
            {
                var colorOption = m_options[i];
                var optionName = colorOption.name;
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
                row.RegisterCallback<MouseEnterEvent>(_ =>
                {
                    row.style.backgroundColor = new StyleColor(Color.gray);
                });

                row.RegisterCallback<MouseLeaveEvent>(_ =>
                {
                    row.style.backgroundColor = color;
                });

                var colorBox = DrawColorIcon(colorOption);
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
                    row.style.backgroundColor = new StyleColor(HIGHLIGHT_COLOR);

                row.RegisterCallback<ClickEvent>(_ =>
                {
                    m_onSelect?.Invoke(colorOption);
                    Close();
                });

                root.Add(row);
            }
        }

        private static float GetExpectedDefaultWidth() => DEFAULT_WIDTH;
    
        private static float GetExpectedDefaultHeight()
        {
            const int LINE_HEIGHT = 22;
            
            var itemCount = FixedPaletteSettings.Instance.selectedPalette.colors.Count;

            return itemCount * (LINE_HEIGHT+ 2) + LINE_PADDING;
        }

        #endregion //Default List Draw

        
        //============================================================================================================//
    }
}