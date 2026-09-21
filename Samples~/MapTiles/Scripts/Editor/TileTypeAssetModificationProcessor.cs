using UnityEditor;
using MapGeneration.ScriptableObjects;

namespace Samples.MapTiles.Scripts.Editor
{
    public class TileTypeAssetModificationProcessor : AssetModificationProcessor
    {
        private static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                var tileset = AssetDatabase.LoadAssetAtPath<TilesetScriptableObject>(path);

                if (tileset == null)
                    continue;

                TileTypeGenerator.Generate(tileset);
            }

            return paths;
        }
    }
}