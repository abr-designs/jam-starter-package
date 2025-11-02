using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Utilities
{
    public class ScreenFader : MonoBehaviour
    {
        private static readonly Color32 Black = new Color32(0, 0, 0, 255);
        private static readonly Color32 Clear = new Color32(0, 0, 0, 0);

        [SerializeField] 
        private float defaultFadeTime = 0.5f;
        [SerializeField]
        private Image blackImage;

        //============================================================================================================//
        public static void ForceSetColorBlack()
        {
            Instance.blackImage.color = Black;
        }
        public static void ForceSetColorClear()
        {
            Instance.blackImage.color = Clear;
        }

        public static Coroutine FadeInOut(Action onFaded, Action onComplete)
        {
            return FadeInOut(Instance.defaultFadeTime, onFaded, onComplete);
        }
        
        public static Coroutine FadeOut(Action onComplete)
        {
            return FadeOut(Instance.defaultFadeTime, onComplete);
        }
        
        public static Coroutine FadeIn(Action onComplete)
        {
            return FadeIn(Instance.defaultFadeTime, onComplete);
        }
        
        public static Coroutine FadeInOut(float time, Action onFaded, Action onComplete)
        {
            Assert.IsTrue(time >= 0f, "Time must be greater than or equal to zero");
            return Instance.StartCoroutine(Instance.FadeInOutCoroutine(time, onFaded, onComplete));
        }
        
        public static Coroutine FadeOut(float time, Action onComplete)
        {
            Assert.IsTrue(time >= 0f, "Time must be greater than or equal to zero");
            
            return Instance.StartCoroutine(Instance.FadeCoroutine(Clear, Black, time, onComplete));
        }
        
        public static Coroutine FadeIn(float time, Action onComplete)
        {
            Assert.IsTrue(time >= 0f, "Time must be greater than or equal to zero");
            
            return Instance.StartCoroutine(Instance.FadeCoroutine(Black, Clear, time, onComplete));
        }
        
        //Instance Coroutines
        //============================================================================================================//

        private IEnumerator FadeInOutCoroutine(float time, Action onFaded, Action onComplete)
        {
            var halfTime = time / 2f;

            yield return StartCoroutine(FadeCoroutine(Clear, Black, halfTime, onFaded));
            
            yield return StartCoroutine(FadeCoroutine(Black, Clear, halfTime, onComplete));
        }

        private IEnumerator FadeCoroutine(Color32 startColor, Color32 endColor, float time, Action onCompleted)
        {
            blackImage.color = startColor;

            for (float t = 0; t < time; t += Time.deltaTime)
            {
                blackImage.color = Color32.Lerp(startColor, endColor, t / time);
                yield return null;
            }
            
            blackImage.color = endColor;
            onCompleted?.Invoke();
        }
        //============================================================================================================//

        public static ScreenFader Instance => GetOrCreateSingleton();
        private static ScreenFader _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogError($"Attempted to create Multiple instances of {nameof(ScreenFader)}");
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private static ScreenFader GetOrCreateSingleton()
        {
            if (_instance != null)
                return _instance;
            
            var container = new GameObject("=== GENERATED SCREEN FADER ===");
            var screenFader = container.AddComponent<ScreenFader>();
            var canvas = container.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var childImage = new GameObject("BlackImage").AddComponent<Image>();
            childImage.color = Color.clear;
            childImage.transform.SetParent(canvas.transform, false);
            
            var childRectTransform = childImage.transform as RectTransform;
            childRectTransform.anchorMin =  Vector2.zero;
            childRectTransform.anchorMax =  Vector2.one;
            childRectTransform.sizeDelta =  Vector2.zero;

            screenFader.defaultFadeTime = 0.5f;
            screenFader.blackImage = childImage;

            _instance = screenFader;
            return screenFader;
        }
        //============================================================================================================//

    }
}