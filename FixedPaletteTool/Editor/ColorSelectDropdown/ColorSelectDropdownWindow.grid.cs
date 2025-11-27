using UnityEngine;
using UnityEngine.UIElements;
using Scripts.Utilities.Extensions;

namespace FixedColorPaletteTool
{
    internal partial class ColorSelectDropdownWindow
    {
        private void DrawAsGridDefault(VisualElement root)
        {
            var itemsPerRow = Mathf.FloorToInt(this.position.width / (COLOR_BOX_SIZE + 4));
            
            var row = CreateRow();
            for (int i = 0,  c = 0; i < m_options.Count; i++, c++)
            {
                var index = i;

                if (c >= itemsPerRow)
                {
                    c = 0;
                    row = CreateRow();
                }
                
                var colorOption = m_options[i];
                var gridContainer = CreateGridSlot();
                var colorBox = DrawColorIcon(colorOption);

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
                    gridContainer.style.backgroundColor = new StyleColor(HIGHLIGHT_COLOR);

                gridContainer.RegisterCallback<ClickEvent>(_ =>
                {
                    m_onSelect?.Invoke(colorOption);
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
            var itemCount = FixedPaletteSettings.Instance.selectedPalette.colors.Count;
            var itemsPerRow = Mathf.FloorToInt(DEFAULT_WIDTH / (COLOR_BOX_SIZE + 4));

            //If the number of options is would be too small, resize the window to fit the new min
            if (itemCount < itemsPerRow)
                return itemCount * (COLOR_BOX_SIZE + 4) + LINE_PADDING * 2f;
            
            return DEFAULT_WIDTH;
        }
        private static float GetExpectedGridHeight()
        {
            var itemCount = FixedPaletteSettings.Instance.selectedPalette.colors.Count;

            int itemsPerRow = Mathf.FloorToInt(GetExpectedGridWidth() / (COLOR_BOX_SIZE + 4));
            int numberOfRows = Mathf.FloorToInt(itemCount / (float)itemsPerRow) + 1;
                
            return numberOfRows * (COLOR_BOX_SIZE + 4) + LINE_PADDING;
        }
    }
}