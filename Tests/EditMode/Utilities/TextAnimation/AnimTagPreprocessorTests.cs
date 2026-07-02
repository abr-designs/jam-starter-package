// Created by Claude (claude-opus-4-8)
// Date: 2026-07-01

using NUnit.Framework;
using Utilities.TextAnimation;

namespace Tests.Utilities.TextAnimation
{
    public class AnimTagPreprocessorTests
    {
        private AnimTagPreprocessor m_preprocessor;

        [SetUp]
        public void SetUp()
        {
            m_preprocessor = new AnimTagPreprocessor();
        }

        [Test]
        public void PreprocessText_StripsAnimTags()
        {
            var result = m_preprocessor.PreprocessText("Hi <anim motion=\"wave\">there</anim>!");

            Assert.AreEqual("Hi there!", result);
        }

        [Test]
        public void PreprocessText_RecordsSpanRangeInStrippedCoordinates()
        {
            m_preprocessor.PreprocessText("Hi <anim motion=\"wave\">there</anim>!");

            Assert.AreEqual(1, m_preprocessor.Spans.Count);
            var span = m_preprocessor.Spans[0];

            // "Hi there!" -> "there" occupies indices 3..7, so the half-open range is [3, 8).
            Assert.AreEqual(3, span.SourceStart);
            Assert.AreEqual(8, span.SourceEnd);
        }

        [Test]
        public void PreprocessText_ParsesMotionKeyAndPositionalArgs()
        {
            m_preprocessor.PreprocessText("<anim motion=\"wave(20, 2)\">x</anim>");

            var span = m_preprocessor.Spans[0];

            Assert.AreEqual("wave", span.MotionKey);
            Assert.AreEqual(2, span.MotionArgs.Count);
            Assert.AreEqual(20f, span.MotionArgs.GetFloat(0, 0f));
            Assert.AreEqual(2f, span.MotionArgs.GetFloat(1, 0f));
        }

        [Test]
        public void PreprocessText_ExposesNonNumericArgsAsRawStrings()
        {
            // Args are kept as raw tokens, so an effect can interpret non-numeric values like a hex color.
            m_preprocessor.PreprocessText("<anim color=\"tint(#FF0044)\">x</anim>");

            var span = m_preprocessor.Spans[0];

            Assert.AreEqual("tint", span.ColorKey);
            Assert.AreEqual("#FF0044", span.ColorArgs.GetString(0));
        }

        [Test]
        public void PreprocessText_ParsesBothChannels()
        {
            m_preprocessor.PreprocessText("<anim motion=\"wave\" color=\"rainbow\">x</anim>");

            var span = m_preprocessor.Spans[0];

            Assert.AreEqual("wave", span.MotionKey);
            Assert.AreEqual("rainbow", span.ColorKey);
        }

        [Test]
        public void PreprocessText_BareKey_HasNoArgs()
        {
            m_preprocessor.PreprocessText("<anim motion=\"wave\">x</anim>");

            Assert.AreEqual(0, m_preprocessor.Spans[0].MotionArgs.Count);
        }

        [Test]
        public void PreprocessText_KeepsOtherTagsAndKeepsRangeAligned()
        {
            // The <b> tag must survive for TMP to parse; the span range is measured in the kept string.
            var result = m_preprocessor.PreprocessText("<b>hi</b> <anim motion=\"wave\">x</anim>");

            Assert.AreEqual("<b>hi</b> x", result);
            var span = m_preprocessor.Spans[0];

            // "x" sits at the final index of "<b>hi</b> x".
            Assert.AreEqual(result.Length - 1, span.SourceStart);
            Assert.AreEqual(result.Length, span.SourceEnd);
        }

        [Test]
        public void PreprocessText_NoAnimTags_RecordsNothing()
        {
            var result = m_preprocessor.PreprocessText("just <color=red>plain</color> text");

            Assert.AreEqual("just <color=red>plain</color> text", result);
            Assert.AreEqual(0, m_preprocessor.Spans.Count);
        }

        [Test]
        public void PreprocessText_UnclosedTag_ExtendsToEnd()
        {
            m_preprocessor.PreprocessText("start <anim motion=\"wave\">tail");

            Assert.AreEqual(1, m_preprocessor.Spans.Count);
            var span = m_preprocessor.Spans[0];

            Assert.AreEqual("start tail".Length, span.SourceEnd);
        }

        [Test]
        public void PreprocessText_ReusedInstance_ClearsPreviousSpans()
        {
            m_preprocessor.PreprocessText("<anim motion=\"wave\">a</anim>");
            m_preprocessor.PreprocessText("plain");

            Assert.AreEqual(0, m_preprocessor.Spans.Count);
        }
    }
}
