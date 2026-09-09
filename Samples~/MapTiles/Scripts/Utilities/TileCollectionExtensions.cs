using System.Collections.Generic;
using System.Linq;
using Tiles;
using UnityEngine;

namespace Utilities
{
    public static class TileCollectionExtensions
    {
        public static readonly Vector2Int[] CardinalDirections = {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};
        
        public static BaseTile GetFirstTile(this Dictionary<Vector2Int, BaseTile> tiles, int typeId)
        {
            return tiles.Values.FirstOrDefault(x => x.TypeId == typeId);
        }
        
        public static IEnumerable<BaseTile> GetAllTile(this Dictionary<Vector2Int, BaseTile> tiles, int typeId)
        {
            foreach (var simpleTile in tiles)
            {
                if(simpleTile.Value.TypeId == typeId)
                    yield return simpleTile.Value;
            }
        }
        
        /*public static bool AnyAffordableClaimableTiles(this Dictionary<Vector2Int, SimpleTileBase> tiles, int remainingMoves)
        {
            return tiles.Values.Any(x => x.State == TILE_STATE.VISIBLE && x.IsAdjacentToClaimed(tiles) && (x.Cost <= remainingMoves));
        }*/
    }
}