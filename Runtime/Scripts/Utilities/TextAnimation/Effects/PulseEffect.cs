// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using UnityEngine;
using UnityEngine.Scripting;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Per-character size pulse that oscillates scale around 1, each character phase-shifted from the
    /// last. Demonstrates that an effect can drive scale through <see cref="CharMod"/>, not just offset.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    [Preserve]
    [TextEffect("pulse")]
    public class PulseEffect : TextEffect
    {
        protected float m_amplitude = 0.25f;
        protected float m_speed = 6f;
        protected float m_charPhase = 0.5f;

        public override void Apply(ref CharMod mod, int charIndex, float time)
        {
            mod.Scale = 1f + Mathf.Sin(time * m_speed + charIndex * m_charPhase) * m_amplitude;
        }
    }
}
