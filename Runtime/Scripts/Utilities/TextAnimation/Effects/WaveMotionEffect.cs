// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using UnityEngine;
using UnityEngine.Scripting;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Vertical sine wave that travels across the span, each character phase-shifted from the last.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    [Preserve]
    [TextEffect("wave")]
    public class WaveMotionEffect : MotionTextEffect
    {
        protected float m_amplitude = 5f;
        protected float m_speed = 6f;
        protected float m_charPhase = 0.5f;

        // Inline args: wave(amplitude, speed, charPhase).
        public override void Apply(ref CharMod mod, int charIndex, int spanLength, float time, in EffectArgs args)
        {
            var amplitude = args.GetFloat(0, m_amplitude);
            var speed = args.GetFloat(1, m_speed);
            var charPhase = args.GetFloat(2, m_charPhase);

            var y = Mathf.Sin(time * speed + charIndex * charPhase) * amplitude;
            mod.Offset = new Vector3(0f, y, 0f);
        }
    }
}
