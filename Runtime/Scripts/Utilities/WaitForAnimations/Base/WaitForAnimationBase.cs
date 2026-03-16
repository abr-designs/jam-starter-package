using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utilities.WaitForAnimations.Base
{
    public abstract class WaitForAnimationBase : MonoBehaviour, IWaitForAnimation
    {
        /// <summary>
        /// This function can be called from UnityEvents easily to animate
        /// </summary>
        public abstract void Animate();
        //FIXME Only do the animation if we're not already at then intended destination OR Start!!
        public abstract Coroutine DoAnimation(float time, ANIM_DIR animDir);
    }
    /// <summary>
    /// Animation script that returns a Coroutine, to be waited upon in another Coroutine , use <see cref="WaitForAnimationBase"/> in the Unity Inspector
    /// </summary>
    public abstract class WaitForAnimationBase<TR, T> : WaitForAnimationBase where TR: Transform
    {
        [Serializable]
        protected class AnimationData
        {
            public TR transform;
            public SPACE transformSpace = SPACE.WORLD;
            public T start;
            public T end;
        }
        
        //============================================================================================================//

        [SerializeField, Range(0f,1f)]
        protected float startingValue;
        
        [SerializeField] 
        protected AnimationCurve curve;
        
        [SerializeField, Space(10f)]
        protected AnimationData[] objectsToAnimate;

        [SerializeField, Min(0f), Header("Simple-Call Fields")]
        private float defaultAnimationTime;
        private ANIM_DIR m_lastAnimationDirection;

        //Unity Functions
        //============================================================================================================//
        
        protected void Start()
        {
            Assert.IsNotNull(curve);
            Assert.IsNotNull(objectsToAnimate);
            
            for (int i = 0; i < objectsToAnimate.Length; i++)
            {
                var toAnimate = objectsToAnimate[i];
                SetValue(toAnimate, Lerp(toAnimate.start, toAnimate.end, startingValue));
            }
        }

        //============================================================================================================//

        protected abstract T Lerp(T start, T end, float t);

        protected abstract void SetValue(AnimationData data, T value);

        //============================================================================================================//
        
        public override void Animate()
        {
            var dir = m_lastAnimationDirection == ANIM_DIR.START_TO_END ? ANIM_DIR.END_TO_START : ANIM_DIR.START_TO_END;
            
            DoAnimation(defaultAnimationTime, dir);
        }

        //================================================================================================================//
        
        protected IEnumerator DoAnimationCoroutine(float time, ANIM_DIR animDir)
        {
            m_lastAnimationDirection = animDir;
            
            for (int i = 0; i < objectsToAnimate.Length; i++)
            {
                var moveData = objectsToAnimate[i];
                var startPosition = animDir == ANIM_DIR.START_TO_END ? moveData.start : moveData.end;
                var endPosition = animDir == ANIM_DIR.START_TO_END ? moveData.end : moveData.start;

                StartCoroutine(AnimateCoroutine(moveData, startPosition, endPosition, time));
            }

            yield return new WaitForSeconds(time);
        }
        
        private IEnumerator AnimateCoroutine(AnimationData target, T start, T end, float time)
        {
            for (var t = 0f; t <= time; t += Time.deltaTime)
            {
                var dt = curve.Evaluate(t / time);

                SetValue(target, Lerp(start, end, dt));
                    
                yield return null;
            }
            
            SetValue(target, end);
        }
        
        //============================================================================================================//
    }
}