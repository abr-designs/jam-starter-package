// Created by Claude (claude-opus-4-8)
// Date: 2026-07-01

using UnityEngine;
using UnityEngine.Scripting;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Blinks between two colors on a fixed period, good for warnings or emphasis. The active color is
    /// multiplied against the character's original vertex color.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-07-01</remarks>
    [Preserve]
    [TextEffect("flash")]
    public class FlashColorEffect : ColorTextEffect
    {
        protected Color m_colorA = Color.white;
        protected Color m_colorB = new Color(1f, 0.85f, 0.2f);
        protected float m_period = 0.5f;

        // Inline args: flash(period).
        public override void Apply(ref CharMod mod, int charIndex, int spanLength, float time, in EffectArgs args)
        {
            var period = args.GetFloat(0, m_period);
            if (period <= 0f)
                period = m_period;

            mod.Color = Mathf.Repeat(time, period) < period * 0.5f ? m_colorA : m_colorB;
        }
    }
}
