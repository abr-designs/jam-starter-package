using System;
using Cinemachine;
using UnityEngine;
using Utilities;

namespace Cameras
{
    public enum CINEMATIC_CAMERA : int
    {
        DEFAULT, //Go to Default
        
    }
    public class CameraManager : HiddenSingleton<CameraManager>
    {
        public static float CameraBlendTime => Instance.cameraBlendTime;
        [SerializeField, Min(0)]
        private float cameraBlendTime;
        
        [SerializeField]
        private Camera camera;
        [SerializeField]
        private CinemachineBrain brain;
        [SerializeField]
        private CinemachineSmoothPath dollyTrack;

        [SerializeField]
        private CinemachineVirtualCamera[] virtualCameras;

        //============================================================================================================//


        //============================================================================================================//
        
        private void SetCamera(CINEMATIC_CAMERA cinematicCamera)
        {
            var index = (int)cinematicCamera;
            for (var i = 0; i < virtualCameras.Length; i++)
            {
                virtualCameras[i].enabled = index == i;
                virtualCameras[i].Priority = index == i ? 1000 : -1000;
            }
        }

        private void SetDefaultCameraTarget(Transform targetTransform)
        {
            var defaultCamera = virtualCameras[(int)CINEMATIC_CAMERA.DEFAULT];
            defaultCamera.Follow = targetTransform;
            defaultCamera.LookAt = targetTransform;
            
            if(targetTransform != null)
                Debug.Log($"Set Camera target to: {targetTransform.gameObject.name}", targetTransform.gameObject);
        }
        
        //============================================================================================================//

        public static void SetDefaultCameraTargets(Transform target)
        {
            Instance.SetDefaultCameraTarget(target);
        }

    }
}
