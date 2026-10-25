using System.Collections.Generic;
using Samples.MapTiles.Tiles;
using UnityEngine;

namespace Samples.MapTiles.Square2DTiles.Maps
{
    public class Square2DLayout : ITileLayout<Vector2Int>
    {
        private static readonly Vector2Int[] CardinalDirections =
        { 
            Vector2Int.up, 
            Vector2Int.down, 
            Vector2Int.left, 
            Vector2Int.right 
        };

        public float TileSize { get; }

        public Square2DLayout(float tileSize)
        {
            TileSize = tileSize;
        }
        
        public Vector3 ToWorld(Vector2Int position)
        {
            return new Vector3(position.x * TileSize, 0f, position.y * TileSize);
        }

        public Vector2Int FromWorld(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPosition.x / TileSize + 0.5f),
                Mathf.FloorToInt(worldPosition.z / TileSize + 0.5f));
        }

        public IEnumerable<Vector2Int> Neighbours(Vector2Int position)
        {
            foreach (var direction in CardinalDirections)
            {
                yield return position + direction;
            }
        }

        public IEnumerable<Vector2Int> CoordinatesInRadius(Vector2Int center, int radius)
        {
            var rSquared = radius * radius;

            for (var x = -radius; x <= radius; x++)
            {
                for (var y = -radius; y <= radius; y++)
                {
                    if (x * x + y * y <= rSquared)
                        yield return center + new Vector2Int(x, y);
                }
            }
        }

        public IEnumerable<Vector2Int> CoordinatesInSquare(Vector2Int center, int radius)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    yield return center + new Vector2Int(x, y);
                }
            }
        }
    }
}