using System.Collections.Generic;
using MapGeneration.ScriptableObjects;
using Tiles;
using UnityEngine;

namespace Samples.MapTiles.Scripts.Map
{
    public abstract class BaseMap
    {
        protected static readonly Vector2Int[] CardinalDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        
        protected readonly TilesetScriptableObject Tileset;
        protected readonly IGenerate Generator;

        protected BaseMap(TilesetScriptableObject tileset, IGenerate generator)
        {
            Tileset = tileset;
            Generator = generator;
        }
        
        /// <summary>
        /// Returns all coordinates from center (0, 0) for the specified radius
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static IEnumerable<Vector2Int> GetCoordinatesInRadius(int radius)
        {
            var rSquared = radius * radius;

            for (var x = -radius; x <= radius; x++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    if (x * x + y * y <= rSquared)
                        yield return new Vector2Int(x, y);
                }
            }
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Tile Type</typeparam>
    /// <typeparam name="TU">Tile Position Unit</typeparam>
    /// <typeparam name="TS">Tile Size Unit</typeparam>
    public abstract class BaseMap<T, TU, TS> : BaseMap where T : BaseTile<TU, TS>
    {
        protected Dictionary<TU, T> Tiles;
        
        protected BaseMap(TilesetScriptableObject tileset, IGenerate generator) : base(tileset, generator)
        {
            Tiles = new Dictionary<TU, T>();
        }
        

        //Map Tile Searching
        //================================================================================================================//

        #region Map Tile Searching

        public abstract IEnumerable<T> GetSurroundingTiles(T mapTile, TS radius, Dictionary<TU, T> tiles);
        public abstract IEnumerable<T> GetEncompassingTiles(T baseTile, TS radius, Dictionary<TU, T> tiles);
        /*public abstract IEnumerable<TU> GetCardinalDirections(T baseTile);*/
        public abstract IEnumerable<TU> GetCoordinatesInRadius(TU center, TS radius);
        /*public abstract IEnumerable<TU> GetCoordinatesInSquare(TU center, TS radius);
        public abstract IEnumerable<TU> GetCoordinatesInRadius(TS radius);*/
        public abstract List<T> FindAllSimilarConnected(T start, Dictionary<TU, T> tiles);
        protected abstract void Search(T tile, int targetTypeID, HashSet<TU> visited, List<T> result, Dictionary<TU, T> tiles);

        #endregion //Map Tile Searching
        


    }
}