using MapGeneration.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Tiles;
using UnityEngine;
using UnityUtils;
using Utilities;
using Object = UnityEngine.Object;

namespace MapGeneration.Generators
{
    public class RadialGenerator : IGenerate<int>
    {
        public int Seed { get; }

        public int Size { get; }

        //Used as seed to ensure generation sequence is the same
        private readonly System.Random m_originalSeedRandom;
        //Used to generate new maps
        private System.Random m_randomMapGen;

        private int mapSeed;
        public int MapSeed => mapSeed; // TODO - needs to be part of the interface
        
        private Transform m_parent;
        
        public RadialGenerator(int seed, int size)
        {
            Seed = seed;
            Size = size;
            
            m_originalSeedRandom = new System.Random(seed);
            mapSeed = seed;
            IGenerate.OnNewMapSeed?.Invoke(mapSeed);
        }

        public void NextMap()
        {
            mapSeed += 1;
            IGenerate.OnNewMapSeed?.Invoke(mapSeed);
        }

        public void RandomMap()
        {
            mapSeed = m_originalSeedRandom.Next();
            IGenerate.OnNewMapSeed?.Invoke(mapSeed);
        }

        public void DailyMap()
        {
            var now = System.DateTime.UtcNow;
            mapSeed = now.Year * 10000 + now.Month * 100 + now.Day;
            IGenerate.OnNewMapSeed?.Invoke(mapSeed);
        }

        public void LoadMap(int mapSeed)
        {
            this.mapSeed = mapSeed;
            IGenerate.OnNewMapSeed?.Invoke(mapSeed);
        }

        private void SecondPassOfMapTileRules(TilesetScriptableObject tileset, Dictionary<Vector2Int, BaseTile> tiles)
        {
            // change graveyards surrounding tiles to deserts - should also change start and end surrounding tiles to replace graveyards with deserts
            ApplyMapGraveyardRules(tiles);
            ApplyMapStartRules(tiles);
        }

        private void ApplyMapGraveyardRules(Dictionary<Vector2Int, BaseTile> tiles)
        {
            int graveyardId = TileHelper.GetTileId("graveyard");
            int desertId = TileHelper.GetTileId("desert");
            int startId = TileHelper.GetTileId("start");
            int exitId = TileHelper.GetTileId("exit");

            Dictionary<Vector2Int, BaseTile> tilesToAdd = new Dictionary<Vector2Int, BaseTile>();
            foreach (var pair in tiles)
            {
                BaseTile tile = pair.Value;

                if (tile.TileData.id != graveyardId)
                    continue;

                foreach (BaseTile neighbour in SimpleTileBaseExtensions.GetEncompassingTiles(tile, 1, tiles))
                {
                    // checked for exit and start
                    if (neighbour.TileData.id == startId
                        || neighbour.TileData.id == exitId)

                        continue;

                    Vector2Int tilePosition = neighbour.Position;

                    if (!Application.isPlaying)
                    {
                        // destory existing object
                        Object.DestroyImmediate(neighbour.gameObject);
                    }
                    else
                    {
                        // destory existing object
                        Object.Destroy(neighbour.gameObject);
                    }

                    // spawn new simple tile
                    tilesToAdd[tilePosition] = (CreateTile(tilePosition, TileHelper.GetTileData(desertId), m_parent));
                }
            }

            // add changed tiles
            foreach (var pair in tilesToAdd)
            {
                tiles[pair.Key] = pair.Value;
            }
        }

        private void ApplyMapStartRules(Dictionary<Vector2Int, BaseTile> tiles)
        {
            int startId = TileHelper.GetTileId("start");
            int plainId = TileHelper.GetTileId("plains");
            int graveyardId = TileHelper.GetTileId("graveyard");
            int mountainId = TileHelper.GetTileId("mountain");
            int castleId = TileHelper.GetTileId("castle");
            int evlenRangerId = TileHelper.GetTileId("elven ranger");
            int orcCampId = TileHelper.GetTileId("orc camp");

            List<int> disallowedTileIds = new List<int>
            {
                graveyardId,
                mountainId,
                castleId,
                evlenRangerId,
                orcCampId
            };

            Dictionary<Vector2Int, BaseTile> tilesToAdd = new Dictionary<Vector2Int, BaseTile>();
            foreach (var pair in tiles)
            {
                BaseTile tile = pair.Value;

                if (tile.TileData.id != startId)
                    continue;

                foreach (BaseTile neighbour in SimpleTileBaseExtensions.GetEncompassingTiles(tile, 1, tiles))
                {
                    if (!disallowedTileIds.Contains(neighbour.TileData.id))
                        continue;

                    Vector2Int tilePosition = neighbour.Position;

                    // destory existing object
                    Object.Destroy(neighbour.gameObject);

                    // spawn new simple tile
                    tilesToAdd[tilePosition] = (CreateTile(tilePosition, TileHelper.GetTileData(plainId), m_parent));
                }
            }

            // add changed tiles
            foreach (var pair in tilesToAdd)
            {
                tiles[pair.Key] = pair.Value;
            }
        }

        public int GenerateMap(TilesetScriptableObject tileset, Dictionary<Vector2Int, BaseTile> tiles, Transform parent)
        {
            m_randomMapGen = new System.Random(mapSeed);
            m_parent = parent;
            tiles.Clear();

            GenerateStartAndEnd(Size, tileset, tiles);

            int tileCount = 0;

            foreach (var coordinate in SimpleTileBaseExtensions.GetCoordinatesInRadius(Size))
            {
                if(tiles.ContainsKey(coordinate))
                    continue;
                    
                tiles.Add(coordinate, PickRandomTile(coordinate, tileset) );
                tileCount++;
            }

            SecondPassOfMapTileRules(tileset, tiles);

            //Debug.Log($"tileCount = {tileCount}");
            return tileCount;
        }

        //Generate Start & Exit
        //================================================================================================================//

        private void GenerateStartAndEnd(int mapSize, TilesetScriptableObject tileset, Dictionary<Vector2Int, BaseTile> tiles)
        {
            const float MIN_EXIT_DISTANCE = 0.5f;
            
            var startPosition = new Vector2Int(0, 0);
            var startTile = CreateTile(startPosition, tileset.tiles.FirstOrDefault(x => 
                string.Equals(x.name, TileHelper.START_TILE, StringComparison.InvariantCultureIgnoreCase)), m_parent);

            var unitCircle = GetRandomPointWithMinDistance(m_randomMapGen, MIN_EXIT_DISTANCE, 1f);
            
            var exitPosition = new Vector2Int(
                Mathf.RoundToInt(unitCircle.x * mapSize), 
                Mathf.RoundToInt(unitCircle.y * mapSize));
            var endTile = CreateTile(exitPosition, tileset.tiles.FirstOrDefault(x => 
                string.Equals(x.name, TileHelper.END_TILE, StringComparison.InvariantCultureIgnoreCase)), m_parent);
            
            tiles.Add(startPosition, startTile);
            tiles.Add(exitPosition, endTile);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 GetRandomPointWithMinDistance(
            System.Random rand, 
            float minRadius, 
            float maxRadius = 1.0f)
        {
            // 1. Get a uniform random angle
            var theta = (float)rand.NextDouble() * 2.0f * Mathf.PI;
    
            // 2. Uniformly distribute the area between minRadius and maxRadius
            var minRadiusSq = minRadius * minRadius;
            var maxRadiusSq = maxRadius * maxRadius;
    
            // Linear interpolation between squared radii, then square-rooted
            var radius = Mathf.Sqrt(minRadiusSq + (float)rand.NextDouble() * (maxRadiusSq - minRadiusSq));
    
            // 3. Convert to Cartesian coordinates
            var x = radius * Mathf.Cos(theta);
            var y = radius * Mathf.Sin(theta);
    
            return new Vector2(x, y);
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