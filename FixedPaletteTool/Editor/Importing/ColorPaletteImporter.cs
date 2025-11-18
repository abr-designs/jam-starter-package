using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FixedColorPaletteTool.Importing.Importers;
using UnityEditor;
using UnityEngine;

namespace FixedColorPaletteTool.Importing
{
    internal static class ColorPaletteImporter
    {
        private static readonly List<IFixedColorPaletteImporter> Importers = new()
        {
            new PNGFixedColorPaletteImporter(),
            new HEXFixedColorPaletteImporter(),
        };
        
        private static string[] SupportedTypesFilters = 
        {
            
            $"Pallets ({string.Join( ", ", Importers.Select(x=> x.FileExtension))})", 
            $"{string.Join( ',', Importers.Select(x=> x.FileExtension[1..]))}"
        };
        
        internal static void ImportColorFile(ColorPaletteScriptableObject container, bool destructive)
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

            var importer = Importers.FirstOrDefault(x => x.FileExtension == selectedFile.Extension);

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
            currentList ??= new List<ColorData>();
            
            if(destructive)
                currentList.Clear();

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