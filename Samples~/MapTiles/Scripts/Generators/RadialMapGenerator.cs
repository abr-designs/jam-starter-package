using MapGeneration.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using Tiles;
using UnityEngine;
using UnityEngine.WSA;
using UnityUtils;

using Object = UnityEngine.Object;
using Random = System.Random;

namespace MapGeneration.Generators
{
    public class RadialMapGenerator : IGenerate<int, Tile2D, Vector2Int, int>
    {
        int IGenerate.Seed { get; set; }

        Random IGenerate.OriginalSeedRandom { get; set; }

        public int MapSize { get; }
        public int TileSize { get; }
        public Action<Dictionary<Vector2Int, TileData>> PrePass { get; set; }
        public List<Action<Dictionary<Vector2Int, TileData>>> Passes { get; set; }


        private readonly Func<int, IEnumerable<Vector2Int>> m_getCoordinatesInRadius;

        private Transform m_parent;

        public RadialMapGenerator(int seed, int mapSize, int tileSize, Transform parent)
        {
            MapSize = mapSize;
            TileSize = tileSize;
            m_parent = parent;
            
            ((IGenerate)this).SetSeed(seed);
        }

        /*private void SecondPassOfMapTileRules(TilesetScriptableObject tileset, Dictionary<Vector2Int, BaseTile> tiles)
        {
            // change graveyards surrounding tiles to deserts - should also change start and end surrounding tiles to replace graveyards with deserts
            ApplyMapGraveyardRules(tiles);
            ApplyMapStartRules(tiles);
        }*/

        /*private void ApplyMapGraveyardRules(Dictionary<Vector2Int, BaseTile> tiles)
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
        }*/



        public int GenerateMap(TilesetScriptableObject tileset, IDictionary tiles, Transform parent) => GenerateMap(tileset, (Dictionary<Vector2Int, Tile2D>)tiles, parent);

        public int GenerateMap(TilesetScriptableObject tileset, Dictionary<Vector2Int, Tile2D> tiles, Transform parent)
        {
            m_parent = parent;
            tiles.Clear();
            var mapData = new Dictionary<Vector2Int, TileData>();
            
            ProcessPrePass(mapData);

            int tileCount = 0;

            foreach (var coordinate in m_getCoordinatesInRadius(MapSize))
            {
                if(tiles.ContainsKey(coordinate))
                    continue;
                    
                mapData.Add(coordinate, PickRandomTile(coordinate, tileset) );
                tileCount++;
            }

            ProcessPasses(mapData);
            
            CreateTiles(mapData, m_parent, ref tiles);

            return tileCount;
        }

        public void ProcessPrePass(Dictionary<Vector2Int, TileData> tiles)
        {
            PrePass?.Invoke(tiles);
        }

        public void ProcessPasses(Dictionary<Vector2Int, TileData> tiles)
        {
            foreach (var pass in Passes)
            {
                pass.Invoke(tiles);
            }
        }

        public IEnumerable<Vector2Int> GetCoordinatesInRadius(int radius)
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

        //Generate Start & Exit
        //================================================================================================================//

        /*private void GenerateStartAndEnd(int mapSize, TilesetScriptableObject tileset, Dictionary<Vector2Int, BaseTile> tiles)
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
        }*/

        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        }*/
        
        //Tile Factories
        //================================================================================================================//

        private TileData PickRandomTile(Vector2Int position, TilesetScriptableObject tileset)
        {
            var tileData = IGenerate.GetRandomTileData(tileset, ((IGenerate)this).OriginalSeedRandom);
            return tileData;
            //return CreateTile(position, tileData, m_parent);
        }
        
        public void CreateTiles(in Dictionary<Vector2Int, TileData> tileData, Transform parent, ref Dictionary<Vector2Int, Tile2D> generatedTiles)
        {
            foreach (var (pos, td) in tileData)
            {
                if(!generatedTiles.TryAdd(pos, CreateTile(pos, td, parent)))
                    throw new Exception($"Tile {pos.x},{pos.y} has already been generated");
            }
        }
        
        public Tile2D CreateTile(Vector2Int position, TileData data, Transform parent)
        {
            var prefab = data.tileVariants.Random();
            
            var tile2dInstance = Object.Instantiate(
                prefab, 
                new Vector3(position.x * TileSize, 0f, position.y * TileSize),
                Quaternion.identity, parent) as Tile2D;
            
            tile2dInstance.gameObject.name = $"[{position.x},{position.y}]_{prefab.name}";
            
            tile2dInstance.Init(data, position);

            return tile2dInstance;
        }
    }
}