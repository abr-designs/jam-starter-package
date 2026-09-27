using System.Collections.Generic;
using Samples.MapTiles.Maps;
using Samples.MapTiles.ScriptableObjects;
using Samples.MapTiles.Square2DTiles.Tiles;
using UnityEngine;

namespace Samples.MapTiles.Square2DTiles.Maps
{
    public class Square2DTileMap : BaseMap<Square2DTile, Vector2Int>
    {
        public Square2DTileMap(TilesetScriptableObject tileset, IGenerateMap mapGenerator) : base(tileset, mapGenerator)
        {
            
        }

        //Map Helper Functions
        //================================================================================================================//

        #region Map Helper Functions

        public override IEnumerable<T> GetSurroundingTiles<T>(T baseTile, float radius)
        {
            foreach (var coordinate in GetCoordinatesInRadius((int)radius))
            {
                if(!MapTiles.TryGetValue(coordinate + baseTile.Position, out var tile))
                    continue;
                
                yield return tile as T;
            }
        }

        public override IEnumerable<T> GetEncompassingTiles<T>(T baseTile, float radius)
        {
            foreach (var coordinate in GetCoordinatesInSquare((int)radius))
            {
                if(!MapTiles.TryGetValue(coordinate + baseTile.Position, out var tile))
                    continue;
                
                yield return tile as T;
            }
        }


        public override List<T> FindAllSimilarConnected<T>(T start)
        {
            List<Square2DTile> result = new();
            HashSet<Vector2Int> visited = new();

            Search(start, start.TileID, visited, result);

            return result as List<T>;
        }

        protected override void Search<T>(T tile, int targetTypeID, HashSet<Vector2Int> visited, List<T> result)
        {
            if (tile == null)
                return;

            if (!visited.Add(tile.Position))
                return;

            if (tile.TileID != targetTypeID)
                return;

            result.Add(tile);

            foreach (var dir in CardinalDirections)
            {
                if (MapTiles.TryGetValue(tile.Position + dir, out var neighbor))
                {
                    Search(neighbor as T, targetTypeID, visited, result);
                }
            }
        }
        
        /// <summary>
        /// Returns all coordinates from center (0, 0) for the specified radius
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public override IEnumerable<Vector2Int> GetCoordinatesInRadius(int radius)
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
        
        public override IEnumerable<Vector2Int> GetCoordinatesInSquare(int radius)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    yield return new Vector2Int(x, y);
                }
            }
        }

        #endregion //Map Helper Functions

        //================================================================================================================//
    }
}