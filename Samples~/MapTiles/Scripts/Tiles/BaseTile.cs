using MapGeneration;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Samples.MapTiles.Tiles
{
    public abstract class BaseTile<TPos> : BaseTile
    {
        public abstract TPos Position { get; set; }
        
        public void Init(TileTypeDefinition tileTypeDefinition, TPos position)
        {
            Position = position;
            base.Init(tileTypeDefinition);
        }
    }
    
    [RequireComponent(typeof(Collider))]
    public abstract class BaseTile : MonoBehaviour
    {
        public static event Action<BaseTile> OnTileClicked;
        public static event Action<BaseTile> OnTileHover;

        private static BaseTile s_pressedTile;
        protected static Transform MainCameraTransform;

        [SerializeField]
        private MeshRenderer[] extraObjects;
        [SerializeField]
        private Collider collider;
        
        public int TileID => TileTypeDefinition.Id;
        public TileTypeDefinition TileTypeDefinition { get; set; }

        //BaseTile Functions
        //================================================================================================================//
        
        public void Init(TileTypeDefinition tileTypeDefinition)
        {
            if (MainCameraTransform == null)
                MainCameraTransform = FindAnyObjectByType<Camera>().transform;

            TileTypeDefinition = tileTypeDefinition;

            OnInitialized();
        }

        private void SetIsInteractable(bool interactable)
        {
            collider.enabled = interactable;
        }


        private void OnMouseDown()
        {
            s_pressedTile = this;
            OnClickDown();
        }

        private void OnMouseUp()
        {
            bool shouldClick = s_pressedTile == this 
                                //&& !CameraController.SuppressLastClick 
                                && !EventSystem.current.IsPointerOverGameObject();

            if(shouldClick)
            {
                OnTileClicked?.Invoke(this);
            } 
            else if (s_pressedTile == this)
            {
                //SimpleAnimator.AnimateUp();
            }

            if(s_pressedTile == this)
                s_pressedTile = null;
            
            OnClickUp();
        }

        private void OnMouseOver()
        {
            OnTileHover?.Invoke(this);
        }

        //================================================================================================================//

        protected virtual void OnInitialized(){ }
        
        protected virtual void OnClickDown(){ }
        protected virtual void OnClickUp(){ }
        protected virtual void OnHovered(){ }

        //================================================================================================================//

    }
}
