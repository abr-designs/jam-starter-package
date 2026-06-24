// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Maps a contiguous run of characters to a single resolved effect. Built from a TMP link span.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    public struct EffectRange
    {
        public TextEffect Effect;
        public int Start;
        public int Length;
    }
}
