using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Samples.MapTiles.Maps;
using Samples.MapTiles.ScriptableObjects;
using Samples.MapTiles.Square2DTiles.Generators;
using Samples.MapTiles.Square2DTiles.Tiles;
using UnityEngine;

namespace Samples.MapTiles.Square2DTiles.Maps
{
    public class Square2DTileMap : BaseMap<Square2DTile, Vector2Int>
    {
        public Square2DTileMap(TilesetScriptableObject tileset, IGenerateMap<Square2DTile, Vector2Int> mapGenerator, Square2DLayout layout) : base(tileset, mapGenerator, layout)
        {
            
        }

        //Map Generator Helpers
        //================================================================================================================//


        //TODO See if there's a way for these to be automatically added via the Generators, to avoid needing to insert them each time
        /// <summary>
        /// Automatically generates a <see cref="Grid2DTileMapGenerator"/> map, returning the <see cref="Square2DTileMap"/>.
        /// Optionally you can include <see cref="MapGenContext"/> passes when calling
        /// </summary>
        /// <param name="tileset"></param>
        /// <param name="mapSize"></param>
        /// <param name="parent"></param>
        /// <param name="seed"></param>
        /// <param name="prepass"></param>
        /// <param name="passes"></param>
        /// <returns></returns>
        public static Square2DTileMap CreateGridMap(TilesetScriptableObject tileset, Vector2Int mapSize, Transform parent, 
            int seed = 0, 
            Action<MapGenContext<Vector2Int>> prepass = null,
            List<Action<MapGenContext<Vector2Int>>> passes = null)
        {
            var map = new Square2DTileMap(tileset, new Grid2DTileMapGenerator(seed, mapSize, parent)
            {
                PrePass = prepass,
                Passes = passes
            }, new Square2DLayout(tileset.tileSize));

            map.Generate();
            return map;
        }

        /// <summary>
        /// Automatically generates a <see cref="Radial2DTileMapGenerator"/> map, returning the <see cref="Square2DTileMap"/>.
        /// Optionally you can include <see cref="MapGenContext"/> passes when calling
        /// </summary>
        /// <param name="tileset"></param>
        /// <param name="mapRadius"></param>
        /// <param name="parent"></param>
        /// <param name="seed"></param>
        /// <param name="prepass"></param>
        /// <param name="passes"></param>
        /// <returns></returns>
        public static Square2DTileMap CreateRadialMap(TilesetScriptableObject tileset, int mapRadius, Transform parent,
            int seed = 0, 
            Action<MapGenContext<Vector2Int>> prepass = null,
            List<Action<MapGenContext<Vector2Int>>> passes = null)
        {
            var map = new Square2DTileMap(tileset, new Radial2DTileMapGenerator(seed, mapRadius, parent)
            {
                PrePass = prepass,
                Passes = passes
            }, new Square2DLayout(tileset.tileSize));
            
            map.Generate();
            return map;
        }

        //IGenerateMap Bridge Functions
        //================================================================================================================//



        //================================================================================================================//

    }
}