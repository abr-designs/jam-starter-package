using MapGeneration.Generators;
using MapGeneration.ScriptableObjects;
using Samples.MapTiles.Scripts.Map;
using UnityEngine;

namespace Samples.MapTiles
{
    public class MapSample : MonoBehaviour
    {
        [Header("Map Generation")]
        public TilesetScriptableObject mapTileset;
        public int mapSeed;
        public bool useRadial;
        
        [Header("Grid Map")]
        public Vector2Int mapSize;

        [Header("Radial Map"), Min(1)] 
        public int mapRadius;

        private Map2DTiles m_myMap;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            
            m_myMap = new Map2DTiles(
                mapTileset,
                useRadial ? 
                    new RadialMapGenerator(mapSeed, mapRadius, (int)mapTileset.tileSize, transform) :
                    new GridMapGenerator(mapSeed, mapSize, (int)mapTileset.tileSize));
            
            m_myMap.Generate(transform);
        }
    
    }
}
