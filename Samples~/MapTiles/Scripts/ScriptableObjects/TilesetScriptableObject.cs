using NaughtyAttributes;
using UnityEngine;

namespace MapGeneration.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Tileset", menuName = "ScriptableObjects/Tileset", order = 1)]
    public class TilesetScriptableObject : ScriptableObject
    {
        public float tileSize;
#if UNITY_EDITOR
        [Button]
        private void AssignId()
        {
            foreach (var tileData in tiles)
            {
                tileData.id = tileData.name.GetHashCode();
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        
        public TileData[] tiles;
    }
}