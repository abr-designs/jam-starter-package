// Created by Claude (claude-opus-4-8)
// Date: 2026-07-01

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Base for effects on the <c>motion</c> channel of an <c>&lt;anim&gt;</c> tag: those that write the
    /// <see cref="CharMod"/> offset, scale, or rotation. Extending this (rather than
    /// <see cref="ColorTextEffect"/>) is what routes a key into the registry's motion table, so a motion
    /// key can never be resolved as a color and vice versa.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-07-01</remarks>
    public abstract class MotionTextEffect : TextEffect
    {
    }
}
