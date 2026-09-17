using System;
using System.Collections;
using System.Collections.Generic;
using MapGeneration.ScriptableObjects;
using Tiles;
using UnityEngine;
using UnityUtils;

using Object = UnityEngine.Object;
using Random = System.Random;

namespace MapGeneration.Generators
{
    public class GridMapGenerator : IGenerate<Vector2Int, Tile2D, Vector2Int, int>
    {
        public Vector2Int MapSize { get; }
        public int TileSize { get; }

        int IGenerate.Seed { get; set; }

        Random IGenerate.OriginalSeedRandom { get; set; }


        private Transform m_parent;

        public GridMapGenerator(int seed, Vector2Int mapSize, int tileSize)
        {
            ((IGenerate)this).Seed = seed;
            MapSize = mapSize;
            TileSize = tileSize;
        }


        public int GenerateMap(TilesetScriptableObject tileset, IDictionary tiles, Transform parent) => GenerateMap(tileset, (Dictionary<Vector2Int, Tile2D>)tiles, parent);

        public int GenerateMap(TilesetScriptableObject tileset, Dictionary<Vector2Int, Tile2D> tiles, Transform parent)
        {
            m_parent = parent;
            tiles.Clear();

            //GenerateStartAndEnd(MapSize, tileset, tiles);

            var tileCount = 0;

            for (var x = 0; x < MapSize.x; x++)
            {
                for (var y = 0; y < MapSize.y; y++)
                {
                    var position = new Vector2Int(x, y);
                    
                    //Skip if already exists, for start & exit
                    if(tiles.ContainsKey(position))
                        continue;
                    
                    tiles.Add(position, PickRandomTile(position, tileset) );
                    tileCount++;
                }
            }

            return tileCount;
        }

        //Generate Start & Exit
        //================================================================================================================//

        /*private void GenerateStartAndEnd(Vector2Int mapSize, TilesetScriptableObject tileset, Dictionary<Vector2Int, Tile2D> tiles)
        {
            var startPosition = new Vector2Int(1, mapSize.y / 2);
            var startTile = CreateTile(startPosition, tileset.tiles.FirstOrDefault(x => 
                string.Equals(x.name, TileHelper.START_TILE, StringComparison.InvariantCultureIgnoreCase)), m_parent);
            
            var exitY = m_randomMapGen.Next(1, mapSize.y - 2);
            var exitPosition = new Vector2Int(mapSize.x - 2, exitY);
            var endTile = CreateTile(exitPosition, tileset.tiles.FirstOrDefault(x => 
                string.Equals(x.name, TileHelper.END_TILE, StringComparison.InvariantCultureIgnoreCase)), m_parent);
            
            tiles.Add(startPosition, startTile);
            tiles.Add(exitPosition, endTile);
        }*/

        //Tile Factories
        //================================================================================================================//

        private Tile2D PickRandomTile(Vector2Int position, TilesetScriptableObject tileset)
        {
            var tileData = IGenerate.GetRandomTileData(tileset, ((IGenerate)this).OriginalSeedRandom);
            return CreateTile(position, tileData, m_parent);
        }
        
        public Tile2D CreateTile(Vector2Int position, TileData data, Transform parent)
        {
            if (data == null)
                throw new NullReferenceException("TileData is null");
                
            
            var prefab = data.tileVariants.Random();
            
            var simpleTileInstance = Object.Instantiate(
                prefab, 
                new Vector3(position.x * TileSize, 0f, position.y * TileSize),
                Quaternion.identity, parent) as Tile2D;
            
            simpleTileInstance.gameObject.name = $"[{position.x},{position.y}]_{prefab.name}";
            
            simpleTileInstance.Init(data, position);

            return simpleTileInstance;
        }
    }
}