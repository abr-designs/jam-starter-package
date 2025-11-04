using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FixedColorPaletteTool.Importing.Importers
{
    public class HEXFixedColorPaletteImporter: IFixedColorPaletteImporter
    {
        public string FileExtention => ".hex";
        public void ParseColorsFromFile(FileInfo file, List<Color32> outColors)
        {
            using var fs = file.OpenRead();
            using var reader = new StreamReader(fs);

            while (reader.ReadLine() is { } line)
            {
                if(string.IsNullOrWhiteSpace(line))
                    continue;

                ColorUtility.TryParseHtmlString(line, out var color);
                var color32 = (Color32)color;
                
                if(outColors.Contains(color32))
                    continue;

                outColors.Add(color32);
            }
        }
    }
}