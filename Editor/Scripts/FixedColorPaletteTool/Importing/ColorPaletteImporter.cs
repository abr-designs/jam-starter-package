using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FixedColorPaletteTool.Importing.Importers;
using UnityEditor;
using UnityEngine;

namespace FixedColorPaletteTool.Importing
{
    public static class ColorPaletteImporter
    {
        private static List<IFixedColorPaletteImporter> s_importers = new()
        {
            new PNGFixedColorPaletteImporter(),
            new HEXFixedColorPaletteImporter(),
        };
        
        private static string[] SupportedTypesFilters = 
        {
            
            $"Pallets ({string.Join( ", ", s_importers.Select(x=> x.FileExtention))})", 
            $"{string.Join( ',', s_importers.Select(x=> x.FileExtention[1..]))}"
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

            var importer = s_importers.FirstOrDefault(x => x.FileExtention == selectedFile.Extension);

            if (importer == null)
                throw new NotImplementedException($"{selectedFile.Extension} file types are not supported by {nameof(ColorPaletteImporter)}");
            
            importer.ParseColorsFromFile(selectedFile, foundColors);
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