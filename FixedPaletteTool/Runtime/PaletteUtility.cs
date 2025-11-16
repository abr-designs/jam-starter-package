using System.Runtime.CompilerServices;
using UnityEngine;

namespace FixedColorPaletteTool
{
    public static class PaletteUtility
    {
        public static Color Primary => Primary32;
        public static Color Secondary => Secondary32;
        public static Color Tertiary => Tertiary32;
        
        public static Color32 Primary32 => GetColorAtIndex(0);
        public static Color32 Secondary32 => GetColorAtIndex(1);
        public static Color32 Tertiary32 => GetColorAtIndex(2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 GetColorAtIndex(int index) => GetColorData(index).color;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ColorData GetColorData(int index)
        {
            return FixedPaletteSettings.Instance.selectedPalette.colors[index];
        }
    }
}