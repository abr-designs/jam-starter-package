using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FixedColorPaletteTool
{
    public static class ColorPaletteImporter
    {
        private static readonly string[] SupportedTypesFilters = 
        {
            "Pallets (.png, .hex, .pal)", "png,hex,pal"
        };
        public static void ImportColorFile(ColorPaletteScriptableObject container, bool destructive)
        {
            var filePath = EditorUtility.OpenFilePanelWithFilters("Open Color Palette", "", SupportedTypesFilters);
            var selectedFile = new FileInfo(filePath);

            if (!selectedFile.Exists)
                throw new FileNotFoundException($"No file at: {filePath}");

            //Only should display dialog if it will impact the user
            if(destructive)
            {
                //Confirm with the user that they want to replace it
                if (!EditorUtility.DisplayDialog("Import Colors",
                        "Importing colors will replace the current palette, continue?", "Replace", "Cancel"))
                    return;
            }
            
            //---------------------------------------------------------//
            
            var foundColors = new List<Color32>();
            
            switch (selectedFile.Extension)
            {
                case ".png":
                    ParseColorsFromPNG(selectedFile, foundColors);
                    break;
                default:
                    throw new NotImplementedException($"{selectedFile.Extension} file types are not supported by {nameof(ColorPaletteImporter)}");
            }
            //---------------------------------------------------------//
            
            if (foundColors.Count == 0)
            {
                Debug.LogError($"No colors found in {selectedFile.Name}");
                return;
            }

            container.name = selectedFile.Name;
            container.paletteName = selectedFile.Name;
            ToColorData(container.colors, foundColors, destructive);

            EditorUtility.SetDirty(container);
            AssetDatabase.SaveAssetIfDirty(container);
        }

        //Parsers
        //============================================================================================================//
        private static void ParseColorsFromPNG(FileInfo file, List<Color32> list)
        {
            // Read bytes via FileInfo
            byte[] pngData;
            using (var fileStream = file.OpenRead())
            {
                pngData = new byte[fileStream.Length];
                fileStream.Read(pngData, 0, pngData.Length);
            }
            
            // Load into texture
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            texture.LoadImage(pngData, false);

            // Example: Read colors
            var allColors = texture.GetPixels32();

            var hashSet = new HashSet<Color32>();

            foreach (var color in allColors)
            {
                if (hashSet.Contains(color))
                    continue;
                
                list.Add(color);
                hashSet.Add(color);
            }
            
            //Cleanup
            Object.DestroyImmediate(texture);
        }
        
        //============================================================================================================//
        
        #region Convert To ColorData[]

        private static void ToColorData(List<ColorData> currentList, List<Color32> colors, bool destructive)
        {
            if(destructive)
                currentList.Clear();
            //var outData = new List<ColorData>(colors.Count);
            for (int i = 0; i < colors.Count; i++)
            {
                var colorData = new ColorData
                {
                    name = $"#{ColorUtility.ToHtmlStringRGBA(colors[i])[..6]}",
                    color = colors[i]
                };
                
                //We don't want to add colors that already exist
                if(currentList.Contains(colorData))
                    continue;
                
                currentList.Add(colorData);
            }
        }

        #endregion //Convert To ColorData[]
        
        //============================================================================================================//
        
    }
}