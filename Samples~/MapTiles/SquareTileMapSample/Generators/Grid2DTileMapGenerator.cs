using System;
using System.Collections.Generic;
using MapGeneration.ScriptableObjects;
using Tiles;
using UnityEngine;
using UnityUtils;

using Object = UnityEngine.Object;

namespace MapGeneration.Generators
{
    public class Grid2DTileMapGenerator : BaseMapGenerator<Vector2Int, SquareTile2D, Vector2Int, int>
    {
        public Grid2DTileMapGenerator(int seed, Vector2Int mapSize, int tileSize, Transform parentContainer) : base(seed, mapSize, tileSize, parentContainer)
        {

        }

        //BaseMapGenerator Overrides
        //================================================================================================================//
        
        protected override void DefaultMapPass(TilesetScriptableObject tileset, Dictionary<Vector2Int, TileTypeDefinition> mapTileData)
        {
            for (var x = 0; x < MapSize.x; x++)
            {
                for (var y = 0; y < MapSize.y; y++)
                {
                    var position = new Vector2Int(x, y);
                    
                    //Skip if already exists, anything added in the pre-pass
                    if(mapTileData.ContainsKey(position))
                        continue;
                    
                    mapTileData.Add(position, GetRandomTileData(tileset, OriginalSeedRandom) );
                }
            }
        }

        public override SquareTile2D CreateTile(Vector2Int position, TileTypeDefinition typeDefinition)
        {
            var prefab = typeDefinition.tileVariants.Random();
            
            if(prefab is not SquareTile2D tilePrefab)
                throw new ArgumentException($"prefab [{prefab.name}] is not a {nameof(SquareTile2D)}", nameof(prefab));
            
            var tile2DInstance = Object.Instantiate(
                tilePrefab, 
                new Vector3(position.x * TileSize, 0f, position.y * TileSize),
                Quaternion.identity, Parent);
            
            tile2DInstance.gameObject.name = $"[{position.x},{position.y}]_{prefab.name}";
            
            tile2DInstance.Init(typeDefinition, position);

            return tile2DInstance;
        }

        //================================================================================================================//

    }
}