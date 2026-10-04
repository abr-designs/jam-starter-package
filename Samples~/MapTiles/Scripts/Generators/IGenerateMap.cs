using System;
using System.Collections;
using System.Collections.Generic;
using Samples.MapTiles.ScriptableObjects;
using Samples.MapTiles.Tiles;


public interface IGenerateMap
{
    public static event Action<int> OnNewMapSeed;
    
    int Seed { get; set; } 
    //Used as seed to ensure generation sequence is the same
    System.Random OriginalSeedRandom { get; set; }
    

    int GenerateMap(TilesetScriptableObject tileset, IDictionary tiles);
    
    public void SetSeed(int seed)
    {
        OriginalSeedRandom = new Random(seed);
        Seed = seed;
        OnNewMapSeed?.Invoke(Seed);
    }
    
    public void NextMap()
    {
        SetSeed(Seed + 1);
        OnNewMapSeed?.Invoke(Seed);
    }

    public void RandomMap()
    {
        SetSeed(OriginalSeedRandom.Next());
        OnNewMapSeed?.Invoke(Seed);
    }

    public void DailyMap()
    {
        var now = DateTime.UtcNow;
        SetSeed(now.Year * 10000 + now.Month * 100 + now.Day);
        OnNewMapSeed?.Invoke(Seed);
    }

    public void LoadMap(int mapSeed) => SetSeed(mapSeed);
}

public class MapGenContext<TIlePosUnit>
{
    //MapGenContext<TPos>` { Tiles, Random, Tileset, Layout, Seed }
    public Dictionary<TIlePosUnit, TileTypeDefinition> Tiles;
    public Random Random;
    public TilesetScriptableObject Tileset;
    public int Seed;
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="MAP_SIZE_TYPE">Map Size Type</typeparam>
/// <typeparam name="TILE_TYPE">BaseTile Type</typeparam>
/// <typeparam name="TILE_POS_UNIT">Tile Position Unit Type</typeparam>
public interface IGenerateMap<MAP_SIZE_TYPE, TILE_TYPE, TILE_POS_UNIT> : IGenerateMap where TILE_TYPE : BaseTile<TILE_POS_UNIT>
{
    MAP_SIZE_TYPE MapSize { get; }
    float TileSize { get; }

    /// <summary>
    /// Data Pre-pass for the map generation. This is meant to be used by external sources to steer the generation by pre-placing
    /// tiles in the world. All pre-placed tiles by default will not be overwritten
    /// </summary>
    Action<MapGenContext<TILE_POS_UNIT>> PrePass { get; }
    /// <summary>
    /// Once the generator default pass has complete, these passes will commence. This is another opportunity to edit the
    /// map data from external locations prior to it being generated
    /// </summary>
    List<Action<MapGenContext<TILE_POS_UNIT>>> Passes { get; }

    int GenerateMap(TilesetScriptableObject tileset, Dictionary<TILE_POS_UNIT, TILE_TYPE> tiles);

    void ProcessPrePass(MapGenContext<TILE_POS_UNIT> mapGenContext);
    void ProcessPasses(MapGenContext<TILE_POS_UNIT> mapGenContext);

    void CreateTiles(MapGenContext<TILE_POS_UNIT> mapGenContext, ref Dictionary<TILE_POS_UNIT, TILE_TYPE> generatedTiles);
    TILE_TYPE CreateTile(TILE_POS_UNIT position, TileTypeDefinition typeDefinition);
}
