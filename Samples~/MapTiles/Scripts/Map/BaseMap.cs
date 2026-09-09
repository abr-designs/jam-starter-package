using System.Collections.Generic;
using Tiles;
using UnityEngine;

namespace Samples.MapTiles.Scripts.Map
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Tile Type</typeparam>
    /// <typeparam name="TU">Tile Position Unit</typeparam>
    /// <typeparam name="TS">Tile Size Unit</typeparam>
    public abstract class BaseMap<T, TU, TS> where T : BaseTile<TU, TS>
    {


        //Map Tile Searching
        //================================================================================================================//

        #region Map Tile Searching

        public abstract IEnumerable<BaseTile> GetSurroundingTiles(BaseTile baseTile, int radius, Dictionary<Vector2Int, BaseTile> tiles);
        public abstract IEnumerable<BaseTile> GetEncompassingTiles(BaseTile baseTile, int radius, Dictionary<Vector2Int, BaseTile> tiles);
        public abstract IEnumerable<Vector2Int> GetCardinalDirections(BaseTile baseTile);
        public abstract IEnumerable<Vector2Int> GetCoordinatesInRadius(Vector2Int center, int radius);
        public abstract IEnumerable<Vector2Int> GetCoordinatesInSquare(Vector2Int center, int radius);
        public abstract IEnumerable<Vector2Int> GetCoordinatesInRadius(int radius);
        public abstract List<BaseTile> FindAllSimilarConnected(BaseTile start, Dictionary<Vector2Int, BaseTile> tiles);
        protected abstract void Search(BaseTile tile, int targetTypeID, HashSet<Vector2Int> visited, List<BaseTile> result, Dictionary<Vector2Int, BaseTile> tiles);

        #endregion //Map Tile Searching

    }

    public class Map2DTiles : BaseMap<Tile2D, Vector2Int, float>
    {
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
}