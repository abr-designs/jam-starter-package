using System;
using System.Collections;
using UnityEngine;
using Utilities.Enums;
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


        #region Sync Calls

        #region Transform Move

        public static void TweenTo(this Transform transform, SPACE transformSpace, Vector3 targetPosition, float time, CURVE curve = CURVE.LINEAR, Action onCompleted = null)
        {
            TweenController.GetTweenData(transform, TRANSFORM.MOVE)
                .SetData(transformSpace, TRANSFORM.MOVE, transform, time, curve, onCompleted)
                .SetTargetPosition(targetPosition);

        }
        
        public static IEnumerator TweenToCoroutine(this Transform transform, SPACE transformSpace, Vector3 targetPosition, float time, CURVE curve = CURVE.LINEAR, Action onCompleted = null)
        {
            yield return TweenController.GetTweenData(transform, TRANSFORM.MOVE)
                .SetData(transformSpace, TRANSFORM.MOVE, transform, time, curve, null)
                .SetTargetPosition(targetPosition)
                .AsCoroutine();
            
            //We delay the callback to when the user would expect, at the end of the Coroutine
            onCompleted?.Invoke();
        }

        #endregion //Transform Move
        
        #region Transform Rotate

        public static void TweenTo(this Transform transform, SPACE transformSpace, Quaternion targetRotation, float time, CURVE curve = CURVE.LINEAR, Action onCompleted = null)
        {
            TweenController.GetTweenData(transform, TRANSFORM.ROTATE)
                .SetData(transformSpace, TRANSFORM.ROTATE, transform, time, curve, onCompleted)
                .SetTargetRotation(targetRotation);
        }
        
        public static IEnumerator TweenToCoroutine(this Transform transform, SPACE transformSpace, Quaternion targetRotation, float time, CURVE curve = CURVE.LINEAR, Action onCompleted = null)
        {
            yield return TweenController.GetTweenData(transform, TRANSFORM.MOVE)
                .SetData(transformSpace, TRANSFORM.MOVE, transform, time, curve, null)
                .SetTargetRotation(targetRotation)
                .AsCoroutine();
            
            //We delay the callback to when the user would expect, at the end of the Coroutine
            onCompleted?.Invoke();
        }

        #endregion //Transform Rotate
        
        #region Transform Scale

        public static void TweenScaleTo(this Transform transform, Vector3 targetScale, float time, CURVE curve = CURVE.LINEAR, Action onCompleted = null)
        {
            TweenController.GetTweenData(transform, TRANSFORM.SCALE)
                .SetData(SPACE.LOCAL, TRANSFORM.SCALE, transform, time, curve, onCompleted)
                .SetTargetScale(targetScale);
        }
        
        public static IEnumerator TweenScaleToCoroutine(this Transform transform, Vector3 targetScale, float time, CURVE curve = CURVE.LINEAR, Action onCompleted = null)
        {
            yield return TweenController.GetTweenData(transform, TRANSFORM.SCALE)
                .SetData(SPACE.LOCAL, TRANSFORM.SCALE, transform, time, curve, null)
                .SetTargetScale(targetScale)
                .AsCoroutine();
            
            //We delay the callback to when the user would expect, at the end of the Coroutine
            onCompleted?.Invoke();
        }

        #endregion //Transform Rotate

        #endregion //Synchronous Calls

       
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