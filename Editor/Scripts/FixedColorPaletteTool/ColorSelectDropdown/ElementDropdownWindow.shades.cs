using UnityEngine;
using UnityEngine.UIElements;
using Scripts.Utilities.Extensions;

namespace FixedColorPaletteTool
{
    public partial class ElementDropdownWindow
    {
        const int SHADES_COUNT = 3;
        
        private void DrawAsListShades(VisualElement root)
        {
            
            const float MIN_VALUE = 0.25f;
            const float MIN_SATURATION = 0.2f;
            
            for (var i = 0; i < m_options.Count; i++)
            {
                var colorOption = m_options[i];
                var row = CreateRow();
                var backgroundColor = row.style.backgroundColor;

                var baseColor = (Color)colorOption.color;
                Color.RGBToHSV(baseColor, out var h, out var s, out var v);
                var fullDevalued = Color.HSVToRGB(h, s,  v * MIN_VALUE);
                var fullDesatured = Color.HSVToRGB(h, s * MIN_SATURATION, 1f);
                
                //Devalued colors
                for (int j = 0; j < SHADES_COUNT; j++)
                {
                    var valueColor = Color.Lerp(fullDevalued, baseColor, j / (float)SHADES_COUNT);
                    row.Add(CreateGridSlot(backgroundColor, valueColor));
                }
                
                //Default Color
                var baseGrid = CreateGridSlot(backgroundColor, baseColor);
                baseGrid.style.marginLeft = baseGrid.style.marginRight = 8;
                row.Add(baseGrid);
                
                //Desaturated colors
                for (int j = SHADES_COUNT - 1; j >= 0; j--)
                {
                    var saturateColor = Color.Lerp(fullDesatured, baseColor, j / (float)SHADES_COUNT);
                    row.Add(CreateGridSlot(backgroundColor, saturateColor));
                }
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
            VisualElement CreateGridSlot(StyleColor defaultBackgroundColor, Color color)
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
                
                gridSlot.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    gridSlot.style.backgroundColor = new StyleColor(Color.gray);
                });

                gridSlot.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    gridSlot.style.backgroundColor = defaultBackgroundColor;
                });

                var colorOption = new ColorData()
                {
                    color = color
                };

                var colorBox = DrawColorBox(colorOption);
                colorBox.style.SetPadding(6,4);
                
                gridSlot.Add(colorBox);
                
                // Highlight current
                if (colorOption.Equals(m_current))
                    gridSlot.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.2f));

                gridSlot.RegisterCallback<ClickEvent>(_ =>
                {
                    m_onSelect?.Invoke(-1, colorOption);
                    Close();
                });
                
                return gridSlot;
            }
        }
        
        public static float GetExpectedWidth(COLOR_SELECT colorSelect)
        {
            const int DEFAULT_WIDTH = 180;
            const int LINE_PADDING = 8;

            if (colorSelect == COLOR_SELECT.DEFAULT)
                return DEFAULT_WIDTH;


            return ((SHADES_COUNT * 2) + 1) * (COLOR_BOX_SIZE + 4) + (COLOR_BOX_SIZE + 16);

        }
    }
}