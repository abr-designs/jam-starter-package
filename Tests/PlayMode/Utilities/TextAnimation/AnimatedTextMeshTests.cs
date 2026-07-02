// Created by Claude (claude-opus-4-8)
// Date: 2026-06-24

using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using Utilities.TextAnimation;

namespace Tests.Utilities.TextAnimation
{
    public class AnimatedTextMeshTests
    {
        public enum TmpKind
        {
            World,
            Ugui,
        }

        private const float MOVED_THRESHOLD = 1e-6f;

        // Time chosen so the wave/pulse sine is positive, giving a predictable upward / grow direction.
        private const float POSITIVE_PHASE_TIME = 0.1f;

        private readonly List<GameObject> m_created = new();
        private readonly List<AnimatedText> m_animated = new();

        [OneTimeSetUp]
        public void RequireTmpEssentials()
        {
            if (TMP_Settings.instance == null || TMP_Settings.defaultFontAsset == null)
                Assert.Ignore("TMP Essential Resources not imported; skipping mesh-dependent tests.");
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var animated in m_animated)
                animated.Dispose();
            m_animated.Clear();

            foreach (var gameObject in m_created)
            {
                if (gameObject != null)
                    Object.DestroyImmediate(gameObject);
            }
            m_created.Clear();
        }

        // --- Span building ---

        [Test]
        public void Spans_AnimatedTag_AreBuilt([Values] TmpKind kind)
        {
            var text = CreateText(kind, "Hello <anim motion=\"wave\">world</anim>!");
            var animated = Animate(text);

            Assert.IsTrue(animated.HasSpans);
        }

        [Test]
        public void Spans_UnknownKey_AreNotBuilt([Values] TmpKind kind)
        {
            var text = CreateText(kind, "Hello <anim motion=\"nope\">world</anim>!");
            var animated = Animate(text);

            Assert.IsFalse(animated.HasSpans, "A tag whose keys match no effect should stay inert.");
        }

        // --- Displacement ---

        [Test]
        public void Apply_SpanChars_Move_NonSpanChars_StayPut([Values] TmpKind kind)
        {
            // Visible characters are "AAABBBCCC"; the wave wraps the middle three (indices 3..5).
            var text = CreateText(kind, "AAA<anim motion=\"wave\">BBB</anim>CCC");
            var animated = Animate(text);

            const int spanStart = 3;
            const int spanEnd = 6;

            var before = CaptureCharCenters(text);
            animated.Apply(POSITIVE_PHASE_TIME, force: true);
            var after = CaptureCharCenters(text);

            foreach (var entry in before)
            {
                var characterIndex = entry.Key;
                var moved = (after[characterIndex] - entry.Value).sqrMagnitude > MOVED_THRESHOLD;

                if (characterIndex >= spanStart && characterIndex < spanEnd)
                    Assert.IsTrue(moved, $"Span character {characterIndex} should have moved.");
                else
                    Assert.IsFalse(moved, $"Non-span character {characterIndex} should not move.");
            }
        }

        [Test]
        public void Wave_OffsetsSpanVerticesVertically([Values] TmpKind kind)
        {
            var text = CreateText(kind, "<anim motion=\"wave\">W</anim>");
            var animated = Animate(text);

            var before = CaptureCharCenters(text);
            animated.Apply(POSITIVE_PHASE_TIME, force: true);
            var after = CaptureCharCenters(text);

            var delta = after[0] - before[0];

            Assert.Greater(delta.y, 0f, "Wave at a positive phase should push the character up.");
            Assert.AreEqual(0f, delta.x, 0.001f, "Wave should not move characters horizontally.");
        }

        [Test]
        public void Pulse_ChangesSpanCharacterSize([Values] TmpKind kind)
        {
            var text = CreateText(kind, "<anim motion=\"pulse\">W</anim>");
            var animated = Animate(text);

            var before = CharacterDiagonal(text, 0);
            animated.Apply(POSITIVE_PHASE_TIME, force: true);
            var after = CharacterDiagonal(text, 0);

            Assert.Greater(after, before, "Pulse at a positive phase should grow the character.");
        }

        // --- Color channel ---

        [Test]
        public void Color_ChangesVertexColor_PositionStays([Values] TmpKind kind)
        {
            var text = CreateText(kind, "<anim color=\"rainbow\">WWWW</anim>");
            var animated = Animate(text);

            var beforeCenters = CaptureCharCenters(text);
            var beforeColors = CaptureCharColors(text);
            animated.Apply(POSITIVE_PHASE_TIME, force: true);
            var afterCenters = CaptureCharCenters(text);
            var afterColors = CaptureCharColors(text);

            foreach (var entry in beforeCenters)
            {
                var drift = (afterCenters[entry.Key] - entry.Value).sqrMagnitude;
                Assert.Less(drift, MOVED_THRESHOLD, $"Color-only span moved character {entry.Key}.");
                Assert.AreNotEqual(beforeColors[entry.Key], afterColors[entry.Key],
                    $"Color-only span should have recolored character {entry.Key}.");
            }
        }

        [Test]
        public void MotionAndColor_ComposeOnSameSpan([Values] TmpKind kind)
        {
            var text = CreateText(kind, "<anim motion=\"wave\" color=\"rainbow\">W</anim>");
            var animated = Animate(text);

            var beforeCenter = CaptureCharCenters(text)[0];
            var beforeColor = CaptureCharColors(text)[0];
            animated.Apply(POSITIVE_PHASE_TIME, force: true);
            var afterCenter = CaptureCharCenters(text)[0];
            var afterColor = CaptureCharColors(text)[0];

            Assert.Greater((afterCenter - beforeCenter).y, 0f, "Motion channel should still move the character.");
            Assert.AreNotEqual(beforeColor, afterColor, "Color channel should still recolor the character.");
        }

        // --- Restore ---

        [Test]
        public void Restore_ReturnsVerticesToOriginal([Values] TmpKind kind)
        {
            var text = CreateText(kind, "AAA<anim motion=\"wave\">BBB</anim>");
            var animated = Animate(text);

            var before = CaptureCharCenters(text);
            animated.Apply(POSITIVE_PHASE_TIME, force: true);
            animated.Restore();
            var after = CaptureCharCenters(text);

            foreach (var entry in before)
            {
                var drift = (after[entry.Key] - entry.Value).sqrMagnitude;
                Assert.Less(drift, MOVED_THRESHOLD, $"Character {entry.Key} was not restored to its original position.");
            }
        }

        // --- Re-parse ---

        [Test]
        public void ReParse_OnTextChange_RebuildsSpans([Values] TmpKind kind)
        {
            var text = CreateText(kind, "plain text");
            var animated = Animate(text);

            Assert.IsFalse(animated.HasSpans);

            text.text = "now <anim motion=\"wave\">animated</anim>";
            text.ForceMeshUpdate(); // raises TEXT_CHANGED, which drives AnimatedText.Refresh

            Assert.IsTrue(animated.HasSpans, "AnimatedText should rebuild spans after the text changes.");
        }

        // --- Helpers ---

        private TMP_Text CreateText(TmpKind kind, string markup)
        {
            TMP_Text text;

            if (kind == TmpKind.Ugui)
            {
                var canvasObject = new GameObject("Canvas", typeof(Canvas));
                m_created.Add(canvasObject);

                var labelObject = new GameObject("Label");
                labelObject.transform.SetParent(canvasObject.transform);
                m_created.Add(labelObject);

                text = labelObject.AddComponent<TextMeshProUGUI>();
            }
            else
            {
                var labelObject = new GameObject("Label");
                m_created.Add(labelObject);

                text = labelObject.AddComponent<TextMeshPro>();
            }

            text.font = TMP_Settings.defaultFontAsset;
            // A roomy rect keeps short strings on one line so every character stays visible.
            text.rectTransform.sizeDelta = new Vector2(2000f, 400f);
            text.text = markup;
            text.ForceMeshUpdate();

            return text;
        }

        private AnimatedText Animate(TMP_Text text)
        {
            var animated = new AnimatedText(text);
            m_animated.Add(animated);
            return animated;
        }

        private static Dictionary<int, Vector3> CaptureCharCenters(TMP_Text text)
        {
            var textInfo = text.textInfo;
            var centers = new Dictionary<int, Vector3>();

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var characterInfo = textInfo.characterInfo[i];
                if (characterInfo.isVisible == false)
                    continue;

                var vertices = textInfo.meshInfo[characterInfo.materialReferenceIndex].vertices;
                var index = characterInfo.vertexIndex;
                centers[i] = (vertices[index] + vertices[index + 1] + vertices[index + 2] + vertices[index + 3]) * 0.25f;
            }

            return centers;
        }

        private static Dictionary<int, Color32> CaptureCharColors(TMP_Text text)
        {
            var textInfo = text.textInfo;
            var colors = new Dictionary<int, Color32>();

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var characterInfo = textInfo.characterInfo[i];
                if (characterInfo.isVisible == false)
                    continue;

                var vertexColors = textInfo.meshInfo[characterInfo.materialReferenceIndex].colors32;
                colors[i] = vertexColors[characterInfo.vertexIndex];
            }

            return colors;
        }

        private static float CharacterDiagonal(TMP_Text text, int characterIndex)
        {
            var textInfo = text.textInfo;
            var characterInfo = textInfo.characterInfo[characterIndex];
            var vertices = textInfo.meshInfo[characterInfo.materialReferenceIndex].vertices;
            var index = characterInfo.vertexIndex;

            return Vector3.Distance(vertices[index], vertices[index + 2]);
        }
    }
}
