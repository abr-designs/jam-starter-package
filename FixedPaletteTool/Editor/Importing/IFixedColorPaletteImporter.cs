using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FixedColorPaletteTool
{
    internal interface IFixedColorPaletteImporter
    {
        string FileExtension { get; }

        void ParseColorsFromFile(FileInfo file, List<Color32> outColors);
    }
}