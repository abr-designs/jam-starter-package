using System;

using System.IO;
using System.Text;
using Samples.MapTiles.ScriptableObjects;
using UnityEditor;

namespace Samples.MapTiles.Editor
{
    public static class TileTypeGenerator
    {
        private const string k_GeneratedFileName = "TILE_TYPE.cs";

        public static void Generate(TilesetScriptableObject tileset)
        {
            if (tileset == null)
                throw new ArgumentNullException(nameof(tileset));
            
            UpdateIds(tileset);
            GenerateCode(tileset);
        }

        private static void UpdateIds(TilesetScriptableObject tileset)
        {
            if (tileset.tiles == null)
                throw new ArgumentNullException(nameof(tileset));

            foreach (var tile in tileset.tiles)
            {
                if (tile == null)
                    continue;

                tile.Id = tile.Name
                    .ToEnumName()
                    .GetHashCode();
            }
        }

        private static void GenerateCode(TilesetScriptableObject tileset)
        {
            var assetPath = AssetDatabase.GetAssetPath(tileset);
            
            var targetDirectory = Path.Join(Path.GetDirectoryName(assetPath), "Generated");
            
            Directory.CreateDirectory(targetDirectory);

            var output = new StringBuilder();

            output.AppendLine("// ==================================================");
            output.AppendLine("// AUTO-GENERATED FILE.");
            output.AppendLine("// DO NOT EDIT.");
            output.AppendLine("// ==================================================");
            output.AppendLine();

            output.AppendLine("public static class TILE_TYPE");
            output.AppendLine("{");

            foreach (var tile in tileset.tiles)
            {
                if (tile == null)
                    continue;

                var name = tile.Name.ToEnumName();

                output.AppendLine(
                    $"    public static readonly TileType {name} = " +
                    $"new ({tile.Id});");
            }

            output.AppendLine("}");

            var contents = output.ToString();
            var filePath = Path.Join(targetDirectory, k_GeneratedFileName);
            
            // Only write if the generated source actually changed.
            if (File.Exists(filePath) && File.ReadAllText(filePath) == contents)
            {
                return;
            }

            File.WriteAllText(
                filePath,
                contents,
                Encoding.UTF8);
            
            AssetDatabase.ImportAsset(filePath);
            AssetDatabase.Refresh();
        }
        
        private static string ToEnumName(this string value)
        {
            return value
                .ToUpperInvariant()
                .Replace(' ', '_');
        }
    }

}
