// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using TMPro;
using UnityEngine;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Ergonomic opt-in surface for the text animator, matching the package's extension-method convention.
    /// Mirrors the inspector "Animate Text" toggle: opting in adds an <see cref="AnimatedTextMarker"/> and
    /// opting out removes it, so the marker stays the single registration authority.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    public static class TMP_TextExtensions
    {
        public static void PlayTextAnimation(this TMP_Text textComponent)
        {
            if (textComponent == null)
                return;

            if (AnimatedTextMarker.Find(textComponent) == null)
                textComponent.gameObject.AddComponent<AnimatedTextMarker>();
        }

        public static void StopTextAnimation(this TMP_Text textComponent)
        {
            if (textComponent == null)
                return;

            var marker = AnimatedTextMarker.Find(textComponent);
            if (marker == null)
                return;

#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                Object.DestroyImmediate(marker);
                return;
            }
#endif
            Object.Destroy(marker);
        }
    }
}
