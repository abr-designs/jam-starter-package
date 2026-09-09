/*using System.Collections.Generic;
using System.Linq;
using Tiles;
using UnityEngine;

namespace Utilities
{
    public static class SimpleTileBaseExtensions
    {
        public static readonly Vector2Int[] CardinalDirections = {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};

        /*public static bool IsAdjacentToClaimed(this SimpleTileBase simpleTileBase, Dictionary<Vector2Int, SimpleTileBase> tiles)
        {
            var tileCoordinate = SimpleTileBase.Position;
            foreach (var relativePosition in CardinalDirections)
            {
                if (!tiles.TryGetValue(tileCoordinate + relativePosition, out var tile))
                    continue;
                
                if(tile.State == TILE_STATE.CLAIMED)
                    return true;
            }

            return false;
        }#1#

        public static IEnumerable<BaseTile> GetSurroundingTiles(this BaseTile baseTile, int radius, Dictionary<Vector2Int, BaseTile> tiles)
        {
            foreach (var coordinate in GetCoordinatesInRadius(baseTile.Position, radius))
            {
                if(!tiles.TryGetValue(coordinate, out var tile))
                    continue;
                
                yield return tile;
            }
            
        }

        public static IEnumerable<BaseTile> GetEncompassingTiles(this BaseTile baseTile, int radius, Dictionary<Vector2Int, BaseTile> tiles)
        {
            foreach (var coordinate in GetCoordinatesInSquare(baseTile.Position, radius))
            {
                if(!tiles.TryGetValue(coordinate, out var tile))
                    continue;
                
                yield return tile;
            }
            
        }

        public static IEnumerable<Vector2Int> GetCardinalDirections(this BaseTile baseTile)
        {
            return CardinalDirections;
        }

        public static IEnumerable<Vector2Int> GetCoordinatesInRadius(Vector2Int center, int radius)
        {
            int rSquared = radius * radius;

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    //Exclude the center
                    if (x == 0 && y == 0)
                        continue;

                    if (x * x + y * y <= rSquared)
                        yield return new Vector2Int(center.x + x, center.y + y);
                }
            }
        }

        public static IEnumerable<Vector2Int> GetCoordinatesInSquare(Vector2Int center, int radius)
        {
            int rSquared = radius * radius;

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    //Exclude the center
                    if (x == 0 && y == 0)
                        continue;

                    yield return new Vector2Int(center.x + x, center.y + y);
                }
            }
        }

        public static IEnumerable<Vector2Int> GetCoordinatesInRadius(int radius)
        {
            int rSquared = radius * radius;

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    //Exclude the center
                    if (x == 0 && y == 0) 
                        continue;
                    
                    if (x * x + y * y <= rSquared)
                        yield return new Vector2Int(x, y);
                }
            }
        }
        
        public static List<BaseTile> FindAllSimilarConnected(
            this BaseTile start, 
            Dictionary<Vector2Int, 
                BaseTile> tiles)
        {
            List<BaseTile> result = new();
            HashSet<Vector2Int> visited = new();

            Search(start, start.TypeId, visited, result, tiles);

            return result;
        }

        private static void Search(
            BaseTile tile,
            int targetTypeID,
            HashSet<Vector2Int> visited,
            List<BaseTile> result,
            Dictionary<Vector2Int, BaseTile> tiles)
        {
            if (tile == null)
                return;

            if (!visited.Add(tile.Position))
                return;

            if (tile.TypeId != targetTypeID)
                return;

            result.Add(tile);

            foreach (Vector2Int dir in CardinalDirections)
            {
                if (tiles.TryGetValue(tile.Position + dir, out var neighbor))
                {
                    Search(neighbor, targetTypeID, visited, result, tiles);
                }
            }
        }
    }
}*/