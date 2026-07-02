// Created by Claude (claude-opus-4-8)
// Date: 2026-07-01

using System.Collections.Generic;
using System.Text;
using TMPro;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// TMP text preprocessor that teaches the parser about the custom <c>&lt;anim&gt;</c> tag. TMP only
    /// understands its own rich-text tags, so before it parses we strip every <c>&lt;anim ...&gt;</c> and
    /// <c>&lt;/anim&gt;</c> token and record each run in <see cref="Spans"/>. Ranges are stored as indices
    /// into the returned (stripped) string, which match <c>TMP_CharacterInfo.index</c>, so
    /// <see cref="AnimatedText"/> can map them back to visible characters. Every other tag is copied
    /// through untouched so TMP still parses colors, styles, and the like.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-07-01</remarks>
    public sealed class AnimTagPreprocessor : ITextPreprocessor
    {
        //Fields
        //================================================================================================================//

        #region Fields

        private const string k_OpenTag = "anim";
        private const string k_CloseTag = "/anim";

        private readonly List<RawAnimSpan> m_spans = new();
        private readonly List<RawAnimSpan> m_openStack = new();
        private readonly StringBuilder m_output = new();

        #endregion //Fields

        //Properties
        //================================================================================================================//

        #region Properties

        /// <summary>Spans recorded on the most recent <see cref="PreprocessText"/> call.</summary>
        public IReadOnlyList<RawAnimSpan> Spans => m_spans;

        #endregion //Properties

        //Public Methods
        //================================================================================================================//

        #region Public Methods

        public string PreprocessText(string text)
        {
            m_spans.Clear();
            m_openStack.Clear();
            m_output.Clear();

            if (string.IsNullOrEmpty(text))
                return text;

            int i = 0;
            while (i < text.Length)
            {
                if (text[i] != '<')
                {
                    m_output.Append(text[i]);
                    i++;
                    continue;
                }

                int close = text.IndexOf('>', i);
                if (close < 0)
                {
                    // Unterminated '<': treat it as a literal character.
                    m_output.Append(text[i]);
                    i++;
                    continue;
                }

                var body = text.Substring(i + 1, close - i - 1);
                if (IsOpenAnimTag(body))
                    PushOpen(body);
                else if (IsCloseAnimTag(body))
                    PopClose();
                else
                    m_output.Append(text, i, close - i + 1); // any other tag: keep it for TMP to parse

                i = close + 1;
            }

            // Close any spans the author left open so their run still animates to the end of the text.
            while (m_openStack.Count > 0)
                Finalize(m_openStack.Count - 1);

            return m_output.ToString();
        }

        #endregion //Public Methods

        //Private Methods
        //================================================================================================================//

        #region Private Methods

        private static bool IsOpenAnimTag(string body)
        {
            if (body.StartsWith(k_OpenTag, System.StringComparison.OrdinalIgnoreCase) == false)
                return false;

            // "anim" exactly, or "anim" followed by whitespace before its attributes.
            return body.Length == k_OpenTag.Length || char.IsWhiteSpace(body[k_OpenTag.Length]);
        }

        private static bool IsCloseAnimTag(string body)
        {
            return body.Trim().Equals(k_CloseTag, System.StringComparison.OrdinalIgnoreCase);
        }

        private void PushOpen(string body)
        {
            var span = new RawAnimSpan { SourceStart = m_output.Length };

            if (TryGetAttribute(body, "motion", out var motionValue))
                ParseKeyAndArgs(motionValue, out span.MotionKey, out span.MotionArgs);

            if (TryGetAttribute(body, "color", out var colorValue))
                ParseKeyAndArgs(colorValue, out span.ColorKey, out span.ColorArgs);

            m_openStack.Add(span);
        }

        private void PopClose()
        {
            if (m_openStack.Count == 0)
                return;

            Finalize(m_openStack.Count - 1);
        }

        private void Finalize(int stackIndex)
        {
            var span = m_openStack[stackIndex];
            span.SourceEnd = m_output.Length;
            m_openStack.RemoveAt(stackIndex);

            // Drop empty runs; they carry no characters to animate.
            if (span.SourceEnd > span.SourceStart)
                m_spans.Add(span);
        }

        // Reads name="value" (or an unquoted value up to the next whitespace) from a tag body.
        private static bool TryGetAttribute(string body, string name, out string value)
        {
            value = null;

            int search = 0;
            while (search < body.Length)
            {
                int nameStart = body.IndexOf(name, search, System.StringComparison.OrdinalIgnoreCase);
                if (nameStart < 0)
                    return false;

                // Require a word boundary before the name so "color" cannot match inside another token.
                bool boundary = nameStart == 0 || char.IsWhiteSpace(body[nameStart - 1]);
                int cursor = nameStart + name.Length;
                if (boundary == false)
                {
                    search = cursor;
                    continue;
                }

                while (cursor < body.Length && char.IsWhiteSpace(body[cursor]))
                    cursor++;

                if (cursor >= body.Length || body[cursor] != '=')
                {
                    search = cursor;
                    continue;
                }

                cursor++;
                while (cursor < body.Length && char.IsWhiteSpace(body[cursor]))
                    cursor++;

                if (cursor < body.Length && body[cursor] == '"')
                {
                    int end = body.IndexOf('"', cursor + 1);
                    if (end < 0)
                        return false;

                    value = body.Substring(cursor + 1, end - cursor - 1);
                    return true;
                }

                int wordEnd = cursor;
                while (wordEnd < body.Length && char.IsWhiteSpace(body[wordEnd]) == false)
                    wordEnd++;

                value = body.Substring(cursor, wordEnd - cursor);
                return true;
            }

            return false;
        }

        // Splits "wave(20, 2)" into key "wave" and raw tokens ["20", "2"]. A bare "wave" yields no args.
        private static void ParseKeyAndArgs(string value, out string key, out EffectArgs args)
        {
            args = default;

            int open = value.IndexOf('(');
            if (open < 0)
            {
                key = value.Trim();
                return;
            }

            key = value.Substring(0, open).Trim();

            int end = value.IndexOf(')', open + 1);
            int innerStart = open + 1;
            int innerLength = (end < 0 ? value.Length : end) - innerStart;
            if (innerLength <= 0)
                return;

            var tokens = value.Substring(innerStart, innerLength).Split(',');
            for (int i = 0; i < tokens.Length; i++)
                tokens[i] = tokens[i].Trim();

            args = new EffectArgs(tokens);
        }

        #endregion //Private Methods
    }
}
