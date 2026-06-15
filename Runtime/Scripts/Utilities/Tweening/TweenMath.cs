// Created by Claude (claude-opus-4-8)
// Date: 2026-06-14

using System;

namespace Utilities.Tweening
{
    /// <summary>
    /// Curve evaluation shared by the sync and async tween engines so the math cannot drift between them.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) - 2026-06-14</remarks>
    internal static class TweenMath
    {
        internal static float GetCurveT(CURVE curve, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
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
    }
}
