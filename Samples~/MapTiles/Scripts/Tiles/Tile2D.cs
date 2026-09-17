using UnityEngine;

namespace Tiles
{
    public class Tile2D : BaseTile<Vector2Int, int>
    {
        public override Vector2Int Position { get; set; }

        protected override void OnClickDown()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnClickUp()
        {
            throw new System.NotImplementedException();
        }
       
    }
}