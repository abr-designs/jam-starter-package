using System;
using System.Collections.Generic;
using System.Linq;
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
#if UNITY_EDITOR
            if (CalledFromConstructor())
                return TryParseManually(index);
#endif
            
            return FixedPaletteSettings.Instance.selectedPalette.colors[index];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 GetColor(COLOR colorType) => GetColorData(colorType).color;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ColorData GetColorData(COLOR colorType)
        {

#if UNITY_EDITOR
            if (CalledFromConstructor())
                return TryParseManually(colorType);
#endif
            
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

#if UNITY_EDITOR

        private static List<ColorData> s_initializerSafeColors;

        private static ColorData TryParseManually(COLOR colorType)
        {
            const string AssetName = "FixedPaletteSettings";
            const string AssetPath = "Assets/Settings/" + AssetName + ".asset";

            if (s_initializerSafeColors == null)
            {
                if (!UnityPaletteParser.TryParsePaletteYaml(AssetPath, out s_initializerSafeColors))
                    return new ColorData();
            }
            
            var found = s_initializerSafeColors.FirstOrDefault(x => x.colorType == colorType);

            if (found.colorType != colorType)
                throw new MissingFieldException($"No color set to {colorType}");

            return found;
        }
        
        private static ColorData TryParseManually(int colorIndex)
        {
            const string AssetName = "FixedPaletteSettings";
            const string AssetPath = "Assets/Settings/" + AssetName + ".asset";

            if (s_initializerSafeColors == null)
            {
                if (!UnityPaletteParser.TryParsePaletteYaml(AssetPath, out s_initializerSafeColors))
                    return new ColorData();
            }

            return s_initializerSafeColors[colorIndex];
        }
        
        private static bool CalledFromConstructor()
        {
            var st = new System.Diagnostics.StackTrace();
            foreach (var frame in st.GetFrames())
            {
                var method = frame.GetMethod();
                if (method.IsConstructor)      // instance ctor
                    return true;
                if (method.Name is ".cctor" or ".ctor")   // type initializer (static ctor)
                    return true;
            }
            return false;
        }
#endif
    }
}