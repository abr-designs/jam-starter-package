using System.Collections.Generic;
using MapGeneration.ScriptableObjects;
using Tiles;
using UnityEngine;

namespace Samples.MapTiles.Scripts.Map
{
    public class Square2DTileMap : BaseMap<Square2DTile, Vector2Int>
    {
        public Square2DTileMap(TilesetScriptableObject tileset, IGenerateMap mapGenerator) : base(tileset, mapGenerator)
        {
            
        }

        //Map Helper Functions
        //================================================================================================================//

        #region Map Helper Functions

        public override IEnumerable<Square2DTile> GetSurroundingTiles(Square2DTile mapTile, float radius, Dictionary<Vector2Int, Square2DTile> tiles)
        {
            foreach (var coordinate in GetCoordinatesInRadius((int)radius))
            {
                if(!tiles.TryGetValue(coordinate + mapTile.Position, out var tile))
                    continue;
                
                yield return tile;
            }
        }

        public override IEnumerable<Square2DTile> GetEncompassingTiles(Square2DTile baseTile, float radius, Dictionary<Vector2Int, Square2DTile> tiles)
        {
            foreach (var coordinate in GetCoordinatesInSquare((int)radius))
            {
                if(!tiles.TryGetValue(coordinate + baseTile.Position, out var tile))
                    continue;
                
                yield return tile;
            }
        }


        public override List<Square2DTile> FindAllSimilarConnected(Square2DTile start, Dictionary<Vector2Int, Square2DTile> tiles)
        {
            List<Square2DTile> result = new();
            HashSet<Vector2Int> visited = new();

            Search(start, start.TileID, visited, result, tiles);

            return result;
        }

        protected override void Search(Square2DTile tile, int targetTypeID, HashSet<Vector2Int> visited, List<Square2DTile> result, Dictionary<Vector2Int, Square2DTile> tiles)
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