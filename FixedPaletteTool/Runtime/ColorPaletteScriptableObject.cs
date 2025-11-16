using System.Collections.Generic;
using UnityEngine;

namespace FixedColorPaletteTool
{
    //TODO Might be better to have the color thing as an attribute to override the inspector setup
    public class ColorPaletteScriptableObject : ScriptableObject
    {
        public string paletteName;
        
        //TODO At runtime, I want this to be converted into a static dictionary.
        //That actually might not be required, if this is just a helper for selecting a color
        //That only depends if we want to access colors dynamically!
        public List<ColorData> colors;
    }
}