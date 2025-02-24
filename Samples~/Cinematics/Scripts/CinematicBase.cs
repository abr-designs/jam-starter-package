using System.Collections;
using Cameras;
using Unity.Cinemachine;
using UnityEngine;

namespace Cinematics
{
    public abstract class CinematicBase : MonoBehaviour
    {
        public string cinematicName;

        public abstract bool IsPlaying { get; protected set; }

        protected virtual bool KeepCamera { get; } = false;

        [SerializeField] protected CinemachineCamera targetCamera;

        protected static Transform playerTransform;


        // Start is called before the first frame update
        private void Start()
        {
            if (playerTransform == null)
                GameObject.FindWithTag("Player");
        }

        public Coroutine StartCinematic()
        {
            return StartCoroutine(PlayCoroutine());
        }

        private IEnumerator PlayCoroutine()
        {
            targetCamera.Priority = 10000;
            yield return StartCoroutine(PlayCinematicCoroutine());
            
            if(KeepCamera == false)
            {
                targetCamera.Priority = 0;
                yield return new WaitForSeconds(CameraManager.CameraBlendTime);
            }
            Destroy(gameObject);
        }

        protected abstract IEnumerator PlayCinematicCoroutine();

        protected void ForceSetCameraPosition(CinemachineCamera camera)
        {
            var brain = Camera.main.GetComponent<CinemachineBrain>();
            brain.enabled = false;

            var newCameraTransform = camera.transform;

            Camera.main.transform.position = newCameraTransform.position;
            Camera.main.transform.rotation = newCameraTransform.rotation;

            brain.enabled = true;
        }
    }
}