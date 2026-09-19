using System;
using System.Collections.Generic;
using MapGeneration;
using UnityEngine;

namespace Tiles
{
    public static class TileHelper
    {
        private static Dictionary<int, TileData> s_idToTileData;
        private static Dictionary<string, TileData> s_nameToTileData;
        
        public static void RegisterTiles(TileData[] tiles)
        {
            s_idToTileData = new Dictionary<int, TileData>(tiles.Length);
            s_nameToTileData = new Dictionary<string, TileData>(tiles.Length);
            
            foreach (var tileData in tiles)
            {
                s_idToTileData.Add(tileData.id, tileData);
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
            return s_idToTileData.GetValueOrDefault(id);
        }

    }
}