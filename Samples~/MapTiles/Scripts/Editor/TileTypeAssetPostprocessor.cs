using System;
using System.Linq;
using Samples.MapTiles.ScriptableObjects;
using UnityEditor;

namespace Samples.MapTiles.Editor
{
    /// <summary>
    /// Regenerates tile types after a tileset is imported, moved or deleted, after a generated file is deleted, and
    /// after each script reload. Generation is deferred with <see cref="EditorApplication.delayCall"/> and runs once
    /// per batch of changes.
    /// </summary>
    /// <remarks>Rewritten by Claude (claude-opus-5-5), 2026-09-30</remarks>
    public class TileTypeAssetPostprocessor : AssetPostprocessor
    {
        private static bool s_isScheduled;

        [InitializeOnLoadMethod]
        private static void OnScriptsReloaded() => Schedule();

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (importedAssets.Any(IsTileset) ||
                movedAssets.Any(IsTileset) ||
                deletedAssets.Any(IsTilesetOrGeneratedPath) ||
                movedFromAssetPaths.Any(IsTilesetOrGeneratedPath))
            {
                Schedule();
            }
        }

        private static void Schedule()
        {
            if (s_isScheduled)
                return;

            s_isScheduled = true;
            EditorApplication.delayCall += Run;
        }

        private static void Run()
        {
            s_isScheduled = false;

            // The build processor checks the generated files instead of writing them.
            if (BuildPipeline.isBuildingPlayer)
                return;

            TileTypeGenerator.GenerateAll();
        }

        private static bool IsTileset(string path)
        {
            return path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) &&
                   typeof(TilesetScriptableObject).IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(path));
        }

        // Deleted assets can no longer be loaded, so any .asset path counts as a possible tileset.
        private static bool IsTilesetOrGeneratedPath(string path)
        {
            return path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase) ||
                   (path.Contains("/Generated/") && path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));
        }
    }
}
