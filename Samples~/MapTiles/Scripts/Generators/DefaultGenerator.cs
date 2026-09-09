using System;
using System.Collections.Generic;
using System.Linq;
using MapGeneration.ScriptableObjects;
using Tiles;
using UnityEngine;
using UnityUtils;
using Object = UnityEngine.Object;

namespace MapGeneration.Generators
{
    public class Default2DGenerator : IGenerate<Vector2Int>
    {
        public Vector2Int Size { get; }
        
        public int Seed { get; }
        //Used as seed to ensure generation sequence is the same
        private readonly System.Random m_originalSeedRandom;
        //Used to generate new maps
        private System.Random m_randomMapGen;

        private int mapSeed;
        
        private Transform m_parent;
        
        public Default2DGenerator(int seed, Vector2Int size)
        {
            Seed = seed;
            Size = size;
            
            m_originalSeedRandom = new System.Random(seed);
            mapSeed = seed;
            IGenerate.OnNewMapSeed?.Invoke(mapSeed);
        }

        // NO LONGER USED - MOVED TO RADIAL GENERATOR
        public void NextMap()
        {
            mapSeed += 1;
            IGenerate.OnNewMapSeed?.Invoke(mapSeed);
        }

        // NO LONGER USED - MOVED TO RADIAL GENERATOR
        public void RandomMap()
        {
            mapSeed = m_originalSeedRandom.Next();
            IGenerate.OnNewMapSeed?.Invoke(mapSeed);
        }

        // NO LONGER USED - MOVED TO RADIAL GENERATOR
        public void DailyMap()
        {
            var now = System.DateTime.UtcNow;
            mapSeed = now.Year * 10000 + now.Month * 100 + now.Day;
            IGenerate.OnNewMapSeed?.Invoke(mapSeed);
        }

        // NO LONGER USED - MOVED TO RADIAL GENERATOR
        public void LoadMap(int mapSeed)
        {
            this.mapSeed = mapSeed;
            IGenerate.OnNewMapSeed?.Invoke(mapSeed);
        }

        public int GenerateMap(TilesetScriptableObject tileset, Dictionary<Vector2Int, BaseTile> tiles, Transform parent)
        {
            m_randomMapGen = new System.Random(mapSeed);
            m_parent = parent;
            tiles.Clear();

            GenerateStartAndEnd(Size, tileset, tiles);

            int tileCount = 0;

            for (int x = 0; x < Size.x; x++)
            {
                for (int y = 0; y < Size.y; y++)
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

        private void GenerateStartAndEnd(Vector2Int mapSize, TilesetScriptableObject tileset, Dictionary<Vector2Int, BaseTile> tiles)
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
        }

        //Tile Factories
        //================================================================================================================//

        private BaseTile PickRandomTile(Vector2Int position, TilesetScriptableObject tileset)
        {
            var tileData = GetRandomTileData(tileset, m_randomMapGen);
            return CreateTile(position, tileData, m_parent);
        }
        
        private static BaseTile CreateTile(Vector2Int position, TileData data, Transform parent)
        {
            if (data == null)
                throw new NullReferenceException("TileData is null");
                
            
            var prefab = data.tileVariants.Random();
            
            var simpleTileInstance = Object.Instantiate(
                prefab, 
                new Vector3(position.x * IGenerate.TILE_SIZE, 0f, position.y * IGenerate.TILE_SIZE),
                Quaternion.identity, parent);
            
            simpleTileInstance.gameObject.name = $"[{position.x},{position.y}]_{prefab.name}";
            
            simpleTileInstance.Init(/*TILE_STATE.HIDDEN, */data, position);

            return simpleTileInstance;
        }

        //Utilities
        //================================================================================================================//

        private static TileData GetRandomTileData(TilesetScriptableObject tileset, System.Random random)
        {
            var tiles = tileset.tiles;
            
            float total = 0f;
            for (int i = 0; i < tiles.Length; i++) 
                total += tiles[i].spawnWeight;

            float roll = (float)(random.NextDouble() * total);
            float cumulative = 0f;

            for (int i = 0; i < tiles.Length; i++)
            {
                if(tiles[i].spawnWeight == 0)
                    continue;
                
                cumulative += tiles[i].spawnWeight;
                if (roll < cumulative) 
                    return tiles[i];
            }
            return tiles[0];
        }

    }
}