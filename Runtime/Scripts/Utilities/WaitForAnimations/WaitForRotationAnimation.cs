using UnityEngine;
using Utilities.WaitForAnimations.Base;

namespace Utilities.WaitForAnimations
{
    public class WaitForRotationAnimation : WaitForAnimationBase<Transform, Vector3>
    {
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
            var rotation = Quaternion.Euler(value);
            switch (data.transformSpace)
            {
                case SPACE.WORLD:
                    data.transform.rotation = rotation;
                    break;
                case SPACE.LOCAL:
                    data.transform.localRotation = rotation;
                    break;
            }
            
        }
    }
}