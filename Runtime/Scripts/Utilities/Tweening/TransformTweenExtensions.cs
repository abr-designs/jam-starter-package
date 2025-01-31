using System;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Utilities.Tweening
{
    public static class TransformTweenExtensions
    {
        private static bool _isSetup;

        private static TweenController TweenController
        {
            get
            {
                if (_isSetup)
                    return _tweenController;
                
                SetupTweenController();
                return _tweenController;
            }
        }
        private static TweenController _tweenController;


        #region Transform Move

        public static void TweenTo(this Transform transform, Vector3 targetWorldPosition, float time, CURVE curve = CURVE.LINEAR, Action onCompleted = null)
        {
            TweenController.GetTweenData(transform, TRANSFORM.MOVE)
                .SetData(true, TRANSFORM.MOVE, transform, time, curve, onCompleted)
                .SetTargetPosition(targetWorldPosition);

        }
        public static void TweenToLocal(this Transform transform, Vector3 targetLocalPosition, float time, CURVE curve = CURVE.LINEAR, Action onCompleted = null)
        {
            TweenController.GetTweenData(transform, TRANSFORM.MOVE)
                .SetData(false, TRANSFORM.MOVE, transform, time, curve, onCompleted)
                .SetTargetPosition(targetLocalPosition);
        }

        #endregion //Transform Move
        
        #region Transform Rotate

        public static void TweenTo(this Transform transform, Quaternion targetWorldRotation, float time, CURVE curve = CURVE.LINEAR, Action onCompleted = null)
        {
            TweenController.GetTweenData(transform, TRANSFORM.ROTATE)
                .SetData(true, TRANSFORM.ROTATE, transform, time, curve, onCompleted)
                .SetTargetRotation(targetWorldRotation);
        }
        public static void TweenToLocal(this Transform transform, Quaternion targetLocalRotation, float time, CURVE curve = CURVE.LINEAR, Action onCompleted = null)
        {
            TweenController.GetTweenData(transform, TRANSFORM.ROTATE)
                .SetData(false, TRANSFORM.ROTATE, transform, time, curve, onCompleted)
                .SetTargetRotation(targetLocalRotation);
        }

        #endregion //Transform Rotate
        
        #region Transform Scale

        public static void TweenScaleTo(this Transform transform, Vector3 targetScale, float time, CURVE curve = CURVE.LINEAR, Action onCompleted = null)
        {
            TweenController.GetTweenData(transform, TRANSFORM.SCALE)
                .SetData(false, TRANSFORM.SCALE, transform, time, curve, onCompleted)
                .SetTargetScale(targetScale);
        }

        #endregion //Transform Rotate

        //TransformTweenExtensions Setup Functions
        //============================================================================================================//
        
        private static void SetupTweenController()
        {
            if (!Application.isPlaying)
                return;
            
            if (_isSetup)
                throw new Exception();
            
            
            var newObject = new GameObject($"=== {nameof(TweenController).ToUpper()} ===", typeof(TweenController));
            Object.DontDestroyOnLoad(newObject);

            _tweenController = newObject.GetComponent<TweenController>();

            _isSetup = true;
        }
    }
}