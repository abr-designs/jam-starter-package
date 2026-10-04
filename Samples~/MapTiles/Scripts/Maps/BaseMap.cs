using System.Collections.Generic;
using System.Linq;
using Samples.MapTiles.ScriptableObjects;
using Samples.MapTiles.Tiles;
using UnityEngine;

namespace Samples.MapTiles.Maps
{
    public abstract class BaseMap
    {
        protected static readonly Vector2Int[] CardinalDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        protected readonly TilesetScriptableObject Tileset;
        private readonly Dictionary<int, TileTypeDefinition> m_tileTypeDefinitions;
        public IGenerateMap MapGenerator { get; }

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
        
        public virtual int Generate() => MapGenerator.GenerateMap(Tileset, MapTiles);
        
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
        public abstract IEnumerable<T> GetSurroundingTiles<T>(T mapTile, float radius) where T : TILE_TYPE;
        public abstract IEnumerable<T> GetEncompassingTiles<T>(T baseTile, float radius) where T : TILE_TYPE;
        public abstract List<TILE_TYPE> FindAllSimilarConnected(TILE_TYPE start);
        protected abstract void Search<T>(T tile, int targetTypeID, HashSet<TILE_POS_UNIT> visited, List<T> result) where T : TILE_TYPE;

        #endregion //Map Tile Searching

        //Misc Functions
        //================================================================================================================//
        
        public T GetFirstTileWhere<T>(TileType typeId) where T : TILE_TYPE
        {
            return MapTiles.Values.FirstOrDefault(x => x.TileID == typeId) as T;
        }
        
        public IEnumerable<T> GetAllTiles<T>(int typeId) where T : TILE_TYPE
        {
            foreach (var tile in MapTiles)
            {
                if(tile.Value.TileID == typeId)
                    yield return tile.Value as T;
            }
        }
        public IEnumerable<T> GetAllTiles<T>(TileType typeId) where T : TILE_TYPE
        {
            foreach (var tile in MapTiles)
            {
                if(tile.Value.TileID == typeId)
                    yield return tile.Value as T;
            }
        }
        //================================================================================================================//

    }
}