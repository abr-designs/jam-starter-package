using System.Collections.Generic;
using MapGeneration.ScriptableObjects;
using Tiles;
using UnityEngine;

namespace Samples.MapTiles.Scripts.Map
{
    public class Map2DTiles : BaseMap<Tile2D, Vector2Int, int>
    {
        public Map2DTiles(TilesetScriptableObject tileset, IGenerate generator) : base(tileset, generator)
        {
            
        }


        //Map Helper Functions
        //================================================================================================================//

        #region Map Helper Functions

        public override IEnumerable<Tile2D> GetSurroundingTiles(Tile2D mapTile, int radius, Dictionary<Vector2Int, Tile2D> tiles)
        {
            foreach (var coordinate in GetCoordinatesInRadius(mapTile.Position, radius))
            {
                if(!tiles.TryGetValue(coordinate, out var tile))
                    continue;
                
                yield return tile;
            }
        }

        public override IEnumerable<Tile2D> GetEncompassingTiles(Tile2D baseTile, int radius, Dictionary<Vector2Int, Tile2D> tiles)
        {
            foreach (var coordinate in GetCoordinatesInSquare(baseTile.Position, radius))
            {
                if(!tiles.TryGetValue(coordinate, out var tile))
                    continue;
                
                yield return tile;
            }
        }

        public override IEnumerable<Vector2Int> GetCoordinatesInRadius(Vector2Int center, int radius)
        {
            var rSquared = radius * radius;

            for (var x = -radius; x <= radius; x++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    //Exclude the center
                    if (x == 0 && y == 0)
                        continue;

                    if (x * x + y * y <= rSquared)
                        yield return new Vector2Int(center.x + x, center.y + y);
                }
            }
        }



        public override List<Tile2D> FindAllSimilarConnected(Tile2D start, Dictionary<Vector2Int, Tile2D> tiles)
        {
            List<Tile2D> result = new();
            HashSet<Vector2Int> visited = new();

            Search(start, start.TypeId, visited, result, tiles);

            return result;
        }

        protected override void Search(Tile2D tile, int targetTypeID, HashSet<Vector2Int> visited, List<Tile2D> result, Dictionary<Vector2Int, Tile2D> tiles)
        {
            if (tile == null)
                return;

            if (!visited.Add(tile.Position))
                return;

            if (tile.TypeId != targetTypeID)
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

        #endregion //Map Helper Functions
        
        //Custom Map Helper Functions
        //================================================================================================================//

        #region Custom Map Helper Functions

        public IEnumerable<Vector2Int> GetCoordinatesInSquare(Vector2Int center, int radius)
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

        public IEnumerable<Vector2Int> GetCoordinatesInRadius(int radius)
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

        #endregion //Custom Map Helper Functions

        //================================================================================================================//
    }
}