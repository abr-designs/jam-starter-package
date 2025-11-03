using System;
using UnityEngine;

namespace FixedColorPaletteTool
{
    public class FixedPaletteSettings : ScriptableObject
    {
        public ColorPaletteScriptableObject selectedPalette;

#if !UNITY_EDITOR
        public static FixedPaletteSettings Instance { get; private set; }

        private void OnEnable()
        {
            Instance = this;
        }
        
#else
        public static FixedPaletteSettings Instance => GetOrCreate();


        public static UnityEditor.SerializedObject GetSerializedObject() => new(GetOrCreate());
        
        
        internal static FixedPaletteSettings GetOrCreate()
        {
            const string AssetPath = "Assets/Settings/FixedPaletteSettings.asset";
            
            var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<FixedPaletteSettings>(AssetPath);
            if (settings != null) 
                return settings;
            
            settings = CreateInstance<FixedPaletteSettings>();
            var palette = CreateInstance<ColorPaletteScriptableObject>();
            palette.name = "Default Color Palette";
            
            settings.selectedPalette = palette;
            UnityEditor.AssetDatabase.CreateAsset(settings, AssetPath);
            UnityEditor.AssetDatabase.AddObjectToAsset(palette, AssetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            return settings;
        }

        /*internal static ColorPaletteScriptableObject GetOrCreateDefaultColorPalette()
        {
            const string AssetPath = "Assets/ColorPalettes/DefaultColorPalette.asset";
            
            var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<ColorPaletteScriptableObject>(AssetPath);
            if (settings != null) 
                return settings;
            
            settings = CreateInstance<ColorPaletteScriptableObject>();
            UnityEditor.AssetDatabase.CreateAsset(settings, AssetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            return settings;
        }*/
#endif
    }
}