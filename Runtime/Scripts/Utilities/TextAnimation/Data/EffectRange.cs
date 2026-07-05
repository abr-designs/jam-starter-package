// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Maps a contiguous run of characters to its resolved motion and color effects. Built from an
    /// <c>&lt;anim&gt;</c> span; either channel may be null when the tag set only one of them.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    public struct EffectRange
    {
        public int Start;
        public int Length;
        public MotionTextEffect Motion;
        public EffectArgs MotionArgs;
        public ColorTextEffect Color;
        public EffectArgs ColorArgs;
    }
}
