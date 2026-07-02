// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using UnityEngine;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Per-character displacement a <see cref="TextEffect"/> writes each frame. Identity defaults
    /// (zero offset, zero rotation, unit scale, white) leave the character untouched. Passed by ref
    /// so future effect kinds can use rotation, scale, or color without changing the effect signature.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    public struct CharMod
    {
        public Vector3 Offset;
        public float RotationDeg;
        public float Scale;
        public Color32 Color;

        public static CharMod Identity { get; } = new CharMod
        {
            Offset = Vector3.zero,
            RotationDeg = 0f,
            Scale = 1f,
            Color = new Color32(255, 255, 255, 255),
        };
    }
}
