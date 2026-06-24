// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Base for a per-character text animation. The extension point: subclass this, tag it with
    /// <see cref="TextEffectAttribute"/> to declare its link id, hold default tuning values as fields,
    /// and implement the per-character math. Variants are subclasses with different defaults and keys.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    public abstract class TextEffect
    {
        /// <summary>
        /// Write displacement for one character into <paramref name="mod"/>.
        /// </summary>
        /// <param name="charIndex">Index of the character within its span, not the full text.</param>
        /// <param name="time">Seconds since startup, shared across every animated text.</param>
        public abstract void Apply(ref CharMod mod, int charIndex, float time);
    }
}
