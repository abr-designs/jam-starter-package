using NaughtyAttributes;
using Samples.MapTiles.ScriptableObjects;
using Samples.MapTiles.Square2DTiles.Maps;
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

        private Square2DTileMap m_myTileMap;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            m_myTileMap = useRadial ? 
                Square2DTileMap.CreateRadialMap(mapTileset, mapRadius, transform, mapSeed) :
                Square2DTileMap.CreateGridMap(mapTileset, mapSize, transform, mapSeed);
        }

        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void Regenerate()
        {
            m_myTileMap.RegenerateMap();
        }
    }
}
