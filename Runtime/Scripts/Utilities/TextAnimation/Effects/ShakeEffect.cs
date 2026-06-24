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
    public class ShakeEffect : TextEffect
    {
        protected float m_amplitude = 4f;
        protected float m_frequency = 25f;

        public override void Apply(ref CharMod mod, int charIndex, float time)
        {
            var noiseTime = time * m_frequency;
            var offsetX = (Mathf.PerlinNoise(charIndex, noiseTime) - 0.5f) * 2f * m_amplitude;
            var offsetY = (Mathf.PerlinNoise(charIndex + 100f, noiseTime) - 0.5f) * 2f * m_amplitude;

            mod.Offset = new Vector3(offsetX, offsetY, 0f);
        }
    }
}
