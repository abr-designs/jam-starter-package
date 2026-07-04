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
    public class PulseMotionEffect : MotionTextEffect
    {
        protected float m_amplitude = 0.25f;
        protected float m_speed = 6f;
        protected float m_charPhase = 0.5f;

        // Inline args: pulse(amplitude, speed, charPhase).
        public override void Apply(ref CharMod mod, int charIndex, int spanLength, float time, in EffectArgs args)
        {
            var amplitude = args.GetFloat(0, m_amplitude);
            var speed = args.GetFloat(1, m_speed);
            var charPhase = args.GetFloat(2, m_charPhase);

            mod.Scale = 1f + Mathf.Sin(time * speed + charIndex * charPhase) * amplitude;
        }

        public override string ValidateArgs(in EffectArgs args) => ValidateFloats(args, "amplitude", "speed", "charPhase");
    }
}
