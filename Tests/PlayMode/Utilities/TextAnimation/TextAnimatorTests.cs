// Created by Claude (claude-opus-4-8)
// Date: 2026-06-24

using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using Utilities.TextAnimation;
using Assert = UnityEngine.Assertions.Assert;

namespace Tests.Utilities.TextAnimation
{
    public class TextAnimatorTests
    {
        private GameObject m_gameObject;
        private TMP_Text m_text;

        [SetUp]
        public void SetUp()
        {
            m_gameObject = new GameObject("TextAnimatorTests_Text");
            m_text = m_gameObject.AddComponent<TextMeshPro>();
            // Registration is font-independent; plain text avoids needing TMP Essentials for these cases.
            m_text.text = "Hello world";
        }

        [TearDown]
        public void TearDown()
        {
            TextAnimator.StopAll();

            if (m_gameObject != null)
                Object.DestroyImmediate(m_gameObject);
        }

        [Test]
        public void Play_RegistersText_HasActiveTextsTrue()
        {
            TextAnimator.PlayTextAnimation(m_text);

            Assert.IsTrue(TextAnimator.HasActiveTexts);
        }

        [Test]
        public void Play_SameTextTwice_NoDuplicate()
        {
            TextAnimator.PlayTextAnimation(m_text);
            TextAnimator.PlayTextAnimation(m_text);

            // A single Stop must clear it: a duplicate entry would survive and keep HasActiveTexts true.
            TextAnimator.StopTextAnimation(m_text);

            Assert.IsFalse(TextAnimator.HasActiveTexts);
        }

        [Test]
        public void Play_Null_DoesNotThrow()
        {
            TextAnimator.PlayTextAnimation(null);

            Assert.IsFalse(TextAnimator.HasActiveTexts);
        }

        [Test]
        public void Stop_UnregistersText()
        {
            TextAnimator.PlayTextAnimation(m_text);
            TextAnimator.StopTextAnimation(m_text);

            Assert.IsFalse(TextAnimator.HasActiveTexts);
        }

        [Test]
        public void TickAll_DestroyedText_DropsEntry()
        {
            TextAnimator.PlayTextAnimation(m_text);

            Object.DestroyImmediate(m_gameObject);
            m_gameObject = null;

            TextAnimator.TickAll(1f);

            Assert.IsFalse(TextAnimator.HasActiveTexts);
        }

        [UnityTest]
        public IEnumerator PlayerLoop_Smoke_AnimatesOverRealFrames()
        {
            if (TMP_Settings.instance == null || TMP_Settings.defaultFontAsset == null)
                NUnit.Framework.Assert.Ignore("TMP Essential Resources not imported; skipping PlayerLoop mesh smoke test.");

            m_text.font = TMP_Settings.defaultFontAsset;
            m_text.text = "<anim motion=\"wave\">animate</anim>";
            m_text.ForceMeshUpdate();

            var baselineY = SampleFirstSpanVertexY(m_text);

            TextAnimator.PlayTextAnimation(m_text);

            // The injected PlayerLoop tick (not a manual TickAll) should move the span over frames.
            var maxDelta = 0f;
            for (int i = 0; i < 10; i++)
            {
                yield return null;
                maxDelta = Mathf.Max(maxDelta, Mathf.Abs(SampleFirstSpanVertexY(m_text) - baselineY));
            }

            Assert.IsTrue(maxDelta > 0.01f,
                $"Expected the PlayerLoop tick to animate span vertices; max delta was {maxDelta}.");
        }

        // The <anim> tag is stripped by the preprocessor, so there is no TMP link to read. The span is
        // the whole visible string, so sample the first visible character's top-left vertex.
        private static float SampleFirstSpanVertexY(TMP_Text text)
        {
            var textInfo = text.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var characterInfo = textInfo.characterInfo[i];
                if (characterInfo.isVisible == false)
                    continue;

                var vertices = textInfo.meshInfo[characterInfo.materialReferenceIndex].vertices;
                return vertices[characterInfo.vertexIndex].y;
            }

            return 0f;
        }
    }
}
