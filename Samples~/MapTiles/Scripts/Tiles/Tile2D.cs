using UnityEngine;

namespace Tiles
{
    public class Tile2D : BaseTile<Vector2Int, int>
    {
        public override Vector2Int Position { get; set; }
        
        public override int Size => 2;

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