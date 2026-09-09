using System;
using System.Collections.Generic;
using MapGeneration.ScriptableObjects;
using Tiles;
using UnityEngine;


public interface IGenerate
{
    public static Action<int> OnNewMapSeed;
    
    int Seed { get; } 
    
    void NextMap();
    void RandomMap();
    void DailyMap();
    void LoadMap(int mapSeed);
    int GenerateMap(TilesetScriptableObject tileset, Dictionary<Vector2Int, BaseTile> tiles, Transform parent);
    
}
public interface IGenerate<out T> : IGenerate
{
    public T Size { get; }
}
