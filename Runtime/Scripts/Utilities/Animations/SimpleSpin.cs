using UnityEngine;
using Utilities.Enums;
using Utilities.Extensions;

namespace Utilities.Animations
{
    public class SimpleSpin : MonoBehaviour
    {
        [SerializeField]
        private SPACE space = SPACE.WORLD;
        
        public bool reverse;
        [SerializeField]
        private Vector3 spin;

        // Update is called once per frame
        private void Update()
        {
            var currentRotation = transform.rotation;

            currentRotation *= Quaternion.Euler(spin * (Time.deltaTime * (reverse ? -1 : 1)));

            transform.SetRotation(space, currentRotation);
        }
    }
}
