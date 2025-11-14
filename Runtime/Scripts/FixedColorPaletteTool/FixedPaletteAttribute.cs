using UnityEngine;

namespace FixedColorPaletteTool
{
    public enum COLOR_SELECT
    {
        DEFAULT,
        SHADES
    }
    
    public class FixedPaletteAttribute : PropertyAttribute
    {
        public COLOR_SELECT ColorSelect { get; }

        public FixedPaletteAttribute(COLOR_SELECT colorSelect = COLOR_SELECT.DEFAULT)
        {
            ColorSelect = colorSelect;
        }
        
    }
}