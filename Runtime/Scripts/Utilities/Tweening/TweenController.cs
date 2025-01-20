using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace Utilities.Tweening
{
    internal class TweenController : HiddenSingleton<TweenController>
    {
        private const int MAX_EMPTY_COUNT = 10;
        
        static readonly ProfilerMarker s_UpdatePerfMarker = new ProfilerMarker("TweenController.Update");
        static readonly ProfilerMarker s_GetTweenDataPerfMarker = new ProfilerMarker("TweenController.GetTweenData");
        
        private Dictionary<int, TweenData> _tweenDataDict;
        private Stack<TweenData> _nullTransformTweenDatas;
        //TODO Ideally, we'd just have a trimmed list of elements that are not moving, reducing calls through _tweenDataDict
        //private List<TweenData> _activeTweens;

        private readonly int[] _tweensToCleans = new int[MAX_EMPTY_COUNT];
        

        internal TweenData GetTweenData(Transform targetTransform, TRANSFORM transformation)
        {
            s_GetTweenDataPerfMarker.Begin();
            var hash = HashCode.Combine(targetTransform, (int)transformation);
            
            _tweenDataDict ??= new Dictionary<int, TweenData>();
            _nullTransformTweenDatas ??= new Stack<TweenData>(); 


            if (_tweenDataDict.TryGetValue(hash, out var tweenData) == false)
            {
                if (_nullTransformTweenDatas.TryPeek(out tweenData) == false)
                {
                    tweenData = new TweenData
                    {
                        TargetTransform = targetTransform,
                        Transformation = transformation
                    };
                }

                _tweenDataDict.Add(hash, tweenData);
            }
            
            s_GetTweenDataPerfMarker.End();

            return tweenData;
            
        }

        private void Update()
        {
            s_UpdatePerfMarker.Begin();
            var deltaTime = Time.deltaTime;


            var toEmptyCount = 0;
            var activeTweens = _tweenDataDict.Values;
            foreach (var tween in activeTweens)
            {
                if(tween.Active == false)
                    continue;

                //If the target Transform was destroyed, we'll need to store the TweenData elsewhere
                if (tween.TargetTransform == null)
                {
                    //We queue the keys that will need to be cleaned up
                    if(toEmptyCount < MAX_EMPTY_COUNT)
                        _tweensToCleans[toEmptyCount++] = tween.CachedHash;

                    continue;
                }
                
                if (!tween.Update(deltaTime)) 
                    continue;

                tween.OnTweenComplete?.Invoke();
            }

            //Move any TweenDatas that were deleted into the empty stack
            for (int i = 0; i < toEmptyCount; i++)
            {
                var key = _tweensToCleans[i];
                _tweenDataDict.TryGetValue(key, out var tween);
                
                _nullTransformTweenDatas.Push(tween);
                _tweenDataDict.Remove(key);
            }

            s_UpdatePerfMarker.End();
        }
    }

    internal enum TRANSFORM
    {
        NONE = 0,
        MOVE = 1 << 1,
        ROTATE = 2 << 1,
        SCALE = 3 << 1,
    }
    internal sealed class TweenData
    {
        internal int CachedHash;
        internal Transform TargetTransform;

        private bool _localTransformation;
        internal TRANSFORM Transformation;

        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private Vector3 _startScale;


        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private Vector3 _targetScale;

        private float _totalTime;
        private float _time;
        private CURVE _curve;

        internal bool Active;

        internal Action OnTweenComplete;

        internal TweenData()
        {
            Active = true;
        }
        
        internal TweenData SetData(bool worldSpace, TRANSFORM transformation, Transform targetTransform, float time, CURVE curve, Action onTweenComplete)
        {
            //Is the requested tween in Local or World space?
            _localTransformation = !worldSpace;
            Transformation = transformation;
            
            TargetTransform = targetTransform;
            _time =_totalTime = time;
            _curve = curve;

            switch (Transformation)
            {
                case TRANSFORM.NONE:
                    break;
                case TRANSFORM.MOVE:
                    _startPosition = _localTransformation ? TargetTransform.localPosition : targetTransform.position;
                    break;
                case TRANSFORM.ROTATE:
                    _startRotation = _localTransformation ? TargetTransform.localRotation : targetTransform.rotation;
                    break;
                case TRANSFORM.SCALE:
                    _startScale = targetTransform.localScale;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            OnTweenComplete = null;
            OnTweenComplete = onTweenComplete;

            Active = true;
            
            CachedHash = HashCode.Combine(targetTransform, (int)transformation);
            return this;
        }

        internal void SetTargetPosition(Vector3 targetPosition) => _targetPosition = targetPosition;
        internal void SetTargetRotation(Quaternion targetRotation) => _targetRotation = targetRotation;
        internal void SetTargetScale(Vector3 targetScale) => _targetScale = targetScale;

        internal bool Update(float deltaTime)
        {
            //We want to countdown the time to the target
            _time = Math.Clamp(_time - deltaTime, 0f, 1f);

            //Because we're counting down, we'll need to invert then normalize the value to get the curve.T
            var dt = GetCurveT(_curve, 1f - (_time / _totalTime));

            switch (Transformation)
            {
                case TRANSFORM.MOVE:
                {
                    if (_localTransformation)
                        TargetTransform.localPosition = Vector3.Lerp(_startPosition, _targetPosition, dt);
                    else
                        TargetTransform.position = Vector3.Lerp(_startPosition, _targetPosition, dt);

                    break;
                }
                case TRANSFORM.ROTATE:
                {
                    if (_localTransformation)
                        TargetTransform.localRotation = Quaternion.Lerp(_startRotation, _targetRotation, dt);
                    else
                        TargetTransform.rotation = Quaternion.Lerp(_startRotation, _targetRotation, dt);
                    break;
                }
                case TRANSFORM.SCALE:
                {
                    TargetTransform.localScale = Vector3.Lerp(_startScale, _targetScale, dt);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_time <= 0f)
            {
                Active = false;
                return true;
            }

            return false;
        }
        
        private static float GetCurveT(CURVE curve, float t)
        {
            t = Math.Clamp(t, 0, 1);
            switch (curve)
            {
                case CURVE.LINEAR:
                    return t;
                case CURVE.EASE_IN:
                    return LerpFunctions.Coserp(0f, 1f, t);
                case CURVE.EASE_OUT:
                    return LerpFunctions.Sinerp(0f, 1f, t);
                case CURVE.EASE_IN_OUT:
                    return LerpFunctions.Hermite(0f, 1f, t);
                default:
                    throw new ArgumentOutOfRangeException(nameof(curve), curve, null);
            }
        }

        public override int GetHashCode() => CachedHash;
    }
}