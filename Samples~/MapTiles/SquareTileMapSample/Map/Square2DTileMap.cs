using System.Collections.Generic;
using MapGeneration.ScriptableObjects;
using Tiles;
using UnityEngine;

namespace Samples.MapTiles.Scripts.Map
{
    public class Square2DTileMap : BaseMap<SquareTile2D, Vector2Int, int>
    {
        public Square2DTileMap(TilesetScriptableObject tileset, IGenerateMap mapGenerator) : base(tileset, mapGenerator)
        {
            
        }

        //Map Helper Functions
        //================================================================================================================//

        #region Map Helper Functions

        public override IEnumerable<SquareTile2D> GetSurroundingTiles(SquareTile2D mapTile, int radius, Dictionary<Vector2Int, SquareTile2D> tiles)
        {
            foreach (var coordinate in GetCoordinatesInRadius(radius))
            {
                if(!tiles.TryGetValue(coordinate + mapTile.Position, out var tile))
                    continue;
                
                yield return tile;
            }
        }

        public override IEnumerable<SquareTile2D> GetEncompassingTiles(SquareTile2D baseTile, int radius, Dictionary<Vector2Int, SquareTile2D> tiles)
        {
            foreach (var coordinate in GetCoordinatesInSquare(radius))
            {
                if(!tiles.TryGetValue(coordinate + baseTile.Position, out var tile))
                    continue;
                
                yield return tile;
            }
        }


        public override List<SquareTile2D> FindAllSimilarConnected(SquareTile2D start, Dictionary<Vector2Int, SquareTile2D> tiles)
        {
            List<SquareTile2D> result = new();
            HashSet<Vector2Int> visited = new();

            Search(start, start.TileType, visited, result, tiles);

            return result;
        }

        protected override void Search(SquareTile2D tile, TileType targetTypeID, HashSet<Vector2Int> visited, List<SquareTile2D> result, Dictionary<Vector2Int, SquareTile2D> tiles)
        {
            if (tile == null)
                return;

            if (!visited.Add(tile.Position))
                return;

            if (tile.TileType != targetTypeID)
                return;

            result.Add(tile);

            foreach (var dir in CardinalDirections)
            {
                if (tiles.TryGetValue(tile.Position + dir, out var neighbor))
                {
                    Search(neighbor, targetTypeID, visited, result, tiles);
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