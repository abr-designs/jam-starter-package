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
        public Action<Dictionary<Vector2Int, TileData>> PrePass { get; }
        public List<Action<Dictionary<Vector2Int, TileData>>> Passes { get; }

        int IGenerate.Seed { get; set; }

        Random IGenerate.OriginalSeedRandom { get; set; }


        private Transform m_parent;

        public GridMapGenerator(int seed, Vector2Int mapSize, int tileSize)
        {
            MapSize = mapSize;
            TileSize = tileSize;
            
            ((IGenerate)this).SetSeed(seed);
        }


        public int GenerateMap(TilesetScriptableObject tileset, IDictionary tiles, Transform parent) => GenerateMap(tileset, (Dictionary<Vector2Int, Tile2D>)tiles, parent);

        public int GenerateMap(TilesetScriptableObject tileset, Dictionary<Vector2Int, Tile2D> tiles, Transform parent)
        {
            m_parent = parent;
            tiles.Clear();
            var mapData = new Dictionary<Vector2Int, TileData>();

            ProcessPrePass(mapData);
            
            var tileCount = 0;

            for (var x = 0; x < MapSize.x; x++)
            {
                for (var y = 0; y < MapSize.y; y++)
                {
                    var position = new Vector2Int(x, y);
                    
                    //Skip if already exists, for start & exit
                    if(tiles.ContainsKey(position))
                        continue;
                    
                    mapData.Add(position, PickRandomTile(position, tileset) );
                    tileCount++;
                }
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
            if (Passes == null || Passes.Count == 0)
                return;
            
            foreach (var pass in Passes)
            {
                pass.Invoke(tiles);
            }
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

        private TileData PickRandomTile(Vector2Int position, TilesetScriptableObject tileset)
        {
            var tileData = IGenerate.GetRandomTileData(tileset, ((IGenerate)this).OriginalSeedRandom);
            return tileData;
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
            if (data == null)
                throw new NullReferenceException("TileData is null");
                
            
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