using UnityEngine;

namespace Samples.SpriteColorChange
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteColorChanger : MonoBehaviour
    {
        [SerializeField]
        private Color32 flashColor = new Color32(255,255,255,0);
        [SerializeField, Min(0)]
        private float speed;

        private float _t;

        private SpriteRenderer _spriteRenderer;

        
        // Start is called before the first frame update
        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.color = flashColor;

        }

        // Update is called once per frame
        private void Update()
        {
            _t = Mathf.PingPong(Time.time * speed, 1f);

            flashColor.a = (byte)Mathf.Lerp(0, 255, _t);
            _spriteRenderer.color = flashColor;
        }
    }
}
