// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Static manager that drives every opted-in <see cref="TMP_Text"/> from a single PlayerLoop tick.
    /// No MonoBehaviour or component is added to text GameObjects. Prefer the
    /// <see cref="TMP_TextExtensions.PlayTextAnimation"/> extension method as the opt-in surface.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    public static class TextAnimator
    {
        //Fields
        //================================================================================================================//

        #region Fields

        private static List<AnimatedText> s_ActiveTexts;

        #endregion //Fields

        //Properties
        //================================================================================================================//

        #region Properties

        internal static bool HasActiveTexts => s_ActiveTexts != null && s_ActiveTexts.Count > 0;

        #endregion //Properties

        //Public Methods
        //================================================================================================================//

        #region Public Methods

        public static void PlayTextAnimation(TMP_Text textComponent)
        {
            if (textComponent == null)
                return;

            if (s_ActiveTexts == null)
                s_ActiveTexts = new List<AnimatedText>();

            var existing = FindEntry(textComponent);
            if (existing != null)
            {
                existing.Refresh();
                return;
            }

            s_ActiveTexts.Add(new AnimatedText(textComponent));
        }

        public static void StopTextAnimation(TMP_Text textComponent)
        {
            if (textComponent == null || s_ActiveTexts == null)
                return;

            for (int i = 0; i < s_ActiveTexts.Count; i++)
            {
                if (s_ActiveTexts[i].TextComponent != textComponent)
                    continue;

                s_ActiveTexts[i].Restore();
                s_ActiveTexts[i].Dispose();
                s_ActiveTexts.RemoveAt(i);
                return;
            }
        }

        #endregion //Public Methods

        //Private Methods
        //================================================================================================================//

        #region Private Methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (s_ActiveTexts == null)
                s_ActiveTexts = new List<AnimatedText>();

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            var tickSystem = new PlayerLoopSystem
            {
                type = typeof(TextAnimator),
                updateDelegate = Tick,
            };

            if (TryInsertSystem(ref playerLoop, typeof(Update), tickSystem))
                PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static void Tick()
        {
            TickAll(Time.time);
        }

        // Drives every active text from a caller-supplied clock so the editor preview can pass a
        // realtime value while the PlayerLoop tick passes Time.time.
        internal static void TickAll(float time)
        {
            if (s_ActiveTexts == null || s_ActiveTexts.Count == 0)
                return;

            for (int i = s_ActiveTexts.Count - 1; i >= 0; i--)
            {
                var animatedText = s_ActiveTexts[i];
                if (animatedText.IsValid == false)
                {
                    animatedText.Dispose();
                    s_ActiveTexts.RemoveAt(i);
                    continue;
                }

                animatedText.Apply(time);
            }
        }

        // Restores and unregisters every active text. Used by the editor preview when leaving edit mode
        // so play mode starts with no preview-registered entries.
        internal static void StopAll()
        {
            if (s_ActiveTexts == null)
                return;

            for (int i = s_ActiveTexts.Count - 1; i >= 0; i--)
            {
                s_ActiveTexts[i].Restore();
                s_ActiveTexts[i].Dispose();
            }

            s_ActiveTexts.Clear();
        }

        private static AnimatedText FindEntry(TMP_Text textComponent)
        {
            for (int i = 0; i < s_ActiveTexts.Count; i++)
            {
                if (s_ActiveTexts[i].TextComponent == textComponent)
                    return s_ActiveTexts[i];
            }

            return null;
        }

        private static bool TryInsertSystem(
            ref PlayerLoopSystem parent,
            Type targetType,
            PlayerLoopSystem systemToInsert)
        {
            if (parent.subSystemList == null)
                return false;

            for (int i = 0; i < parent.subSystemList.Length; i++)
            {
                if (parent.subSystemList[i].type != targetType)
                    continue;

                var subSystems = new List<PlayerLoopSystem>(parent.subSystemList[i].subSystemList ?? Array.Empty<PlayerLoopSystem>())
                {
                    systemToInsert
                };
                parent.subSystemList[i].subSystemList = subSystems.ToArray();
                return true;
            }

            for (int i = 0; i < parent.subSystemList.Length; i++)
            {
                if (TryInsertSystem(ref parent.subSystemList[i], targetType, systemToInsert))
                    return true;
            }

            return false;
        }

        #endregion //Private Methods
    }
}
