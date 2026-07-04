// Created by Claude (claude-opus-4-8)
// Date: 2026-07-01

using UnityEngine;
using UnityEngine.Scripting;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Cycles hue across the span and over time, so the run reads as a moving rainbow. The color is
    /// multiplied against the character's original vertex color, so it is most vivid on white text.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-07-01</remarks>
    [Preserve]
    [TextEffect("rainbow")]
    public class RainbowColorEffect : ColorTextEffect
    {
        protected float m_speed = 2f;
        protected float m_charPhase = 0.15f;

        // Inline args: rainbow(speed, charPhase).
        public override void Apply(ref CharMod mod, int charIndex, int spanLength, float time, in EffectArgs args)
        {
            var speed = args.GetFloat(0, m_speed);
            var charPhase = args.GetFloat(1, m_charPhase);

            var hue = Mathf.Repeat(time * speed * 0.1f + charIndex * charPhase, 1f);
            mod.Color = Color.HSVToRGB(hue, 1f, 1f);
        }

        public override string ValidateArgs(in EffectArgs args) => ValidateFloats(args, "speed", "charPhase");
    }
}
