// Created by Claude (claude-opus-4-8)
// Date: 2026-07-01

namespace Utilities.TextAnimation
{
    /// <summary>
    /// One parsed <c>&lt;anim&gt;</c> run as recorded by <see cref="AnimTagPreprocessor"/>, before it is
    /// mapped to visible characters. <see cref="SourceStart"/> and <see cref="SourceEnd"/> are indices
    /// into the preprocessed (tag-stripped) string, which line up with <c>TMP_CharacterInfo.index</c>.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-07-01</remarks>
    public struct RawAnimSpan
    {
        public int SourceStart;
        public int SourceEnd;
        public string MotionKey;
        public EffectArgs MotionArgs;
        public string ColorKey;
        public EffectArgs ColorArgs;
    }
}
