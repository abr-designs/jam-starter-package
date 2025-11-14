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
        private System.Action<int, ColorData> m_onSelect;
        
        private Unity.Plastic.Antlr3.Runtime.Misc.Func<ColorData, string> m_getName;
        private Unity.Plastic.Antlr3.Runtime.Misc.Func<ColorData, Color> m_getColor;

        public void Init(COLOR_SELECT colorSelectType, List<ColorData> options, ColorData current, System.Action<int, ColorData> onSelect, Unity.Plastic.Antlr3.Runtime.Misc.Func<ColorData, string> getName, Unity.Plastic.Antlr3.Runtime.Misc.Func<ColorData, Color> getColor)
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
                    if (FixedPaletteSettings.Instance.dropdownAsGrid)
                        DrawAsGridDefault(root);
                    else
                        DrawAsListDefault(root);
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

        private void DrawAsGridDefault(VisualElement root)
        {
            int itemsPerRow = Mathf.FloorToInt(this.position.width / (COLOR_BOX_SIZE + 4));
            int numberOfRows = Mathf.FloorToInt(m_options.Count / (float)itemsPerRow);
            
            var c = 0;
            var row = CreateRow();
            for (var i = 0; i < m_options.Count; i++, c++)
            {
                var index = i;

                if (c >= itemsPerRow)
                {
                    c = 0;
                    row = CreateRow();
                }
                
                var colorOption = m_options[i];
                var gridContainer = CreateGridSlot();
                var colorBox = DrawColorBox(colorOption);

                gridContainer.Add(colorBox);

                var color = row.style.backgroundColor;
                gridContainer.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    gridContainer.style.backgroundColor = new StyleColor(Color.gray);
                });

                gridContainer.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    gridContainer.style.backgroundColor = color;
                });

                // Highlight current
                if (colorOption.Equals(m_current))
                    gridContainer.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.2f));

                gridContainer.RegisterCallback<ClickEvent>(_ =>
                {
                    m_onSelect?.Invoke(index, colorOption);
                    Close();
                });

                row.Add(gridContainer);
            }

            return;

            VisualElement CreateRow()
            {
                var newRow = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        flexShrink = 0
                    }
                    
                };
                root.Add(newRow);
                return newRow;
            }
            VisualElement CreateGridSlot()
            {
                var gridSlot = new VisualElement
                {
                    style =
                    {
                        alignItems = Align.Center,
                        flexShrink = 0
                    }
                    
                };
                gridSlot.style.SetPadding(2);
                
                return gridSlot;
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

            return colorBox;
        }

        public static float GetExpectedHeight(float width, bool asGrid, COLOR_SELECT colorSelect)
        {
            const int LINE_HEIGHT = 22;
            const int LINE_PADDING = 8;
            
            var itemCount = FixedPaletteSettings.Instance.selectedPalette.colors.Count;

            if (!asGrid || colorSelect == COLOR_SELECT.SHADES) 
                return itemCount * (LINE_HEIGHT+ 2) + LINE_PADDING;
            
            
            int itemsPerRow = Mathf.FloorToInt(width / (COLOR_BOX_SIZE + 4));
            int numberOfRows = Mathf.FloorToInt(itemCount / (float)itemsPerRow) + 1;
                
            return numberOfRows * (COLOR_BOX_SIZE + 4) + LINE_PADDING;

        }

        
        //============================================================================================================//
    }
}