using Audio;
using MapGeneration;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VisualFX;

namespace Tiles
{
    public abstract class BaseTile<T, TU> : BaseTile
    {
        public abstract T Position { get; set; }
        
        public abstract TU Size { get; }
        
        public void Init(/*TILE_STATE state, */TileData tileData, T position)
        {
            if (MainCameraTransform == null)
                MainCameraTransform = FindAnyObjectByType<Camera>().transform;

            TileData = tileData;
            Position = position;
            /*SetState(state);*/
            //UpdateCostText();
        }
    }
    
    [RequireComponent(typeof(Collider))]
    public abstract class BaseTile : MonoBehaviour
    {
        public static event Action<BaseTile> OnTileClicked;
        public static event Action<BaseTile> OnRevealed;
        public static event Action<BaseTile> OnTileHover;

        private static BaseTile s_pressedTile;

        protected static Transform MainCameraTransform;

        public int TypeId => TileData.id;
        //[field: SerializeField, ReadOnly]
        public TileData TileData { get; set; } = null;
        
        //[field: SerializeField, ReadOnly]
        

        /*public int Cost => Mathf.Clamp(TileData.cost + CostModifier, TileData.costMinMax.x, TileData.costMinMax.y);
        public int CostModifier { get; set; } = 0;
        public int BaseCost => TileData.cost;*/

        /*private bool hasBeenRevealed;
        public bool CanBeClaimed { get; private set; }*/

        public event Action OnClaimed;


        /*[SerializeField]
        private TMP_Text costText;
        [SerializeField]
        private TMP_Text costTextBackground;
        private Vector3 costTextBaseScale = Vector3.zero;*/

        /*[SerializeField]
        private ParticleSystem cloudParticle;
        [SerializeField]
        private MeshRenderer cloudPlane;*/
        [SerializeField]
        private MeshRenderer[] extraObjects;
        [SerializeField]
        private Collider collider;

        /*[Header("Audio")]
        [SerializeField]
        private SFX captureSFX;
        [SerializeField]
        private SFX revealSFX;
        [SerializeField]
        private VFX revealVFX;
        public bool HasRevealSFX => revealSFX != SFX.NONE;*/

        //SimpleTile Functions
        //================================================================================================================//



        /*public void SetState(TILE_STATE state, bool canBeClaimed = false)
        {
            CanBeClaimed = canBeClaimed;
            State = state;
            switch (state)
            {
                case TILE_STATE.HIDDEN:
                    // SetInteractable(false);
                    SetVisible(false);
                    SetRevealed(false);
                    break;
                case TILE_STATE.VISIBLE:
                    SetInteractable(true);
                    SetVisible(true);
                    SetRevealed(true);
                    PlayRevealEffects();
                    break;
                case TILE_STATE.CLAIMED:
                    Claim();
                    SetInteractable(true);
                    SetVisible(true);
                    SetRevealed(true);

                    // metrics
                    //GameManager.IncreaseTilesClaimed();
                    OnClaimed?.Invoke();

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }*/

        /*public void PlayCaptureSFX()
        {
            if (captureSFX != SFX.NONE)
                SFXManager.PlaySound(captureSFX);
        }

        public void PlayRevealEffects()
        {
            if (revealSFX != SFX.NONE)
                SFXManager.PlaySound(revealSFX);

            revealVFX.PlayAtLocation(transform.position);
        }*/

        //TODO Setup Enqueued Modifiers
        /*public void ApplyCostModifier(int costModifier)
        {
            CostModifier += costModifier;

            if (State == TILE_STATE.CLAIMED || State == TILE_STATE.HIDDEN)
                return;

            //TODO Update Visual
            UpdateCostText();
        }*/

        /*private void Claim()
        {
            CanBeClaimed = false;
            if (costText != null)
                costText.text = "";

            //TODO Apply some sort of border
            if (costTextBackground != null)
                costTextBackground.text = "";

            //TODO Update Visual
        }*/

        /*private void UpdateCostText()
        {
            if (costText == null)
                return;

            if (costTextBaseScale == Vector3.zero)
                costTextBaseScale = costText.transform.localScale;

            costText.text = $"{(Cost > 0 ? "-" : "+")}{Math.Abs(Cost)}";
            costText.transform.localScale = costTextBaseScale * (costText.text.Length > 2 ? 0.75f : 1f);

            if (costTextBackground == null)
                return;

            if (costTextBaseScale == Vector3.zero)
                costTextBaseScale = costTextBackground.transform.localScale;

            costTextBackground.text = "█";
            if(Cost <= 5)
                costTextBackground.color = Cost > 0 ? new Color32(242, 207, 157, 255) : new Color32(143, 189, 163, 255);
            if (Cost > 5)
                costTextBackground.color = new Color32(209, 130, 147, 255);
            costTextBackground.transform.localScale = costTextBaseScale;// * (costTextBackground.text.Length > 2 ? 0.75f : 1f);
            float costBackgroundWidthMod = 2.25f;
            costTextBackground.transform.localScale = new Vector3(costTextBackground.transform.localScale.x * costBackgroundWidthMod, costTextBackground.transform.localScale.y, costTextBackground.transform.localScale.z);
        }

        private void SetVisible(bool visible)
        {
            cloudPlane.enabled = !visible;

            // Update map
            CloudPlane.UpdateMap(Position, visible ? (byte)255 : (byte)0);

            for (int i = 0; i < extraObjects.Length; i++)
            {
                extraObjects[i].enabled = visible;
            }

            if (visible && CanBeClaimed)
                UpdateCostText();
            if (visible)
                cloudParticle.Play();

        }

        private void SetRevealed(bool revealed)
        {
            if (!revealed)
            {
                hasBeenRevealed = revealed;
                return;
            }

            if (hasBeenRevealed)
                return;

            hasBeenRevealed = revealed;

            OnRevealed?.Invoke(this);
        }*/

        private void SetInteractable(bool interactable)
        {
            collider.enabled = interactable;
        }


        private void OnMouseDown()
        {
            /*if(!CanBeClaimed)
                return;*/
            
            s_pressedTile = this;
            // Play start of capture, user can still cancel by moving off
            //SimpleAnimator.AnimateDown();
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

        protected abstract void OnClickDown();
        protected abstract void OnClickUp();

        //Callbacks
        //================================================================================================================//
        /*private void OnGameOver(GameManager.GAME_STATE state)
        {
            if (costText != null)
                costText.enabled = false;

            if (costTextBackground != null)
                costTextBackground.enabled = false;

            SetInteractable(false);
        }

        private void OnGestureStarted()
        {
            if (s_pressedTileBase != this)
                return;

            SimpleAnimator.AnimateUp();
            s_pressedTileBase = null;
        }*/

    }
}
