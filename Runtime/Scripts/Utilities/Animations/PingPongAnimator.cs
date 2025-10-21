using System;
using UnityEngine;
using UnityEngine.Assertions;
using Utilities.Enums;
using Utilities.Extensions;

namespace Utilities.Animations
{
    /// <summary>
    /// Type of Simple animator that PingPongs the rotation & Scale of an object
    /// </summary>
    public class PingPongAnimator : MonoBehaviour
    {
        [SerializeField]
        private SPACE space = SPACE.WORLD;
        
        [SerializeField, Min(0f)]
        private float speed;
        private float _current;

        [SerializeField, Header("Rotation")] private bool useRotation;
        [SerializeField]
        private Vector3 startRotation, endRotation;
        [SerializeField, Header("Scale")] private bool useScale;
        [SerializeField]
        private Vector3 startScale, endScale;
        
        [SerializeField, Header("Position")] 
        private bool usePosition;
        [SerializeField]
        private Vector3 startPosition, endPosition;

        [SerializeField, Space(10f)]
        private AnimationCurve curve;

        private void Start()
        {
            Assert.IsNotNull(curve, $"{nameof(curve)} needs to be set!");
        }

        // Start is called before the first frame update// Update is called once per frame
        private void Update()
        {
            _current += Time.deltaTime * speed;
            var t = curve.Evaluate(Mathf.PingPong(_current, 1f));

            if(usePosition)
                transform.SetPosition(space, Vector3.Lerp(startPosition, endPosition, t));
            
            if(useRotation)
                transform.SetRotation(space,Quaternion.Euler(Vector3.Lerp(startRotation, endRotation, t)));
            
            if(useScale)
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
        }
    }
}
