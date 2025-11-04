using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FixedColorPaletteTool.Importing.Importers
{
    public class PNGFixedColorPaletteImporter : IFixedColorPaletteImporter
    {
        public string FileExtention => ".png";
        
        public void ParseColorsFromFile(FileInfo file, List<Color32> outColors)
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
                
                outColors.Add(color);
                hashSet.Add(color);
            }
            
            //Cleanup
            Object.DestroyImmediate(texture);
        }
    }
}