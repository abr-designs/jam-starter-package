using System;
using System.Collections;
using System.Collections.Generic;
using Samples.MapTiles.ScriptableObjects;
using Samples.MapTiles.Tiles;


public interface IGenerateMap
{
    int Seed { get; } 
    //Used as seed to ensure generation sequence is the same
    System.Random OriginalSeedRandom { get; }
    
    void NextMap();
    void LoadMap(int seed);
    void SetSeed(int seed);
    void RandomMap();
}

public class MapGenContext<TILE_POS_UNIT>
{
    public ITileLayout<TILE_POS_UNIT> Layout;
    public Dictionary<TILE_POS_UNIT, TileTypeDefinition> Tiles;
    public Random Random;
    public TilesetScriptableObject Tileset;
    public int Seed;
}

// What a map needs: knows the tile and position types, not the map size.
public interface IGenerateMap<TILE_TYPE, TILE_POS_UNIT> : IGenerateMap where TILE_TYPE : BaseTile<TILE_POS_UNIT>
{
    int GenerateMap(TilesetScriptableObject tileset, ITileLayout<TILE_POS_UNIT> layout, Dictionary<TILE_POS_UNIT, TILE_TYPE> tiles);
}

// What a concrete generator exposes.
public interface IGenerateMap<MAP_SIZE_TYPE, TILE_TYPE, TILE_POS_UNIT> : IGenerateMap<TILE_TYPE, TILE_POS_UNIT> where TILE_TYPE : BaseTile<TILE_POS_UNIT>
{
    MAP_SIZE_TYPE MapSize { get; }
}
