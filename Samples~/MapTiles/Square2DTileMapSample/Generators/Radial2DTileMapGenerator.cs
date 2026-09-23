using MapGeneration.ScriptableObjects;
using System;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
using UnityUtils;

using Object = UnityEngine.Object;

namespace MapGeneration.Generators
{
    public class Radial2DTileMapGenerator : BaseMapGenerator<int, Square2DTile, Vector2Int>
    {
        public Radial2DTileMapGenerator(int seed, int mapSize, int tileSize, Transform parent) : base(seed, mapSize, tileSize, parent)
        {
            
        }

        //BaseMapGenerator Overrides
        //================================================================================================================//

        protected override void DefaultMapPass(TilesetScriptableObject tileset, Dictionary<Vector2Int, TileTypeDefinition> mapTileData)
        {
            foreach (var coordinate in GetCoordinatesInRadius(MapSize))
            {
                if(mapTileData.ContainsKey(coordinate))
                    continue;
                    
                mapTileData.Add(coordinate, GetRandomTileData(tileset, OriginalSeedRandom) );
            }
        }

        public override Square2DTile CreateTile(Vector2Int position, TileTypeDefinition typeDefinition)
        {
            var prefab = typeDefinition.tileVariants.Random();
            
            if(prefab is not Square2DTile tilePrefab)
                throw new ArgumentException($"prefab [{prefab.name}] is not a {nameof(Square2DTile)}", nameof(prefab));
            
            var tile2DInstance = Object.Instantiate(
                tilePrefab, 
                new Vector3(position.x * TileSize, 0f, position.y * TileSize),
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