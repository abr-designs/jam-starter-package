using System;
using System.Collections;
using System.Collections.Generic;
using Samples.MapTiles.ScriptableObjects;
using Samples.MapTiles.Tiles;
using UnityEngine;
using Random = System.Random;

namespace Samples.MapTiles.Generators
{
    public abstract class BaseMapGenerator<MAP_SIZE_TYPE, TILE_TYPE, TILE_POS_UNIT> : IGenerateMap<MAP_SIZE_TYPE, TILE_TYPE, TILE_POS_UNIT> where TILE_TYPE : BaseTile<TILE_POS_UNIT>
    {
        public static event Action<int> OnNewMapSeed;
        
        public int Seed { get; private set; }
        public Random OriginalSeedRandom { get; private set; }
        
        public MAP_SIZE_TYPE MapSize { get; }
        public Action<MapGenContext<TILE_POS_UNIT>> PrePass { get; set; }
        public List<Action<MapGenContext<TILE_POS_UNIT>>> Passes { get; set; } = new();
        
        protected readonly Transform Parent;

        // [Claude 2026-09-30 start]
        private float[] m_cumulativeWeights = Array.Empty<float>();
        private float m_totalWeight;
        private TileTypeDefinition[] m_spawnTiles;
        // [Claude 2026-09-30 end]
        

        protected BaseMapGenerator(int seed, MAP_SIZE_TYPE mapSize, Transform parentContainer)
        {
            MapSize = mapSize;
            Parent = parentContainer;

            SetSeed(seed);
        }
        
        public virtual int GenerateMap(TilesetScriptableObject tileset, ITileLayout<TILE_POS_UNIT> layout, Dictionary<TILE_POS_UNIT, TILE_TYPE> tiles)
        {
            SetSeed(Seed);
            
            tiles.Clear();
            BuildSpawnTable(tileset); // [Claude 2026-09-30]
            
            var mapGenContext = new MapGenContext<TILE_POS_UNIT>()
            {
                Tiles = new Dictionary<TILE_POS_UNIT, TileTypeDefinition>(),
                Tileset =  tileset,
                Layout = layout,
                Seed = Seed,
                Random = OriginalSeedRandom
            };

            ProcessPrePass(mapGenContext);
            
            DefaultMapPass(mapGenContext);
            
            ProcessPasses(mapGenContext);
            
            InstantiateTilesFromData(mapGenContext, ref tiles);

            return tiles.Count;
        }

        //Map Generation Passes
        //================================================================================================================//

        #region Map Generation Passes

        protected void ProcessPrePass(MapGenContext<TILE_POS_UNIT> mapGenContext)
        {
            PrePass?.Invoke(mapGenContext);
        }

        protected abstract void DefaultMapPass(MapGenContext<TILE_POS_UNIT> mapGenContext);

        protected void ProcessPasses(MapGenContext<TILE_POS_UNIT> mapGenContext)
        {
            if(Passes == null || Passes.Count == 0)
                return;
                
            foreach (var pass in Passes)
            {
                pass?.Invoke(mapGenContext);
            }
        }

        #endregion //Map Generation Passes

        
        //Tile Instantiation
        //================================================================================================================//

        #region Tile Instantiation

        private void InstantiateTilesFromData(MapGenContext<TILE_POS_UNIT> mapGenContext, ref Dictionary<TILE_POS_UNIT, TILE_TYPE> generatedTiles)
        {
            foreach (var (pos, td) in mapGenContext.Tiles)
            {
                if(!generatedTiles.TryAdd(pos, InstantiateTileObject(pos, td, mapGenContext)))
                    throw new Exception($"Tile {pos} has already been generated");
            }
        }

        protected abstract TILE_TYPE InstantiateTileObject(TILE_POS_UNIT position, TileTypeDefinition typeDefinition, MapGenContext<TILE_POS_UNIT> mapGenContext);

        #endregion //Tile Instantiation

        //================================================================================================================//
        
        /// <summary>
        /// Builds the cumulative spawn weights of the tileset once per generation. Reuses the array while the tile
        /// count stays the same. Subclasses that override GenerateMap without calling base must call this before
        /// <see cref="GetRandomTileData"/>.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        private void BuildSpawnTable(TilesetScriptableObject tileset)
        {
            m_spawnTiles = tileset.tiles;

            if (m_cumulativeWeights.Length != m_spawnTiles.Length)
                m_cumulativeWeights = new float[m_spawnTiles.Length];

            m_totalWeight = 0f;
            for (var i = 0; i < m_spawnTiles.Length; i++)
            {
                m_totalWeight += m_spawnTiles[i].spawnWeight;
                m_cumulativeWeights[i] = m_totalWeight;
            }
        }

        /// <summary>
        /// Picks a tile by spawn weight from the table built by <see cref="BuildSpawnTable"/>. Tiles with a weight of 0
        /// are never picked. Falls back to the first tile when nothing matches.
        /// </summary>
        /// <remarks>Rewritten by Claude (claude-opus-5-5), 2026-09-30</remarks>
        protected TileTypeDefinition GetRandomTileData(System.Random random)
        {
            var roll = (float)(random.NextDouble() * m_totalWeight);

            for (var i = 0; i < m_spawnTiles.Length; i++)
            {
                if (m_spawnTiles[i].spawnWeight == 0)
                    continue;

                if (roll < m_cumulativeWeights[i])
                    return m_spawnTiles[i];
            }

            return m_spawnTiles[0];
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
            SetSeed(Seed + 1);
        }

        public void RandomMap()
        {
            SetSeed(OriginalSeedRandom.Next());
        }

        public void DailyMap()
        {
            var now = DateTime.UtcNow;
            SetSeed(now.Year * 10000 + now.Month * 100 + now.Day);
        }

        public void LoadMap(int mapSeed) => SetSeed(mapSeed);

        #endregion //Seed Functions
        //================================================================================================================//


    }
}