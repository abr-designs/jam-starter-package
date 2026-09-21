using System.Collections.Generic;
using System.Linq;
using MapGeneration;
using MapGeneration.ScriptableObjects;
using Tiles;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    /// <typeparam name="T">Tile Type</typeparam>
    /// <typeparam name="TU">Tile Position Unit</typeparam>
    /// <typeparam name="TS">Tile Size Unit</typeparam>
    public abstract class BaseMap<T, TU, TS> : BaseMap where T : BaseTile<TU>
    {
        public readonly Dictionary<TU, T> MapTiles;
        
        protected BaseMap(TilesetScriptableObject tileset, IGenerateMap mapGenerator) : base(tileset, mapGenerator)
        {
            MapTiles = new Dictionary<TU, T>();
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
        public abstract IEnumerable<TU> GetCoordinatesInRadius(int radius);

        public abstract IEnumerable<TU> GetCoordinatesInSquare(int radius);

        public abstract IEnumerable<T> GetSurroundingTiles(T mapTile, TS radius, Dictionary<TU, T> tiles);
        
        public abstract IEnumerable<T> GetEncompassingTiles(T baseTile, TS radius, Dictionary<TU, T> tiles);
        /*public abstract IEnumerable<TU> GetCardinalDirections(T baseTile);*/
        /*public abstract IEnumerable<TU> GetCoordinatesInSquare(TU center, TS radius);
        public abstract IEnumerable<TU> GetCoordinatesInRadius(TS radius);*/
        public abstract List<T> FindAllSimilarConnected(T start, Dictionary<TU, T> tiles);
        protected abstract void Search(T tile, TileType targetTypeID, HashSet<TU> visited, List<T> result, Dictionary<TU, T> tiles);

        #endregion //Map Tile Searching

        //Misc Functions
        //================================================================================================================//
        
        public T GetFirstTileWhere(TileType typeId)
        {
            return MapTiles.Values.FirstOrDefault(x => x.TileType == typeId);
        }
        
        public IEnumerable<T> GetAllTiles(TileType typeId)
        {
            foreach (var tile in MapTiles)
            {
                if(tile.Value.TileType == typeId)
                    yield return tile.Value;
            }
        }
        //================================================================================================================//

    }
}