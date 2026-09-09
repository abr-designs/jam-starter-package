using System;
using System.Collections.Generic;
using MapGeneration;
using UnityEngine;

namespace Tiles
{
    public interface IGetMapTiles
    {
        Dictionary<Vector2Int, BaseTile> Tiles { get; }
    }
    public static class TileHelper
    {
        public const string START_TILE = "start";
        public const string END_TILE = "exit";
        
        public static int START_TILE_ID => GetTileId(START_TILE);
        public static int END_TILE_ID => GetTileId(END_TILE);
        
        private static Dictionary<string, int> s_nameToId;
        private static Dictionary<int, string> s_IdToName;
        private static Dictionary<int, TileData> s_IdToTileData;
        private static Dictionary<string, TileData> s_nameToTileData;
        
        public static void RegisterTiles(TileData[] tiles)
        {
            s_nameToId = new Dictionary<string, int>(tiles.Length);
            s_IdToName = new Dictionary<int, string>(tiles.Length);
            s_IdToTileData = new Dictionary<int, TileData>(tiles.Length);
            s_nameToTileData = new Dictionary<string, TileData>(tiles.Length);
            
            foreach (var tileData in tiles)
            {
                s_nameToId.Add(tileData.name.ToLower(), tileData.id);
                s_IdToName.Add(tileData.id, tileData.name);
                s_IdToTileData.Add(tileData.id, tileData);
                s_nameToTileData.Add(tileData.name.ToLower(), tileData);
            }
        }

        public static TileData GetTileData(string name)
        {
            foreach (var tileData in s_nameToTileData)
            {
                if(string.Equals(tileData.Value.name, name, System.StringComparison.OrdinalIgnoreCase))
                    return tileData.Value;
            }
            
            throw new Exception($"Tile with name {name} not found");
        }
        public static TileData GetTileData(int id)
        {
            return s_IdToTileData.GetValueOrDefault(id);
        }
        public static int GetTileId(string name)
        {
            foreach (var tileData in s_nameToTileData)
            {
                if(string.Equals(tileData.Value.name, name, System.StringComparison.OrdinalIgnoreCase))
                    return tileData.Value.id;
            }
            
            throw new Exception($"Tile with name {name} not found");
        }

    }
}