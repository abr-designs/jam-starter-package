// Created by Claude (claude-opus-4-8)
// Date: 2026-06-24

using UnityEngine;
using UnityEngine.Scripting;
using Utilities.TextAnimation;

namespace Tests.Utilities.TextAnimation
{
    /// <summary>
    /// Test-only effect living outside the runtime assembly. Its unique key proves the registry's
    /// reflection scan discovers effects in any loaded assembly, and cannot collide with a built-in key.
    /// </summary>
    [Preserve]
    [TextEffect("__test_marker")]
    public class TestMarkerEffect : TextEffect
    {
        public static readonly Vector3 MarkerOffset = new Vector3(7f, 0f, 0f);

        public override void Apply(ref CharMod mod, int charIndex, float time)
        {
            mod.Offset = MarkerOffset;
        }
    }
}
