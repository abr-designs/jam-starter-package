using System.Collections.Generic;
using Samples.MapTiles.ScriptableObjects;
using Samples.MapTiles.Square2DTiles.Generators;
using Samples.MapTiles.Square2DTiles.Maps;
using Samples.MapTiles.Tiles;
using UnityEngine;

namespace Samples.MapTiles
{
    public class MapSample : MonoBehaviour
    {
        [Header("Map Generation")]
        public TilesetScriptableObject mapTileset;
        public int mapSeed;
        public bool useRadial;
        
        [Header("Grid Map")]
        public Vector2Int mapSize;

        [Header("Radial Map"), Min(1)] 
        public int mapRadius;

        private Square2DTileMap m_myTileMap;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            m_myTileMap = new Square2DTileMap(
                mapTileset,
                useRadial ? 
                    new Radial2DTileMapGenerator(mapSeed, mapRadius, (int)mapTileset.tileSize, transform)
                    {
                        PrePass = ApplyMapGraveyardRules,
                        Passes =
                        {
                            ApplyMapGraveyardRules,
                            ApplyMapGraveyardRules,
                        }
                    }:
                    new Grid2DTileMapGenerator(mapSeed, mapSize, (int)mapTileset.tileSize, transform));
            
            m_myTileMap.Generate();
        }

        private void ApplyMapGraveyardRules(Dictionary<Vector2Int, TileTypeDefinition> tiles)
        {
            
        }
    
                /*private void SecondPassOfMapTileRules(TilesetScriptableObject tileset, Dictionary<Vector2Int, BaseTile> tiles)
        {
            // change graveyards surrounding tiles to deserts - should also change start and end surrounding tiles to replace graveyards with deserts
            ApplyMapGraveyardRules(tiles);
            ApplyMapStartRules(tiles);
        }*/

        /*private void ApplyMapGraveyardRules(Dictionary<Vector2Int, BaseTile> tiles)
        {
            int graveyardId = TileHelper.GetTileId("graveyard");
            int desertId = TileHelper.GetTileId("desert");
            int startId = TileHelper.GetTileId("start");
            int exitId = TileHelper.GetTileId("exit");

            Dictionary<Vector2Int, BaseTile> tilesToAdd = new Dictionary<Vector2Int, BaseTile>();
            foreach (var pair in tiles)
            {
                BaseTile tile = pair.Value;

                if (tile.TileData.id != graveyardId)
                    continue;

                foreach (BaseTile neighbour in SimpleTileBaseExtensions.GetEncompassingTiles(tile, 1, tiles))
                {
                    // checked for exit and start
                    if (neighbour.TileData.id == startId
                        || neighbour.TileData.id == exitId)

                        continue;

                    Vector2Int tilePosition = neighbour.Position;

                    if (!Application.isPlaying)
                    {
                        // destory existing object
                        Object.DestroyImmediate(neighbour.gameObject);
                    }
                    else
                    {
                        // destory existing object
                        Object.Destroy(neighbour.gameObject);
                    }

                    // spawn new simple tile
                    tilesToAdd[tilePosition] = (CreateTile(tilePosition, TileHelper.GetTileData(desertId), m_parent));
                }
            }

            // add changed tiles
            foreach (var pair in tilesToAdd)
            {
                tiles[pair.Key] = pair.Value;
            }
        }

        private void ApplyMapStartRules(Dictionary<Vector2Int, BaseTile> tiles)
        {
            int startId = TileHelper.GetTileId("start");
            int plainId = TileHelper.GetTileId("plains");
            int graveyardId = TileHelper.GetTileId("graveyard");
            int mountainId = TileHelper.GetTileId("mountain");
            int castleId = TileHelper.GetTileId("castle");
            int evlenRangerId = TileHelper.GetTileId("elven ranger");
            int orcCampId = TileHelper.GetTileId("orc camp");

            List<int> disallowedTileIds = new List<int>
            {
                graveyardId,
                mountainId,
                castleId,
                evlenRangerId,
                orcCampId
            };

            Dictionary<Vector2Int, BaseTile> tilesToAdd = new Dictionary<Vector2Int, BaseTile>();
            foreach (var pair in tiles)
            {
                BaseTile tile = pair.Value;

                if (tile.TileData.id != startId)
                    continue;

                foreach (BaseTile neighbour in SimpleTileBaseExtensions.GetEncompassingTiles(tile, 1, tiles))
                {
                    if (!disallowedTileIds.Contains(neighbour.TileData.id))
                        continue;

                    Vector2Int tilePosition = neighbour.Position;

                    // destory existing object
                    Object.Destroy(neighbour.gameObject);

                    // spawn new simple tile
                    tilesToAdd[tilePosition] = (CreateTile(tilePosition, TileHelper.GetTileData(plainId), m_parent));
                }
            }

            // add changed tiles
            foreach (var pair in tilesToAdd)
            {
                tiles[pair.Key] = pair.Value;
            }
        }*/
        
        /*private void GenerateStartAndEnd(int mapSize, TilesetScriptableObject tileset, Dictionary<Vector2Int, BaseTile> tiles)
{
    const float MIN_EXIT_DISTANCE = 0.5f;

    var startPosition = new Vector2Int(0, 0);
    var startTile = CreateTile(startPosition, tileset.tiles.FirstOrDefault(x =>
        string.Equals(x.name, TileHelper.START_TILE, StringComparison.InvariantCultureIgnoreCase)), m_parent);

    var unitCircle = GetRandomPointWithMinDistance(m_randomMapGen, MIN_EXIT_DISTANCE, 1f);

    var exitPosition = new Vector2Int(
        Mathf.RoundToInt(unitCircle.x * mapSize),
        Mathf.RoundToInt(unitCircle.y * mapSize));
    var endTile = CreateTile(exitPosition, tileset.tiles.FirstOrDefault(x =>
        string.Equals(x.name, TileHelper.END_TILE, StringComparison.InvariantCultureIgnoreCase)), m_parent);

    tiles.Add(startPosition, startTile);
    tiles.Add(exitPosition, endTile);
}*/
    }
}
