using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utilities
{
    public class MouseCaster : MonoBehaviour
    {
        public GameObject HitObject 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get; 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set; 
        }

        [SerializeField]
        private new Camera camera;
        [SerializeField]
        private LayerMask mask;

        [SerializeField, Min(0f)]
        private float castDistance;

        //Unity Functions
        //============================================================================================================//
        
        private void Start()
        {
            Assert.IsNotNull(camera, "Camera is Null");
        }

        private void Update()
        {
            var cameraRay = GetCameraRay();
            DrawRay(cameraRay, castDistance, Color.red);

            if (UnityEngine.Physics.Raycast(cameraRay, out var raycastHit, castDistance, mask.value) == false)
            {
                HitObject = null;
                return;
            }
            
            DrawRay(cameraRay, raycastHit.distance, Color.green);

            //TODO May want to include a callback when the object changes!
            if (HitObject == raycastHit.transform.gameObject)
                return;

            HitObject = raycastHit.transform.gameObject;
        }

        //MouseCaster Functions
        //============================================================================================================//
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Ray GetCameraRay()
        {
            return camera.ScreenPointToRay(Input.mousePosition);
        }

        //Unity Editor Functions
        //============================================================================================================//
        
        [Conditional("UNITY_EDITOR")]
        private static void DrawRay(Ray ray, float dist, Color color)
        {
            UnityEngine.Debug.DrawRay(ray.origin, ray.direction * dist, color);
        }
        
    }
}