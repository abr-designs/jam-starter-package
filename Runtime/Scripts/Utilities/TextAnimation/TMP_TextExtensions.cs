// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using TMPro;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Ergonomic opt-in surface for the text animator, matching the package's extension-method convention.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    public static class TMP_TextExtensions
    {
        public static void PlayTextAnimation(this TMP_Text textComponent)
        {
            TextAnimator.PlayTextAnimation(textComponent);
        }

        public static void StopTextAnimation(this TMP_Text textComponent)
        {
            TextAnimator.StopTextAnimation(textComponent);
        }
    }
}
