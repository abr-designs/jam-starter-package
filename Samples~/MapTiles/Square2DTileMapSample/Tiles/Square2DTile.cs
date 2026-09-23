using UnityEngine;

namespace Tiles
{
    public class Square2DTile : BaseTile<Vector2Int>
    {
        public override Vector2Int Position { get; set; }

        protected override void OnInitialized()
        {
            Debug.Log("OnInitialized", gameObject);
        }

        protected override void OnClickDown()
        {
            Debug.Log("OnClickDown", gameObject);
        }

        protected override void OnClickUp()
        {
            Debug.Log("OnClickUp", gameObject);
        }

        protected override void OnHovered()
        {
            Debug.Log("OnHovered", gameObject);
        }
    }
}