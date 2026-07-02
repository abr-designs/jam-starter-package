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
    public class JitterMotionEffect : MotionTextEffect
    {
        protected float m_positionAmount = 3f;
        protected float m_rotationAmount = 8f;

        // Inline args: jitter(positionAmount, rotationAmount).
        public override void Apply(ref CharMod mod, int charIndex, int spanLength, float time, in EffectArgs args)
        {
            var positionAmount = args.GetFloat(0, m_positionAmount);
            var rotationAmount = args.GetFloat(1, m_rotationAmount);

            mod.Offset = new Vector3(
                Random.Range(-positionAmount, positionAmount),
                Random.Range(-positionAmount, positionAmount),
                0f);

            mod.RotationDeg = Random.Range(-rotationAmount, rotationAmount);
        }
    }
}
