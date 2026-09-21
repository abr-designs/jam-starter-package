using System;
using JamStarter.Utilities.Attributes;
using Tiles;
using UnityEngine;

namespace MapGeneration
{
    /// <summary>
    /// Extend this class to include custom functionality. Used as the main definition for any tiles.
    /// </summary>
    [Serializable]
    public class TileTypeDefinition : ITileType
    {
        [field: SerializeField]
        public string Name { get; set; }
        [field: SerializeField, ReadOnly]
        public int Id { get; set; }
        
        [TextArea]
        public string description;
        [Range(0,100)]
        public int spawnWeight;
        public BaseTile[] tileVariants;
    }
}