using System.Collections.Generic;
using Samples.MapTiles.Tiles;
using UnityEngine;

namespace Samples.MapTiles.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Tileset", menuName = "ScriptableObjects/Tileset", order = 1)]
    public class TilesetScriptableObject : ScriptableObject
    {
        public float tileSize;

        // [Claude 2026-09-30 start]
        [Header("Code Generation")] [Tooltip("Generated class name. Empty uses the asset name followed by \"Types\".")]
        public string generatedClassName;

        [Tooltip("Namespace of the generated class. Empty uses the global namespace.")]
        public string generatedNamespace;
        // [Claude 2026-09-30 end]

        public TileTypeDefinition[] tiles;

        //Unity Editor Functions
        //================================================================================================================//

        #region Unity Editor Functions

#if UNITY_EDITOR
        /// <summary>
        /// Keeps each tile Id in sync with its name (visible feedback in the inspector) and warns about tile data that
        /// fails at generation or at runtime: empty or duplicate names and missing variants. Marks the asset dirty only
        /// when an Id changed.
        /// </summary>
        /// <remarks>Rewritten by Claude (claude-opus-5-5), 2026-09-30</remarks>
        private void OnValidate()
        {
            if (tiles == null)
                return;

            var codeNames = new Dictionary<string, string>();
            var idChanged = false;

            for (var i = 0; i < tiles.Length; i++)
            {
                var tile = tiles[i];

                if (tile == null)
                    continue;

                var id = TileTypeDefinition.ConvertNameToId(tile.Name);
                if (tile.Id != id)
                {
                    tile.Id = id;
                    idChanged = true;
                }

                var codeName = TileTypeDefinition.ConvertNameToCodeName(tile.Name);
                if (codeName.Length == 0)
                    Debug.LogWarning($"[{name}] Tile {i} has no name.", this);
                else if (!codeNames.TryAdd(codeName, tile.Name))
                    Debug.LogWarning(
                        $"[{name}] Tiles '{codeNames[codeName]}' and '{tile.Name}' both become {codeName}.",
                        this);

                if (tile.tileVariants == null || tile.tileVariants.Length == 0)
                {
                    Debug.LogWarning($"[{name}] Tile '{tile.Name}' has no tile variants.", this);
                    continue;
                }

                for (var v = 0; v < tile.tileVariants.Length; v++)
                {
                    var variant = tile.tileVariants[v];

                    if (variant == null)
                        Debug.LogWarning($"[{name}] Tile '{tile.Name}' variant {v} is empty.", this);
                }
            }

            if (idChanged)
                UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        #endregion //Unity Editor Functions

        //================================================================================================================//
    }
}