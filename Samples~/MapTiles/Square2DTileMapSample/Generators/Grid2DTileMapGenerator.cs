using System;
using Samples.MapTiles.Generators;
using Samples.MapTiles.Square2DTiles.Tiles;
using Samples.MapTiles.Tiles;
using UnityEngine;
using Utilities;

using Object = UnityEngine.Object;

namespace Samples.MapTiles.Square2DTiles.Generators
{
    public class Grid2DTileMapGenerator : BaseMapGenerator<Vector2Int, Square2DTile, Vector2Int>
    {
        public Grid2DTileMapGenerator(int seed, Vector2Int mapSize, Transform parentContainer) : base(seed, mapSize, parentContainer)
        {

        }

        //BaseMapGenerator Overrides
        //================================================================================================================//
        
        protected override void DefaultMapPass(MapGenContext<Vector2Int> mapGenContext)
        {
            for (var x = 0; x < MapSize.x; x++)
            {
                for (var y = 0; y < MapSize.y; y++)
                {
                    var position = new Vector2Int(x, y);
                    
                    //Skip if already exists, anything added in the pre-pass
                    if(mapGenContext.Tiles.ContainsKey(position))
                        continue;
                    
                    mapGenContext.Tiles.Add(position, GetRandomTileData(OriginalSeedRandom) );
                }
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

        //================================================================================================================//

    }
}