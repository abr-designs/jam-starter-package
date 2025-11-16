using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace FixedColorPaletteTool
{
    public class FixedPaletteSettings : ScriptableObject
    {
        private const string AssetName = "FixedPaletteSettings";
        public const string AssetPath = "Assets/Settings/" + AssetName + ".asset";
        
        public ColorPaletteScriptableObject selectedPalette;

#if !UNITY_EDITOR
        public static FixedPaletteSettings Instance => Get();
        private static FixedPaletteSettings s_instance;

        private FixedPaletteSettings Get()
        {
            if (s_instance == null)
                s_instance = Resources.Load<FixedPaletteSettings>(AssetName);
            
            return s_instance;
        }
        
#else
        public static FixedPaletteSettings Instance => GetOrCreate();


        public static SerializedObject GetSerializedObject() => new(GetOrCreate());
        
        
        internal static FixedPaletteSettings GetOrCreate()
        {
            
            
            var settings = AssetDatabase.LoadAssetAtPath<FixedPaletteSettings>(AssetPath);
            if (settings != null) 
                return settings;
            
            settings = CreateInstance<FixedPaletteSettings>();
            var palette = CreateInstance<ColorPaletteScriptableObject>();
            palette.name = "Default Color Palette";
            
            settings.selectedPalette = palette;
            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.AddObjectToAsset(palette, AssetPath);
            AssetDatabase.SaveAssets();
            return settings;
        }

        public static ColorPaletteScriptableObject AddNewPalette()
        {
            const string AssetPath = "Assets/Settings/FixedPaletteSettings.asset";

            var settings = GetOrCreate();
            var palette = CreateInstance<ColorPaletteScriptableObject>();
            palette.name = "New Color Palette";
            palette.colors = new List<ColorData>();
            
            settings.selectedPalette = palette;
            AssetDatabase.AddObjectToAsset(palette, AssetPath);
            AssetDatabase.SaveAssets();
            return palette;
        }
        public static void DeletePalette(ColorPaletteScriptableObject toDestroy)
        {
            var settings = GetOrCreate();

            AssetDatabase.RemoveObjectFromAsset(toDestroy);
            
            DestroyImmediate(toDestroy);
            
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
        }

#endif
    }
}