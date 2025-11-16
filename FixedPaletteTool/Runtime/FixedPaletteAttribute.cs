using UnityEngine;

namespace FixedColorPaletteTool
{
    public class FixedPaletteAttribute : PropertyAttribute
    {
        public COLOR_SELECT ColorSelect { get; }

        public FixedPaletteAttribute(COLOR_SELECT colorSelect = COLOR_SELECT.DEFAULT)
        {
            ColorSelect = colorSelect;
        }
        
    }
}