using System;
using System.Collections.Generic;
using MapGeneration;
using MapGeneration.ScriptableObjects;
using Tiles;
using UnityEngine;


public interface IGenerate
{
    public static Action<int> OnNewMapSeed;
    
    int Seed { get; } 
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
    
    int GenerateMap(TilesetScriptableObject tileset, Dictionary<TILE_POS_UNIT, TILE_TYPE> tiles, Transform parent);

    TILE_TYPE CreateTile(TILE_POS_UNIT position, TileData data, Transform parent);
}
