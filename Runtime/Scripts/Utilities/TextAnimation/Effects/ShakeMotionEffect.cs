// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using UnityEngine;
using UnityEngine.Scripting;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Per-character positional shake using Perlin noise on each axis, so the motion stays smooth
    /// rather than snapping between random values.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    [Preserve]
    [TextEffect("shake")]
    public class ShakeMotionEffect : MotionTextEffect
    {
        // amplitude is in ems: 1 = one line height.
        protected float m_amplitude = 0.12f;
        protected float m_frequency = 25f;

        // Inline args: shake(amplitude, frequency).
        public override void Apply(ref CharMod mod, int charIndex, int spanLength, float time, in EffectArgs args)
        {
            var amplitude = args.GetFloat(0, m_amplitude);
            var frequency = args.GetFloat(1, m_frequency);

            var noiseTime = time * frequency;
            var offsetX = (Mathf.PerlinNoise(charIndex, noiseTime) - 0.5f) * 2f * amplitude;
            var offsetY = (Mathf.PerlinNoise(charIndex + 100f, noiseTime) - 0.5f) * 2f * amplitude;

            mod.Offset = new Vector3(offsetX, offsetY, 0f);
        }
    }
}
