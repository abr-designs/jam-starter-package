using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FixedColorPaletteTool
{
    public interface IFixedColorPaletteImporter
    {
        string FileExtention { get; }

        void ParseColorsFromFile(FileInfo file, List<Color32> outColors);
    }
}