using System;
using System.Runtime.CompilerServices;
using FixedColorPaletteTool.Enums;
using UnityEngine;

namespace FixedColorPaletteTool
{
    public static class PaletteUtility
    {
        public static Color Primary => Primary32;
        public static Color Secondary => Secondary32;
        public static Color Tertiary => Tertiary32;
        
        public static Color32 Primary32 => GetColor(COLOR.PRIMARY);
        public static Color32 Secondary32 => GetColor(COLOR.SECONDARY);
        public static Color32 Tertiary32 => GetColor(COLOR.TERTIARY);

        public static int ColorCount => FixedPaletteSettings.Instance.selectedPalette.colors.Count;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 GetColorAtIndex(int index) => GetColorData(index).color;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ColorData GetColorData(int index)
        {
            return FixedPaletteSettings.Instance.selectedPalette.colors[index];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 GetColor(COLOR colorType) => GetColorData(colorType).color;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ColorData GetColorData(COLOR colorType)
        {
            switch (colorType)
            {
                case COLOR.NONE: return default;
                case COLOR.PRIMARY:
                    return FixedPaletteSettings.Instance.selectedPalette.Primary;
                case COLOR.SECONDARY:
                    return FixedPaletteSettings.Instance.selectedPalette.Secondary;
                case COLOR.TERTIARY:
                    return FixedPaletteSettings.Instance.selectedPalette.Tertiary;
                default:
                    throw new ArgumentOutOfRangeException(nameof(colorType), colorType, null);
            }
        }
    }
}