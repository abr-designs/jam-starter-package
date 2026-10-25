using System;
using System.Collections.Generic;
using System.Linq;
using Samples.MapTiles.ScriptableObjects;
using Samples.MapTiles.Tiles;
using Object = UnityEngine.Object;

namespace Samples.MapTiles.Maps
{
    public abstract class BaseMap
    {
        protected readonly TilesetScriptableObject Tileset;
        private readonly Dictionary<int, TileTypeDefinition> m_tileTypeDefinitions;
        public IGenerateMap MapGenerator { get; }

        protected BaseMap(TilesetScriptableObject tileset, IGenerateMap mapGenerator)
        {
            Tileset = tileset;
            MapGenerator = mapGenerator;
            
            m_tileTypeDefinitions =  new Dictionary<int, TileTypeDefinition>(tileset.tiles.Length);
            foreach (var tile in tileset.tiles)
            {
                m_tileTypeDefinitions.Add(tile.Id, tile);
            }
        }

        public TileTypeDefinition GetTileDefinition(TileType type) => m_tileTypeDefinitions[type.Id];
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TILE_TYPE">Tile Type</typeparam>
    /// <typeparam name="TILE_POS_UNIT">Tile Position Unit</typeparam>
    public abstract class BaseMap<TILE_TYPE, TILE_POS_UNIT> : BaseMap where TILE_TYPE : BaseTile<TILE_POS_UNIT>
    {
        public readonly Dictionary<TILE_POS_UNIT, TILE_TYPE> MapTiles;
        
        public new IGenerateMap<TILE_TYPE, TILE_POS_UNIT> MapGenerator { get; }
        public ITileLayout<TILE_POS_UNIT> TileLayout;
        
        protected BaseMap(TilesetScriptableObject tileset, IGenerateMap<TILE_TYPE, TILE_POS_UNIT> mapGenerator, ITileLayout<TILE_POS_UNIT> tileLayout) : base(tileset, mapGenerator)
        {
            MapGenerator = mapGenerator;
            MapTiles = new Dictionary<TILE_POS_UNIT, TILE_TYPE>();
            TileLayout = tileLayout;
        }

        //================================================================================================================//

        public virtual int Generate()
        {
            ClearMapTiles();
            return MapGenerator.GenerateMap(Tileset, TileLayout, MapTiles);
        }
        
        //Map Tile Searching
        //================================================================================================================//

        #region Map Tile Searching

        public IEnumerable<T> GetSurroundingTiles<T>(T mapTile, int radius) where T : TILE_TYPE
        {
            foreach (var coordinate in TileLayout.CoordinatesInRadius(mapTile.Position, radius))
            {
                if(!MapTiles.TryGetValue(coordinate, out var tile) || tile is not T tileType)
                    continue;
                
                yield return tileType;
            }
        }

        public List<TILE_TYPE> FindAllSimilarConnected(TILE_TYPE start)
        {
            List<TILE_TYPE> result = new();
            HashSet<TILE_POS_UNIT> visited = new();

            Search(start, start.TileID, visited, result);

            return result;
        }

        protected void Search<T>(T start, int targetTypeID, HashSet<TILE_POS_UNIT> visited, List<T> result) where T : TILE_TYPE
        {
            if (start == null || start.TileID != targetTypeID || !visited.Add(start.Position))
                return;

            var frontier = new Queue<T>();
            frontier.Enqueue(start);

            while (frontier.Count > 0)
            {
                var tile = frontier.Dequeue();
                result.Add(tile);

                foreach (var neighborPosition in TileLayout.Neighbours(tile.Position))
                {
                    if (!MapTiles.TryGetValue(neighborPosition, out var neighbor))
                        continue;
                    if (neighbor is not T typedNeighbor || typedNeighbor.TileID != targetTypeID)
                        continue;
                    if (!visited.Add(neighborPosition))
                        continue;

                    frontier.Enqueue(typedNeighbor);
                }
            }
        }

        #endregion //Map Tile Searching

        //Misc Functions
        //================================================================================================================//
        
        public T GetFirstTileWhere<T>(TileType typeId) where T : TILE_TYPE
        {
            return MapTiles.Values.OfType<T>().FirstOrDefault(x => x.TileID == typeId);
        }
        
        public IEnumerable<T> GetAllTiles<T>(int typeId) where T : TILE_TYPE
        {
            foreach (var tile in MapTiles)
            {
                if (tile.Value is not T tileType)
                    continue;
                
                if(tileType.TileID == typeId)
                    yield return tileType;
            }
        }
        public IEnumerable<T> GetAllTiles<T>(TileType typeId) where T : TILE_TYPE
        {
            foreach (var tile in MapTiles)
            {
                if (tile.Value is not T tileType)
                    continue;

                if (tileType.TileID == typeId)
                    yield return tileType;
            }
        }

        //Map Seed Generation Functions
        //================================================================================================================//

        public int Seed => MapGenerator.Seed;

        public void NextMap()
        {
            MapGenerator.NextMap();
            Generate();
        }

        public void RegenerateMap() => RegenerateMap(Seed);

        public void RegenerateMap(int seed)
        {
            MapGenerator.LoadMap(seed);
            Generate();
        }
        
        //Cleanup Functions
        //================================================================================================================//

        //TODO Opportunity to add some recycling here
        public void ClearMapTiles()
        {
            foreach (var (_, value) in MapTiles)
            {
                Object.Destroy(value.gameObject);
            }
            
            MapTiles.Clear();
        }

        //================================================================================================================//

    }
}