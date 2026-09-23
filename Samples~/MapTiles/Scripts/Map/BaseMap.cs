using System.Collections.Generic;
using System.Linq;
using MapGeneration;
using MapGeneration.ScriptableObjects;
using Tiles;
using UnityEngine;

namespace Samples.MapTiles.Scripts.Map
{
    public abstract class BaseMap
    {
        protected static readonly Vector2Int[] CardinalDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        protected readonly TilesetScriptableObject Tileset;
        private readonly Dictionary<int, TileTypeDefinition> m_tileTypeDefinitions;
        protected readonly IGenerateMap MapGenerator;

        protected BaseMap(TilesetScriptableObject tileset, IGenerateMap mapGenerator)
        {
            Tileset = tileset;
            MapGenerator = mapGenerator;
            
            m_tileTypeDefinitions =  new Dictionary<int, TileTypeDefinition>(tileset.tiles.Length);
            foreach (var tile in tileset.tiles)
            {
                m_tileTypeDefinitions.Add(tile.Id, tile);
            }
        }

        public TileTypeDefinition GetTileDefinition(TileType type) => m_tileTypeDefinitions[type.Id];
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TILE_TYPE">Tile Type</typeparam>
    /// <typeparam name="TILE_POS_UNIT">Tile Position Unit</typeparam>
    public abstract class BaseMap<TILE_TYPE, TILE_POS_UNIT> : BaseMap where TILE_TYPE : BaseTile<TILE_POS_UNIT>
    {
        public readonly Dictionary<TILE_POS_UNIT, TILE_TYPE> MapTiles;
        
        protected BaseMap(TilesetScriptableObject tileset, IGenerateMap mapGenerator) : base(tileset, mapGenerator)
        {
            MapTiles = new Dictionary<TILE_POS_UNIT, TILE_TYPE>();
        }
        
        public virtual void Generate() => MapGenerator.GenerateMap(Tileset, MapTiles);
        
        //Map Tile Searching
        //================================================================================================================//

        #region Map Tile Searching

        /// <summary>
        /// Returns all coordinates from center (0, 0) for the specified radius
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public abstract IEnumerable<TILE_POS_UNIT> GetCoordinatesInRadius(int radius);
        public abstract IEnumerable<TILE_POS_UNIT> GetCoordinatesInSquare(int radius);
        public abstract IEnumerable<TILE_TYPE> GetSurroundingTiles(TILE_TYPE mapTile, float radius, Dictionary<TILE_POS_UNIT, TILE_TYPE> tiles);
        public abstract IEnumerable<TILE_TYPE> GetEncompassingTiles(TILE_TYPE baseTile, float radius, Dictionary<TILE_POS_UNIT, TILE_TYPE> tiles);
        public abstract List<TILE_TYPE> FindAllSimilarConnected(TILE_TYPE start, Dictionary<TILE_POS_UNIT, TILE_TYPE> tiles);
        protected abstract void Search(TILE_TYPE tile, int targetTypeID, HashSet<TILE_POS_UNIT> visited, List<TILE_TYPE> result, Dictionary<TILE_POS_UNIT, TILE_TYPE> tiles);

        #endregion //Map Tile Searching

        //Misc Functions
        //================================================================================================================//
        
        public TILE_TYPE GetFirstTileWhere(TileType typeId)
        {
            return MapTiles.Values.FirstOrDefault(x => x.TileID == typeId);
        }
        
        public IEnumerable<TILE_TYPE> GetAllTiles(TileType typeId)
        {
            foreach (var tile in MapTiles)
            {
                if(tile.Value.TileID == typeId)
                    yield return tile.Value;
            }
        }
        //================================================================================================================//

    }
}