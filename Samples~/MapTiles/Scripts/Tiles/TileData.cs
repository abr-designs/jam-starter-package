using System;
using JamStarter.Utilities.Attributes;
using Tiles;
using UnityEngine;

namespace MapGeneration
{
    [Serializable]
    public class TileData
    {
        public string name;
        [ReadOnly]
        public int id;
        [TextArea]
        public string description;
        /*[Range(-10,99)]
        public int cost;
        public Vector2Int costMinMax;*/
        [Range(0,100)]
        public int spawnWeight;
        public BaseTile[] tileVariants;
    }
}