// Created by Claude (claude-opus-4-8)
// Date: 2026-07-01

using UnityEngine;
using UnityEngine.Scripting;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Static two-color blend across the span: the first character takes <see cref="m_from"/>, the last
    /// takes <see cref="m_to"/>, and the rest interpolate by position. Time-independent, so it tests the
    /// per-character color ramp rather than animation.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-07-01</remarks>
    [Preserve]
    [TextEffect("gradient")]
    public class GradientColorEffect : ColorTextEffect
    {
        protected Color m_from = new Color(1f, 0.35f, 0.35f);
        protected Color m_to = new Color(0.35f, 0.5f, 1f);

        public override void Apply(ref CharMod mod, int charIndex, int spanLength, float time, in EffectArgs args)
        {
            var t = spanLength <= 1 ? 0f : (float)charIndex / (spanLength - 1);
            mod.Color = Color.Lerp(m_from, m_to, t);
        }
    }
}
