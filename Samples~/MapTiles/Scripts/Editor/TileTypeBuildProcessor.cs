using System;
using MapGeneration.ScriptableObjects;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Samples.MapTiles.Scripts.Editor
{
    public class TileTypeBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(TilesetScriptableObject)}");
            
            if(guids.Length > 1)
                throw new BuildFailedException("Multiple Tileset GUIDs found.\nThere is only support for a single tileset");

            try
            {
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var tileset = AssetDatabase.LoadAssetAtPath<TilesetScriptableObject>(path);

                    if (tileset == null)
                        continue;

                    TileTypeGenerator.Generate(tileset);
                }
            }
            catch (Exception e)
            {
                throw new BuildFailedException(e);
            }
        }
    }

}