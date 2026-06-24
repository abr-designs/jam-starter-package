// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Utilities.TextAnimation.Editor
{
    /// <summary>
    /// Drives text animations in edit mode. The runtime <see cref="TextAnimator"/> ticks from the
    /// PlayerLoop, which only runs in Play mode, so this editor-only driver ticks from
    /// <see cref="EditorApplication.update"/> using a realtime clock and repaints the views each frame.
    /// Any loaded TMP text containing a `link` tag is animated automatically.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    [InitializeOnLoad]
    internal static class TextAnimatorEditorDriver
    {
        //Fields
        //================================================================================================================//

        #region Fields

        private const string k_LinkTagToken = "<link=";

        private static bool s_NeedsScan;
        private static bool s_IsProcessing;

        #endregion //Fields

        //Constructors
        //================================================================================================================//

        #region Constructors

        static TextAnimatorEditorDriver()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);

            s_NeedsScan = true;
        }

        #endregion //Constructors

        //Private Methods
        //================================================================================================================//

        #region Private Methods

        private static void OnEditorUpdate()
        {
            if (Application.isPlaying)
                return;

            // Scanning and ticking force mesh updates, which re-raise TEXT_CHANGED_EVENT synchronously.
            // Flag the work so OnTextChanged ignores those self-induced events instead of re-arming a scan.
            s_IsProcessing = true;
            try
            {
                if (s_NeedsScan)
                {
                    ScanLoadedScenes();
                    s_NeedsScan = false;
                }

                if (TextAnimator.HasActiveTexts == false)
                    return;

                TextAnimator.TickAll(Time.realtimeSinceStartup);
                InternalEditorUtility.RepaintAllViews();
            }
            finally
            {
                s_IsProcessing = false;
            }
        }

        private static void OnHierarchyChanged()
        {
            s_NeedsScan = true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
                TextAnimator.StopAll();
            else if (change == PlayModeStateChange.EnteredEditMode)
                s_NeedsScan = true;
        }

        private static void OnTextChanged(UnityEngine.Object changedObject)
        {
            if (Application.isPlaying || s_IsProcessing)
                return;

            // Defer registration to the next editor tick. Calling PlayTextAnimation here would force a
            // mesh update from inside the text-changed callback, which re-enters this handler and recurses.
            if (changedObject is TMP_Text textComponent && ContainsAnimatedLink(textComponent))
                s_NeedsScan = true;
        }

        private static void ScanLoadedScenes()
        {
            var textComponents = UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include);

            for (int i = 0; i < textComponents.Length; i++)
            {
                if (ContainsAnimatedLink(textComponents[i]))
                    TextAnimator.PlayTextAnimation(textComponents[i]);
            }
        }

        private static bool ContainsAnimatedLink(TMP_Text textComponent)
        {
            return textComponent != null
                && string.IsNullOrEmpty(textComponent.text) == false
                && textComponent.text.Contains(k_LinkTagToken);
        }

        #endregion //Private Methods
    }
}
