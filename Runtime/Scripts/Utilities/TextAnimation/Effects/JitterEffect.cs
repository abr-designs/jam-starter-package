// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using UnityEngine;
using UnityEngine.Scripting;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Per-frame random position and rotation jitter. Demonstrates that an effect can drive rotation
    /// through <see cref="CharMod"/>, not just offset.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    [Preserve]
    [TextEffect("jitter")]
    public class JitterEffect : TextEffect
    {
        protected float m_positionAmount = 3f;
        protected float m_rotationAmount = 8f;

        public override void Apply(ref CharMod mod, int charIndex, float time)
        {
            mod.Offset = new Vector3(
                Random.Range(-m_positionAmount, m_positionAmount),
                Random.Range(-m_positionAmount, m_positionAmount),
                0f);

            mod.RotationDeg = Random.Range(-m_rotationAmount, m_rotationAmount);
        }
    }
}
