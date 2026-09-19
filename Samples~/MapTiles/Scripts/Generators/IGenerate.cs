using System;
using System.Collections;
using System.Collections.Generic;
using MapGeneration;
using MapGeneration.ScriptableObjects;
using Tiles;
using UnityEngine;


public interface IGenerate
{
    public static event Action<int> OnNewMapSeed;
    
    int Seed { get; protected internal set; } 
    //Used as seed to ensure generation sequence is the same
    System.Random OriginalSeedRandom { get; protected set; }


    int GenerateMap(TilesetScriptableObject tileset, IDictionary tiles, Transform parent);

    //Defined Seed Functions
    //================================================================================================================//


    public void SetSeed(int seed)
    {
        OriginalSeedRandom = new System.Random(seed);
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
    
    //================================================================================================================//
    
    protected static TileData GetRandomTileData(TilesetScriptableObject tileset, System.Random random)
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

/// <summary>
/// 
/// </summary>
/// <typeparam name="MAP_SIZE_TYPE">Map Size Type</typeparam>
/// <typeparam name="TILE_TYPE">BaseTile Type</typeparam>
/// <typeparam name="TILE_POS_UNIT">Tile Position Unit Type</typeparam>
/// <typeparam name="TILE_SIZE_UNIT">Tile Size Type</typeparam>
public interface IGenerate<MAP_SIZE_TYPE, TILE_TYPE, TILE_POS_UNIT, TILE_SIZE_UNIT> : IGenerate where TILE_TYPE : BaseTile<TILE_POS_UNIT, TILE_SIZE_UNIT>
{
    MAP_SIZE_TYPE MapSize { get; }
    TILE_SIZE_UNIT TileSize { get; }

    Action<Dictionary<TILE_POS_UNIT, TileData>> PrePass { get; }
    List<Action<Dictionary<TILE_POS_UNIT, TileData>>> Passes { get; }

    int GenerateMap(TilesetScriptableObject tileset, Dictionary<TILE_POS_UNIT, TILE_TYPE> tiles, Transform parent);

    void ProcessPrePass(Dictionary<TILE_POS_UNIT, TileData> tiles);
    void ProcessPasses(Dictionary<TILE_POS_UNIT, TileData> tiles);

    void CreateTiles(in Dictionary<TILE_POS_UNIT, TileData> tileData, Transform parent, ref Dictionary<TILE_POS_UNIT, TILE_TYPE> generatedTiles);
    TILE_TYPE CreateTile(TILE_POS_UNIT position, TileData data, Transform parent);
}
