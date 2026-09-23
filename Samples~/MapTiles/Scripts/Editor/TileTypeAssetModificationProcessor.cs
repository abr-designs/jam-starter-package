using Samples.MapTiles.ScriptableObjects;
using UnityEditor;


namespace Samples.MapTiles.Editor
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