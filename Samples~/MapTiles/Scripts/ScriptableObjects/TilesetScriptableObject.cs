using Samples.MapTiles.Tiles;
using UnityEditor;
using UnityEngine;

namespace Samples.MapTiles.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Tileset", menuName = "ScriptableObjects/Tileset", order = 1)]
    public class TilesetScriptableObject : ScriptableObject
    {
        public float tileSize;
        
        public TileTypeDefinition[] tiles;

        //Unity Editor Functions
        //================================================================================================================//

        #region Unity Editor Functions

#if UNITY_EDITOR
        //This remains because having the visual feedback that the ID is changing in the inspector is valuable
        private void OnValidate()
        {
            foreach (var tileData in tiles)
            {
                tileData.Id = ToEnumName(tileData.Name).GetHashCode();
            }

            EditorUtility.SetDirty(this);
            return;

            static string ToEnumName(string value)
            {
                return value
                    .ToUpperInvariant()
                    .Replace(' ', '_');
            }
        }
#endif

        #endregion //Unity Editor Functions
        
        //================================================================================================================//

    }
}