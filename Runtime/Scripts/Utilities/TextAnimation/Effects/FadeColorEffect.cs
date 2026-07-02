// Created by Claude (claude-opus-4-8)
// Date: 2026-07-01

using UnityEngine;
using UnityEngine.Scripting;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Oscillates alpha over time so the span breathes, leaving RGB untouched. The alpha multiplies the
    /// character's original alpha, so it fades the existing color rather than recoloring it.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-07-01</remarks>
    [Preserve]
    [TextEffect("fade")]
    public class FadeColorEffect : ColorTextEffect
    {
        protected float m_speed = 3f;
        protected float m_minAlpha = 0.15f;

        // Inline args: fade(speed, minAlpha).
        public override void Apply(ref CharMod mod, int charIndex, int spanLength, float time, in EffectArgs args)
        {
            var speed = args.GetFloat(0, m_speed);
            var minAlpha = args.GetFloat(1, m_minAlpha);

            var wave = 0.5f + 0.5f * Mathf.Sin(time * speed);
            var alpha = Mathf.Lerp(minAlpha, 1f, wave);
            mod.Color = new Color(1f, 1f, 1f, alpha);
        }
    }
}
