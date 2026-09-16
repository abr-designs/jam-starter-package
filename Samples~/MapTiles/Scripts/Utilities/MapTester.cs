#if UNITY_EDITOR
using System.Collections.Generic;
using MapGeneration.Generators;
using MapGeneration.ScriptableObjects;
using NaughtyAttributes;
using Samples.MapTiles.Scripts.Map;
using Tiles;
using UnityEditor;
using UnityEngine;

namespace MapGeneration
{
    public class MapTester : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int mapSize;

        [SerializeField, Min(2)]
        private int mapRadius;

        [SerializeField, Min(1)]
        private int tileSize;
        
        [SerializeField]
        private TilesetScriptableObject tileset;

        private Dictionary<Vector2Int, Tile2D> m_tiles;
        
        [SerializeField]
        private List<GameObject> tileObjects;

        [SerializeField]
        private int seed;

        [Button]
        private void RandomSeed()
        {
            seed = Random.Range(0, int.MaxValue);
            EditorUtility.SetDirty(this);
        }
        
        
        [Button]
        private void Generate()
        {
            TileHelper.RegisterTiles(tileset.tiles);
            
            Cleanup();
            tileObjects = new List<GameObject>();
            m_tiles = new Dictionary<Vector2Int, Tile2D>(mapSize.x * mapSize.y);

            var mapGenerator = new RadialGenerator(seed, mapRadius, tileSize, transform, BaseMap.GetCoordinatesInRadius);
            mapGenerator.GenerateMap(tileset, m_tiles,transform);

            foreach (var simpleTile in m_tiles)
            {
                //simpleTile.Value.SetState(TILE_STATE.CLAIMED);
                tileObjects.Add(simpleTile.Value.gameObject);
            }
        }
        
        private void Cleanup()
        {
            foreach (var tileObject in tileObjects)
            {
                DestroyImmediate(tileObject);
            }
        }
    }
}
#endif