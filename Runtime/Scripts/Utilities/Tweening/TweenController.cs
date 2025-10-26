using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    internal enum UPDATE_TYPE
    {
        UPDATE,
        COROUTINE,
        ASYNC
    }
    
    internal sealed class TweenData
    {
        internal UPDATE_TYPE UpdateType;
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
            if (targetTransform == null)
                throw new ArgumentOutOfRangeException(nameof(targetTransform), $"{nameof(targetTransform)} should not be null!");

            UpdateType = UPDATE_TYPE.UPDATE;
            
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

        internal TweenData SetTargetPosition(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
            return this;
        }
        internal TweenData SetTargetRotation(Quaternion targetRotation)
        {
            _targetRotation = targetRotation;
            return this;
        }
        internal TweenData SetTargetScale(Vector3 targetScale)
        {
            _targetScale = targetScale;
            return this;
        }

        internal IEnumerator AsCoroutine()
        {
            UpdateType = UPDATE_TYPE.COROUTINE;

            while (Active)
                yield return null;
        }
        
        internal async Task AsAsncTask() => throw new NotImplementedException();

        internal bool Update(float deltaTime)
        {
            //This circumstance should be avoided when possible, as it should just set the transformation
            if (_totalTime <= 0f)
            {
                InstantTween();
                Active = false;
                return true;
            }
            
            //We want to countdown the time to the target
            _time = Math.Clamp(_time - deltaTime, 0f, _totalTime);

            //Because we're counting down, we'll need to invert then normalize the value to get the curve.T
            var normalizedTime = (_time / _totalTime);
            var dt = GetCurveT(_curve, 1f - normalizedTime);

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

        /// <summary>
        /// In the event that the tween should be happening instantly, such as when the total tween time is zero, we do
        /// so here. Returning True to indicate that the tween is completed.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void InstantTween()
        {
            Debug.LogError($"Attempting to apply {Transformation} to {TargetTransform.gameObject.name} with a time of {_totalTime:#0.0s}.\n" +
                           "Tween's should only be used for movement over time, to instantly apply a tween, just set the transform.");
            switch (Transformation)
            {
                case TRANSFORM.MOVE:
                {
                    if (_localTransformation)
                        TargetTransform.localPosition = _targetPosition;
                    else
                        TargetTransform.position = _targetPosition;

                    break;
                }
                case TRANSFORM.ROTATE:
                {
                    if (_localTransformation)
                        TargetTransform.localRotation = _targetRotation;
                    else
                        TargetTransform.rotation = _targetRotation;
                    break;
                }
                case TRANSFORM.SCALE:
                {
                    TargetTransform.localScale = _targetScale;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
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