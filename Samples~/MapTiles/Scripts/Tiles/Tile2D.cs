using UnityEngine;

namespace Tiles
{
    public class Tile2D : BaseTile<Vector2Int, float>
    {
        public override Vector2Int Position { get; set; }
        
        public override float Size => 2f;

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