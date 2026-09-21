using UnityEngine;

namespace Tiles
{
    public class SquareTile2D : BaseTile<Vector2Int>
    {
        public override Vector2Int Position { get; set; }

        protected override void OnClickDown()
        {
            Debug.Log("OnClickDown");
        }

        protected override void OnClickUp()
        {
            Debug.Log("OnClickUp");
        }
       
    }
}