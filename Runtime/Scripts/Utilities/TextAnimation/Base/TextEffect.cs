// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Base for a per-character text animation. Do not subclass this directly; extend
    /// <see cref="MotionTextEffect"/> or <see cref="ColorTextEffect"/> so the registry can resolve the two
    /// <c>&lt;anim&gt;</c> channels separately. Tag the subclass with <see cref="TextEffectAttribute"/>
    /// to declare its key, hold default tuning as fields, and implement the per-character math.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    public abstract class TextEffect
    {
        /// <summary>
        /// Write displacement or color for one character into <paramref name="mod"/>.
        /// </summary>
        /// <param name="charIndex">Index of the character within its span, not the full text.</param>
        /// <param name="spanLength">Number of characters in the span, for normalizing <paramref name="charIndex"/>.</param>
        /// <param name="time">Seconds since startup, shared across every animated text.</param>
        /// <param name="args">Inline positional arguments from the tag, empty when the author gave none.</param>
        public abstract void Apply(ref CharMod mod, int charIndex, int spanLength, float time, in EffectArgs args);

        /// <summary>
        /// Optionally report a problem with the inline <paramref name="args"/> so a typo like
        /// <c>wave(foo)</c> surfaces to the author. Return null when the args are fine. This is called once
        /// when spans are built (on text change), never per frame, so it is the place to validate rather
        /// than repeatedly falling back inside <see cref="Apply"/>. Default: no validation.
        /// </summary>
        public virtual string ValidateArgs(in EffectArgs args) => null;

        /// <summary>
        /// Helper for the common case where every listed slot must be a number: returns a message naming the
        /// first slot that is present but not a float, or null when they are all absent or numeric.
        /// </summary>
        protected static string ValidateFloats(in EffectArgs args, params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                if (args.IsFloat(i))
                    continue;

                return $"argument {i} ({names[i]}) is '{args.GetString(i)}', which is not a number";
            }

            return null;
        }
    }
}
