using UnityEngine;
using Utilities.Debugging;
using Utilities.WaitForAnimations.Base;

namespace Utilities.WaitForAnimations
{
    /// <summary>
    /// Sets the position of a transform over time
    /// </summary>
    public class WaitForMoveAnimations : WaitForAnimationBase<Transform, Vector3>
    {

        //============================================================================================================//
        
        public override Coroutine DoAnimation(float time, ANIM_DIR animDir)
        {
            return StartCoroutine(DoAnimationCoroutine(time, animDir));
        }

        protected override Vector3 Lerp(Vector3 start, Vector3 end, float t)
        {
            return Vector3.Lerp(start, end, t);
        }

        protected override void SetValue(AnimationData data, Vector3 value)
        {
            switch (data.transformSpace)
            {
                case SPACE.WORLD:
                    data.transform.position = value;
                    break;
                case SPACE.LOCAL:
                    data.transform.localPosition = value;
                    break;
            }
            
        }

        //============================================================================================================//

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (objectsToAnimate == null || objectsToAnimate.Length == 0)
                return;
            
            for (int i = 0; i < objectsToAnimate.Length; i++)
            {
                var objectToAnimate = objectsToAnimate[i];
                

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(objectToAnimate.start, objectToAnimate.end);

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(objectToAnimate.start, 0.5f);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(objectToAnimate.end, 0.5f);
                
                if(objectToAnimate.transform == null)
                    continue;
                
                var midPoint = (objectToAnimate.start + objectToAnimate.end) / 2f;
                Draw.Label(midPoint, objectToAnimate.transform.name);
            }
        }
#endif
    }
}
