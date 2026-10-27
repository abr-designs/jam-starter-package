using System.Collections.Generic;
using UnityEngine;

namespace Samples.MapTiles.Tiles
{
    public interface ITileLayout<TILE_POS_UNIT>
    {
        Vector3 ToWorld(TILE_POS_UNIT position);
        TILE_POS_UNIT FromWorld(Vector3 worldPosition);
        IEnumerable<TILE_POS_UNIT> Neighbours(TILE_POS_UNIT position);
        IEnumerable<TILE_POS_UNIT> CoordinatesInRadius(TILE_POS_UNIT center, int radius);
    }
}