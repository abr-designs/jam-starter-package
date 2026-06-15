// Created by Claude (claude-opus-4-8)
// Date: 2026-06-14

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utilities.Enums;

namespace Utilities.Tweening
{
    /// <summary>
    /// Awaitable tween extensions backed by UniTask (https://github.com/Cysharp/UniTask). Each call is its own async
    /// state machine; there is no central controller on the async path. Cancellation flows through the token passed to
    /// <c>UniTask.Yield</c>, conflict prevention through <see cref="TweenRegistry"/>, and curve evaluation is shared
    /// with the sync engine via <see cref="TweenMath"/>.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) - 2026-06-14</remarks>
    public static class TransformTweenExtensionsAsync
    {
        /// <summary>
        /// Tweens position to <paramref name="targetPosition"/> over <paramref name="duration"/> seconds, awaitable.
        /// Cancelling <paramref name="cancellationToken"/> throws <see cref="System.OperationCanceledException"/> and
        /// freezes the transform at its current value.
        /// </summary>
        public static async UniTask TweenToAsync(this Transform transform, 
            SPACE transformSpace, 
            Vector3 targetPosition,
            float duration, 
            CURVE curve = CURVE.LINEAR, 
            PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default)
        {
            if (transform == null)
                return;

#if DEBUG
            Debug.Assert(!TweenController.HasActiveTween(transform, TRANSFORM.MOVE),
                $"[Tweening] TweenToAsync started on '{transform.name}' while a sync tween for MOVE is active. " +
                "Mixing sync & async tweens on the same transform+transformation is undefined behaviour.");
#endif

            var cts = TweenRegistry.Register(transform, TRANSFORM.MOVE, cancellationToken);
            var token = cts.Token;
            try
            {
                var start = transformSpace == SPACE.LOCAL ? transform.localPosition : transform.position;
                var elapsed = 0f;
                while (elapsed < duration)
                {
                    // TODO(unscaled-time): expose ignoreTimeScale if pause-aware tweens become a need. See issue #124.
                    await UniTask.Yield(timing, token);
                    if (transform == null)
                        return;

                    elapsed += Time.deltaTime;
                    var t = TweenMath.GetCurveT(curve, Mathf.Clamp01(elapsed / duration));
                    var position = Vector3.Lerp(start, targetPosition, t);
                    if (transformSpace == SPACE.LOCAL)
                        transform.localPosition = position;
                    else
                        transform.position = position;
                }

                if (transform != null)
                {
                    if (transformSpace == SPACE.LOCAL)
                        transform.localPosition = targetPosition;
                    else
                        transform.position = targetPosition;
                }
            }
            finally
            {
                TweenRegistry.Release(transform, TRANSFORM.MOVE, cts);
            }
        }

        /// <summary>
        /// Tweens rotation to <paramref name="targetRotation"/> over <paramref name="duration"/> seconds, awaitable.
        /// Cancelling <paramref name="cancellationToken"/> throws <see cref="System.OperationCanceledException"/> and
        /// freezes the transform at its current value.
        /// </summary>
        public static async UniTask TweenToAsync(this Transform transform, 
            SPACE transformSpace, 
            Quaternion targetRotation,
            float duration, 
            CURVE curve = CURVE.LINEAR, 
            PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default)
        {
            if (transform == null)
                return;

#if DEBUG
            Debug.Assert(!TweenController.HasActiveTween(transform, TRANSFORM.ROTATE),
                $"[Tweening] TweenToAsync started on '{transform.name}' while a sync tween for ROTATE is active. " +
                "Mixing sync & async tweens on the same transform+transformation is undefined behaviour.");
#endif

            var cts = TweenRegistry.Register(transform, TRANSFORM.ROTATE, cancellationToken);
            var token = cts.Token;
            try
            {
                var start = transformSpace == SPACE.LOCAL ? transform.localRotation : transform.rotation;
                var elapsed = 0f;
                while (elapsed < duration)
                {
                    // TODO(unscaled-time): expose ignoreTimeScale if pause-aware tweens become a need. See issue #124.
                    await UniTask.Yield(timing, token);
                    if (transform == null)
                        return;

                    elapsed += Time.deltaTime;
                    var t = TweenMath.GetCurveT(curve, Mathf.Clamp01(elapsed / duration));
                    var rotation = Quaternion.Lerp(start, targetRotation, t);
                    if (transformSpace == SPACE.LOCAL)
                        transform.localRotation = rotation;
                    else
                        transform.rotation = rotation;
                }

                if (transform != null)
                {
                    if (transformSpace == SPACE.LOCAL)
                        transform.localRotation = targetRotation;
                    else
                        transform.rotation = targetRotation;
                }
            }
            finally
            {
                TweenRegistry.Release(transform, TRANSFORM.ROTATE, cts);
            }
        }

        /// <summary>
        /// Tweens local scale to <paramref name="targetScale"/> over <paramref name="duration"/> seconds, awaitable.
        /// Cancelling <paramref name="cancellationToken"/> throws <see cref="System.OperationCanceledException"/> and
        /// freezes the transform at its current value.
        /// </summary>
        public static async UniTask TweenScaleToAsync(this Transform transform, 
            Vector3 targetScale,
            float duration, 
            CURVE curve = CURVE.LINEAR, 
            PlayerLoopTiming timing = PlayerLoopTiming.Update,
            CancellationToken cancellationToken = default)
        {
            if (transform == null)
                return;

#if DEBUG
            Debug.Assert(!TweenController.HasActiveTween(transform, TRANSFORM.SCALE),
                $"[Tweening] TweenScaleToAsync started on '{transform.name}' while a sync tween for SCALE is active. " +
                "Mixing sync & async tweens on the same transform+transformation is undefined behaviour.");
#endif

            var cts = TweenRegistry.Register(transform, TRANSFORM.SCALE, cancellationToken);
            var token = cts.Token;
            try
            {
                var start = transform.localScale;
                var elapsed = 0f;
                while (elapsed < duration)
                {
                    // TODO(unscaled-time): expose ignoreTimeScale if pause-aware tweens become a need. See issue #124.
                    await UniTask.Yield(timing, token);
                    if (transform == null)
                        return;

                    elapsed += Time.deltaTime;
                    var t = TweenMath.GetCurveT(curve, Mathf.Clamp01(elapsed / duration));
                    transform.localScale = Vector3.Lerp(start, targetScale, t);
                }

                if (transform != null)
                    transform.localScale = targetScale;
            }
            finally
            {
                TweenRegistry.Release(transform, TRANSFORM.SCALE, cts);
            }
        }
    }
}
