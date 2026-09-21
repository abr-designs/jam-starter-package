using System;
using System.Collections;
using System.Collections.Generic;
using MapGeneration.ScriptableObjects;
using Tiles;
using UnityEngine;
using Random = System.Random;

namespace MapGeneration.Generators
{
    public abstract class BaseMapGenerator<MAP_SIZE_TYPE, TILE_TYPE, TILE_POS_UNIT, TILE_SIZE_UNIT> : IGenerateMap<MAP_SIZE_TYPE, TILE_TYPE, TILE_POS_UNIT, TILE_SIZE_UNIT> where TILE_TYPE : BaseTile<TILE_POS_UNIT>
    {
        public static event Action<int> OnNewMapSeed;

        public int Seed { get; private set; }
        public Random OriginalSeedRandom { get; private set; }

        public MAP_SIZE_TYPE MapSize { get; }
        public TILE_SIZE_UNIT TileSize { get; }
        public Action<Dictionary<TILE_POS_UNIT, TileTypeDefinition>> PrePass { get; set; }
        public List<Action<Dictionary<TILE_POS_UNIT, TileTypeDefinition>>> Passes { get; set; }
        
        protected readonly Transform Parent;
        

        protected BaseMapGenerator(int seed, MAP_SIZE_TYPE mapSize, TILE_SIZE_UNIT tileSize, Transform parentContainer)
        {
            MapSize = mapSize;
            TileSize = tileSize;
            Parent = parentContainer;
            
            SetSeed(seed);
        }
        
        // IGenerate Base call
        public int GenerateMap(TilesetScriptableObject tileset, IDictionary tiles)
        {
            return GenerateMap(tileset, (Dictionary<TILE_POS_UNIT, TILE_TYPE>)tiles);
        }
        
        public virtual int GenerateMap(TilesetScriptableObject tileset, Dictionary<TILE_POS_UNIT, TILE_TYPE> tiles)
        {
            tiles.Clear();
            var mapData = new Dictionary<TILE_POS_UNIT, TileTypeDefinition>();

            ProcessPrePass(mapData);
            
            DefaultMapPass(tileset, mapData);
            
            ProcessPasses(mapData);
            
            CreateTiles(mapData, ref tiles);

            return tiles.Count;
        }

        //Map Generation Passes
        //================================================================================================================//

        #region Map Generation Passes

        public void ProcessPrePass(Dictionary<TILE_POS_UNIT, TileTypeDefinition> mapTileData)
        {
            PrePass?.Invoke(mapTileData);
        }

        protected abstract void DefaultMapPass(TilesetScriptableObject tileset, Dictionary<TILE_POS_UNIT, TileTypeDefinition> mapTileData);

        public void ProcessPasses(Dictionary<TILE_POS_UNIT, TileTypeDefinition> mapTileData)
        {
            foreach (var pass in Passes)
            {
                pass.Invoke(mapTileData);
            }
        }

        #endregion //Map Generation Passes

        
        //Tile Instantiation
        //================================================================================================================//

        #region Tile Instantiation

        public void CreateTiles(in Dictionary<TILE_POS_UNIT, TileTypeDefinition> mapTileData, ref Dictionary<TILE_POS_UNIT, TILE_TYPE> generatedTiles)
        {
            foreach (var (pos, td) in mapTileData)
            {
                if(!generatedTiles.TryAdd(pos, CreateTile(pos, td)))
                    throw new Exception($"Tile {pos} has already been generated");
            }
        }

        public abstract TILE_TYPE CreateTile(TILE_POS_UNIT position, TileTypeDefinition typeDefinition);

        #endregion //Tile Instantiation

        //================================================================================================================//
        
        protected static TileTypeDefinition GetRandomTileData(TilesetScriptableObject tileset, System.Random random)
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

        //Seed Functions
        //================================================================================================================//

        #region Seed Functions

        public void SetSeed(int seed)
        {
            OriginalSeedRandom = new Random(seed);
            Seed = seed;
            OnNewMapSeed?.Invoke(Seed);
        }
    
        public void NextMap()
        {
            Seed += 1;
            OnNewMapSeed?.Invoke(Seed);
        }

        public void RandomMap()
        {
            Seed = OriginalSeedRandom.Next();
            OnNewMapSeed?.Invoke(Seed);
        }

        public void DailyMap()
        {
            var now = DateTime.UtcNow;
            Seed = now.Year * 10000 + now.Month * 100 + now.Day;
            OnNewMapSeed?.Invoke(Seed);
        }

        public void LoadMap(int mapSeed)
        {
            Seed = mapSeed;
            OnNewMapSeed?.Invoke(mapSeed);
        }

        #endregion //Seed Functions

        //================================================================================================================//

    }
}