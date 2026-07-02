// Created by Claude (claude-opus-4-8)
// Date: 2026-07-01

using System.Globalization;
using UnityEngine;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// The positional arguments parsed from an inline tag value such as <c>wave(20, 2)</c> or
    /// <c>tint(#FF0044)</c>. The tokens are kept as raw strings so an effect can interpret any type it
    /// wants and validate the contents itself. The typed getters use non-allocating parses, so reading
    /// them each frame stays allocation-free; the split happens once per text change.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-07-01</remarks>
    public readonly struct EffectArgs
    {
        private readonly string[] m_args;

        public EffectArgs(string[] args)
        {
            m_args = args;
        }

        public int Count => m_args?.Length ?? 0;

        /// <summary>Return the raw token at <paramref name="index"/>, or <paramref name="fallback"/> when absent.</summary>
        public string GetString(int index, string fallback = null)
        {
            return m_args != null && index >= 0 && index < m_args.Length ? m_args[index] : fallback;
        }

        public float GetFloat(int index, float fallback)
        {
            return float.TryParse(GetString(index), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value
                : fallback;
        }

        public int GetInt(int index, int fallback)
        {
            return int.TryParse(GetString(index), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : fallback;
        }

        public bool GetBool(int index, bool fallback)
        {
            return bool.TryParse(GetString(index), out var value) ? value : fallback;
        }

        /// <summary>Parse a token as an HTML color (e.g. <c>#FF0044</c> or a named color), else the fallback.</summary>
        public Color GetColor(int index, Color fallback)
        {
            return ColorUtility.TryParseHtmlString(GetString(index), out var value) ? value : fallback;
        }
    }
}
