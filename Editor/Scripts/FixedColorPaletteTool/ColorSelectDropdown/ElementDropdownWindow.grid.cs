using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Scripts.Utilities.Extensions;

namespace FixedColorPaletteTool
{

    public partial class ElementDropdownWindow
    {
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
        
        private static float GetExpectedGridWidth()
        {
            const int DEFAULT_WIDTH = 180;
            
            return DEFAULT_WIDTH;
        }
        private static float GetExpectedGridHeight()
        {
            const int DEFAULT_WIDTH = 180;
            const int LINE_PADDING = 8;
            
            var itemCount = FixedPaletteSettings.Instance.selectedPalette.colors.Count;

            int itemsPerRow = Mathf.FloorToInt(GetExpectedGridWidth() / (COLOR_BOX_SIZE + 4));
            int numberOfRows = Mathf.FloorToInt(itemCount / (float)itemsPerRow) + 1;
                
            return numberOfRows * (COLOR_BOX_SIZE + 4) + LINE_PADDING;
        }
    }
}