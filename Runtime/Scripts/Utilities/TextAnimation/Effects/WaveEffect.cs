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
    public class WaveEffect : TextEffect
    {
        protected float m_amplitude = 5f;
        protected float m_speed = 6f;
        protected float m_charPhase = 0.5f;

        public override void Apply(ref CharMod mod, int charIndex, float time)
        {
            var y = Mathf.Sin(time * m_speed + charIndex * m_charPhase) * m_amplitude;
            mod.Offset = new Vector3(0f, y, 0f);
        }
    }
}
