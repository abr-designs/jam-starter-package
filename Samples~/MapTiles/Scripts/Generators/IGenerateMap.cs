using System;
using System.Collections;
using System.Collections.Generic;
using MapGeneration;
using MapGeneration.ScriptableObjects;
using Tiles;


public interface IGenerateMap
{
    int Seed { get; } 
    //Used as seed to ensure generation sequence is the same
    System.Random OriginalSeedRandom { get; }

    int GenerateMap(TilesetScriptableObject tileset, IDictionary tiles);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="MAP_SIZE_TYPE">Map Size Type</typeparam>
/// <typeparam name="TILE_TYPE">BaseTile Type</typeparam>
/// <typeparam name="TILE_POS_UNIT">Tile Position Unit Type</typeparam>
/// <typeparam name="TILE_SIZE_UNIT">Tile Size Type</typeparam>
public interface IGenerateMap<MAP_SIZE_TYPE, TILE_TYPE, TILE_POS_UNIT, TILE_SIZE_UNIT> : IGenerateMap where TILE_TYPE : BaseTile<TILE_POS_UNIT>
{
    MAP_SIZE_TYPE MapSize { get; }
    TILE_SIZE_UNIT TileSize { get; }

    Action<Dictionary<TILE_POS_UNIT, TileTypeDefinition>> PrePass { get; }
    List<Action<Dictionary<TILE_POS_UNIT, TileTypeDefinition>>> Passes { get; }

    int GenerateMap(TilesetScriptableObject tileset, Dictionary<TILE_POS_UNIT, TILE_TYPE> tiles);

    void ProcessPrePass(Dictionary<TILE_POS_UNIT, TileTypeDefinition> tiles);
    void ProcessPasses(Dictionary<TILE_POS_UNIT, TileTypeDefinition> tiles);

    void CreateTiles(in Dictionary<TILE_POS_UNIT, TileTypeDefinition> tileData, ref Dictionary<TILE_POS_UNIT, TILE_TYPE> generatedTiles);
    TILE_TYPE CreateTile(TILE_POS_UNIT position, TileTypeDefinition typeDefinition);
}
