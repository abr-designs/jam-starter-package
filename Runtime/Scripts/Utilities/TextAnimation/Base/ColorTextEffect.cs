// Created by Claude (claude-opus-4-8)
// Date: 2026-07-01

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Base for effects on the <c>color</c> channel of an <c>&lt;anim&gt;</c> tag: those that write the
    /// <see cref="CharMod"/> color. The written color is multiplied against the character's original
    /// vertex color, so on default white text it shows as the literal color, and on tinted text it
    /// modulates. Extending this routes a key into the registry's color table.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-07-01</remarks>
    public abstract class ColorTextEffect : TextEffect
    {
    }
}
