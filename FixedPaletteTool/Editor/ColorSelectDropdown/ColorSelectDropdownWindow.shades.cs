using UnityEngine;
using UnityEngine.UIElements;
using Scripts.Utilities.Extensions;

namespace FixedColorPaletteTool
{
    internal partial class ColorSelectDropdownWindow
    {
        private const int SHADES_COUNT = 3;
        
        /// <summary>
        /// Draws a grid of colors, where the horizontal space is taken up by the palette entries & the vertical space
        /// is taken by the transition of colors from DeSaturated to DeValued.
        /// </summary>
        /// <param name="root"></param>
        private void DrawAsListShades(VisualElement root)
        {
            //We don't want the value to reach 0.0f, since that would be 100% black, we just want a dark variation of the current color
            const float MIN_VALUE = 0.25f;
            //We don't want the value to reach 0.0f, since that would be 100% white, we just want a lighter variation of the current color
            const float MIN_SATURATION = 0.2f;

            root.style.flexDirection = FlexDirection.Row;
            
            for (var i = 0; i < m_options.Count; i++)
            {
                var colorOption = m_options[i];
                var row = CreateRow();
                var backgroundColor = row.style.backgroundColor;

                //Determine the color targets
                var baseColor = (Color)colorOption.color;
                Color.RGBToHSV(baseColor, out var h, out var s, out var v);
                var fullDevalued = Color.HSVToRGB(h, s,  v * MIN_VALUE);
                var fullDesatured = Color.HSVToRGB(h, s * MIN_SATURATION, 1f);
                
                //Default Color
                var baseGrid = CreateGridSlot(backgroundColor, baseColor);
                row.Add(baseGrid);
                
                //Desaturated colors
                for (int j = 0; j < SHADES_COUNT; j++)
                {
                    var saturateColor = Color.Lerp(fullDesatured, baseColor, j / (float)SHADES_COUNT);
                    row.Add(CreateGridSlot(backgroundColor, saturateColor));
                }
                
                //Devalued colors
                for (int j = SHADES_COUNT - 1; j >= 0; j--)
                {
                    var valueColor = Color.Lerp(fullDevalued, baseColor, j / (float)SHADES_COUNT);
                    row.Add(CreateGridSlot(backgroundColor, valueColor));
                }
            }

            return;
            
            VisualElement CreateRow()
            {
                var newRow = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Column,
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

                var colorBox = DrawColorIcon(colorOption);

                colorBox.style.SetPadding(6,4);
                
                gridSlot.Add(colorBox);
                
                // Highlight current
                if (colorOption.Equals(m_current))
                    gridSlot.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.2f));

                gridSlot.RegisterCallback<ClickEvent>(_ =>
                {
                    m_onSelect?.Invoke(colorOption);
                    Close();
                });
                
                return gridSlot;
            }
        }
        
        private static float GetExpectedShadesWidth()
        {
            var itemCount = FixedPaletteSettings.Instance.selectedPalette.colors.Count;

            return (itemCount) * (COLOR_BOX_SIZE + 4) + LINE_PADDING * 2f;
        }
        private static float GetExpectedShadesHeight()
        {
            return ((SHADES_COUNT * 2) + 1) * (COLOR_BOX_SIZE + 4) + LINE_PADDING;
        }
    }
}