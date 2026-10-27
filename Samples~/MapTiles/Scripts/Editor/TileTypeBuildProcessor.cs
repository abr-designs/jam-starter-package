using System.Collections.Generic;
using Samples.MapTiles.Tiles;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Samples.MapTiles.Editor
{
    public class TileTypeBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        /// <summary>
        /// Fails the build when a tileset is invalid, a generated file is missing or differs from what the generator
        /// would write, or a serialized tile Id is out of date. Writes nothing.
        /// </summary>
        /// <remarks>Rewritten by Claude (claude-opus-5-5), 2026-09-30</remarks>
        public void OnPreprocessBuild(BuildReport report)
        {
            var problems = new List<string>();
            var tilesets = TileTypeGenerator.LoadAllTilesets();

            foreach (var source in TileTypeGenerator.BuildSources(tilesets, problems))
            {
                if (!TileTypeGenerator.IsUpToDate(source))
                    problems.Add($"'{source.FilePath}' is missing or out of date.");
            }

            foreach (var tileset in tilesets)
            {
                if (tileset.tiles == null)
                    continue;

                foreach (var tile in tileset.tiles)
                {
                    if (tile != null && tile.Id != TileTypeDefinition.ConvertNameToId(tile.Name))
                        problems.Add(
                            $"Tileset '{AssetDatabase.GetAssetPath(tileset)}': tile '{tile.Name}' has an out of date Id.");
                }
            }

            if (problems.Count == 0)
                return;

            throw new BuildFailedException(
                "Tile types are out of date:\n" +
                string.Join("\n", problems) +
                $"\nRun {TileTypeGenerator.RegenerateMenuPath.Replace("/", " > ")}.");
        }
    }
}