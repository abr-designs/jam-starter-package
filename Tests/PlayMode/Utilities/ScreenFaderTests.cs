using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Utilities;
using Assert = UnityEngine.Assertions.Assert;

namespace Tests.Utilities
{
    public class ScreenFaderTests
    {
        private Image _image;
        private ScreenFader _fader;
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _fader = ScreenFader.Instance;
            _image ??=  _fader.GetComponentInChildren<Image>();
        }
        [UnityTest]
        public IEnumerator FadeInDefaultTest()
        {
            yield return ScreenFader.FadeIn(null);
            
            Assert.AreEqual(_image.color, Color.clear);
        }
        [UnityTest]
        public IEnumerator FadeOutDefaultTest()
        {
            yield return ScreenFader.FadeOut(null);
            
            Assert.AreEqual(_image.color, Color.black);
        }
        [UnityTest]
        public IEnumerator FadeInOutDefaultTest()
        {
            yield return ScreenFader.FadeInOut(null, null);
            Assert.AreEqual(_image.color, Color.clear);
        }
    
        [UnityTest]
        public IEnumerator FadeInTimed(
            [Values(0f, 0.5f, 1f)]float time)
        {
            yield return ScreenFader.FadeIn(time, null);
            Assert.AreEqual(_image.color, Color.clear);
        }
        [UnityTest]
        public IEnumerator FadeOutTimed(
            [Values(0f, 0.5f, 1f)]float time)
        {
            yield return ScreenFader.FadeOut(time, null);
            Assert.AreEqual(_image.color, Color.black);
        }
        [UnityTest]
        public IEnumerator FadeInOutTimed(
            [Values(0f, 0.5f, 1f)]float time)
        {
            yield return ScreenFader.FadeInOut(time, null, null);
            Assert.AreEqual(_image.color, Color.clear);
        }
    }
}