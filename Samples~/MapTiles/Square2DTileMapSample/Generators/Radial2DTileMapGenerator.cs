using System;
using System.Collections.Generic;
using Samples.MapTiles.Generators;
using Samples.MapTiles.Square2DTiles.Tiles;
using Samples.MapTiles.Tiles;
using UnityEngine;
using Utilities;

using Object = UnityEngine.Object;

namespace Samples.MapTiles.Square2DTiles.Generators
{
    public class Radial2DTileMapGenerator : BaseMapGenerator<int, Square2DTile, Vector2Int>
    {
        public Radial2DTileMapGenerator(int seed, int mapSize, Transform parent) : base(seed, mapSize, parent)
        {
            
        }

        //BaseMapGenerator Overrides
        //================================================================================================================//

        protected override void DefaultMapPass(MapGenContext<Vector2Int> mapGenContext)
        {
            foreach (var coordinate in GetCoordinatesInRadius(MapSize))
            {
                if(mapGenContext.Tiles.ContainsKey(coordinate))
                    continue;
                    
                mapGenContext.Tiles.Add(coordinate, GetRandomTileData(OriginalSeedRandom) );
            }
        }

        protected override Square2DTile InstantiateTileObject(Vector2Int position, TileTypeDefinition typeDefinition, MapGenContext<Vector2Int> mapGenContext)
        {
            var prefab = typeDefinition.tileVariants.PickRandomElement(OriginalSeedRandom);
            
            if(prefab is not Square2DTile tilePrefab)
                throw new ArgumentException($"prefab [{prefab.name}] is not a {nameof(Square2DTile)}", nameof(prefab));
            
            var tile2DInstance = Object.Instantiate(
                tilePrefab, 
                mapGenContext.Layout.ToWorld(position),
                Quaternion.identity, Parent);
            
            tile2DInstance.gameObject.name = $"[{position.x},{position.y}]_{prefab.name}";
            
            tile2DInstance.Init(typeDefinition, position);

            return tile2DInstance;
        }

        //Helper Functions
        //================================================================================================================//
        
        private static IEnumerable<Vector2Int> GetCoordinatesInRadius(int radius)
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

        //================================================================================================================//

    }
}