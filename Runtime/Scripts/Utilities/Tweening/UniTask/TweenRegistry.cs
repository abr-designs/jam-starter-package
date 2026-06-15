// Created by Claude (claude-opus-4-8)
// Date: 2026-06-14

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Utilities.Tweening
{
    /// <summary>
    /// Tracks the active async tween per <c>(Transform, TRANSFORM)</c> key so a newly started tween preempts the
    /// prior one. Mirrors the conceptual role of <see cref="TweenController"/>'s tween dictionary on the async side,
    /// kept separate because core holds no reference to the gated asmdef.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) - 2026-06-14</remarks>
    internal static class TweenRegistry
    {
        private static readonly Dictionary<int, CancellationTokenSource> s_activeTweens = new();

        /// <summary>
        /// Cancels any tween already running on the key, then registers a linked source so both caller cancellation
        /// (<paramref name="externalToken"/>) and preemption by a newer tween flow through one token.
        /// </summary>
        internal static CancellationTokenSource Register(Transform targetTransform, TRANSFORM transformation, CancellationToken externalToken)
        {
            var hash = HashCode.Combine(targetTransform, (int)transformation);

            //Cancel the prior tween; its own finally block disposes it via Release.
            if (s_activeTweens.TryGetValue(hash, out var existing))
                existing.Cancel();

            var linked = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            s_activeTweens[hash] = linked;
            return linked;
        }

        /// <summary>
        /// Clears the registry entry if it still points at <paramref name="cts"/> (a newer tween may have replaced it),
        /// then disposes the source.
        /// </summary>
        internal static void Release(Transform targetTransform, TRANSFORM transformation, CancellationTokenSource cts)
        {
            var hash = HashCode.Combine(targetTransform, (int)transformation);

            if (s_activeTweens.TryGetValue(hash, out var current) && ReferenceEquals(current, cts))
                s_activeTweens.Remove(hash);

            cts.Dispose();
        }
    }
}
