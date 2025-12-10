using System.Collections;
using System.Runtime.CompilerServices;
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
            [Values(-1f, 0f, 0.5f, 1f)]float time)
        {
            if(time < 0f)
                LogAssert.Expect(LogType.Assert, "Time must be greater than or equal to zero");
            
            var expectedEndTime = Time.time + time;
            yield return ScreenFader.FadeIn(time, null);

            if (time < 0f)
                yield break;
            
            IsWithinThreshold(expectedEndTime);
            Assert.AreEqual(_image.color, Color.clear);
        }
        [UnityTest]
        public IEnumerator FadeOutTimed(
            [Values(-1f, 0f, 0.5f, 1f)]float time)
        {
            if(time < 0f)
                LogAssert.Expect(LogType.Assert, "Time must be greater than or equal to zero");
            
            var expectedEndTime = Time.time + time;
            yield return ScreenFader.FadeOut(time, null);
            
            if (time < 0f)
                yield break;
            
            IsWithinThreshold(expectedEndTime);
            Assert.AreEqual(_image.color, Color.black);
        }
        [UnityTest]
        public IEnumerator FadeInOutTimed(
            [Values(-1f, 0f, 0.5f, 1f)]float time)
        {
            if(time < 0f)
                LogAssert.Expect(LogType.Assert, "Time must be greater than or equal to zero");
            
            var expectedEndTime = Time.time + time;
            yield return ScreenFader.FadeInOut(time, () =>
            {
                if(time < 0f)
                    LogAssert.Expect(LogType.Assert, "Time must be greater than or equal to zero");
                
            }, null);

            if (time < 0f)
                yield break;
            
            IsWithinThreshold(expectedEndTime);
            Assert.AreEqual(_image.color, Color.clear);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void IsWithinThreshold(float expectedEndTime)
        {
            const float THRESHOLD = 0.05f;
            
            var diff = Time.time - expectedEndTime;
            Assert.IsTrue(diff <= THRESHOLD, $"{expectedEndTime} != {Time.time} [DIFF {Time.time - expectedEndTime}]");
        }
    }
}