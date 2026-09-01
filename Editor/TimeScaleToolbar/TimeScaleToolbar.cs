// Created by Claude (claude-opus-5)
// Date: 2026-08-07
// Based on TimeScale Toolbar by Paul Berne, MIT licensed. See LICENSE in this folder.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: InternalsVisibleTo("com.abrds.jam-starter.Editor.Tests")]

namespace JamStarter.Editor
{
    /// <summary>
    /// Adds a Time.timeScale slider & reset button to the Unity main toolbar. Forced override & maximum
    /// scale are configured through the element's right-click context menu.
    /// </summary>
    /// <remarks>Rewritten by Claude (claude-opus-5) on 2026-08-07</remarks>
    [InitializeOnLoad]
    public static class TimeScaleToolbar
    {
        //Fields
        //================================================================================================================//

        #region Fields

        private const string k_ElementPath = "Jam Starter/Time Scale";

        private const string k_TimeScaleKey = "TimeScaleToolbar_TimeScale";
        private const string k_ForcedOverrideKey = "TimeScaleToolbar_ForcedOverride";
        private const string k_MaxScaleKey = "TimeScaleToolbar_Max";

        private const float k_DefaultMaxScale = 2f;
        private const float k_MinimumMaxScale = 1f;
        private const float k_MaximumMaxScale = 100f;

        // Element values are fixed at construction, so adopting a drifted Time.timeScale means rebuilding the
        // element. Throttling keeps a game that animates timeScale from rebuilding the toolbar every frame.
        private const float k_DriftEpsilon = 0.001f;
        private const double k_RefreshIntervalSeconds = 0.1;

        private static readonly float[] k_MaxScalePresets = { 2f, 5f, 10f, 100f };

        private static float s_TimeScale;
        private static float s_MaxScale;
        private static bool s_ForcedOverride;
        private static double s_LastRefreshTime;

        #endregion // Fields

        //Constructors
        //================================================================================================================//

        #region Constructors

        static TimeScaleToolbar()
        {
            s_MaxScale = Mathf.Clamp(EditorPrefs.GetFloat(k_MaxScaleKey, k_DefaultMaxScale), k_MinimumMaxScale, k_MaximumMaxScale);
            s_ForcedOverride = EditorPrefs.GetBool(k_ForcedOverrideKey, false);
            s_TimeScale = ClampToMax(EditorPrefs.GetFloat(k_TimeScaleKey, 1f), s_MaxScale);

            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        #endregion // Constructors

        //Public Methods
        //================================================================================================================//

        #region Public Methods

        [MainToolbarElement(k_ElementPath, defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static IEnumerable<MainToolbarElement> CreateTimeScaleElements()
        {
            var sliderContent = new MainToolbarContent("Time Scale", "Drag to change Time.timeScale");
            var slider = new MainToolbarSlider(sliderContent, s_TimeScale, 0f, s_MaxScale, OnSliderValueChanged)
            {
                populateContextMenu = PopulateContextMenu,
            };

            var resetContent = new MainToolbarContent("Reset", "Reset Time.timeScale to 1");
            var resetButton = new MainToolbarButton(resetContent, OnResetClicked)
            {
                populateContextMenu = PopulateContextMenu,
            };

            return new MainToolbarElement[] { slider, resetButton };
        }

        #endregion // Public Methods

        //Private Methods
        //================================================================================================================//

        #region Private Methods

        /// <summary>
        /// Decides whether a drifted Time.timeScale is worth rebuilding the toolbar element for.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) on 2026-08-07</remarks>
        internal static bool ShouldRefresh(float displayedValue, float runtimeValue, double lastRefreshTime, double currentTime)
        {
            if (Mathf.Abs(runtimeValue - displayedValue) <= k_DriftEpsilon)
                return false;

            return currentTime - lastRefreshTime >= k_RefreshIntervalSeconds;
        }

        /// <summary>
        /// Clamps a timeScale into the configured range, used when the maximum is lowered below the current value.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5) on 2026-08-07</remarks>
        internal static float ClampToMax(float timeScale, float maxScale)
        {
            return Mathf.Clamp(timeScale, 0f, maxScale);
        }

        // Unity invokes this before the element's own Copy/Paste/Edit items and appends its own separator
        // afterwards, so leading with a separator would open the menu on an empty divider.
        private static void PopulateContextMenu(DropdownMenu menu)
        {
            menu.AppendAction(
                "Forced Override",
                _ => SetForcedOverride(!s_ForcedOverride),
                _ => s_ForcedOverride ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            foreach (var preset in k_MaxScalePresets)
            {
                menu.AppendAction(
                    "Max Scale/" + preset,
                    _ => SetMaxScale(preset),
                    _ => Mathf.Approximately(s_MaxScale, preset)
                        ? DropdownMenuAction.Status.Checked
                        : DropdownMenuAction.Status.Normal);
            }
        }

        private static void OnEditorUpdate()
        {
            var runtimeTimeScale = Time.timeScale;

            if (s_ForcedOverride)
            {
                if (!Mathf.Approximately(runtimeTimeScale, s_TimeScale))
                    Time.timeScale = s_TimeScale;
                return;
            }

            var currentTime = EditorApplication.timeSinceStartup;
            if (!ShouldRefresh(s_TimeScale, runtimeTimeScale, s_LastRefreshTime, currentTime))
                return;

            SetTimeScale(ClampToMax(runtimeTimeScale, s_MaxScale));
            s_LastRefreshTime = currentTime;
            MainToolbar.Refresh(k_ElementPath);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
                Time.timeScale = s_TimeScale;
        }

        private static void OnSliderValueChanged(float newValue)
        {
            SetTimeScale(newValue);
            Time.timeScale = s_TimeScale;
        }

        private static void OnResetClicked()
        {
            SetTimeScale(1f);
            Time.timeScale = s_TimeScale;
            MainToolbar.Refresh(k_ElementPath);
        }

        private static void SetForcedOverride(bool isForced)
        {
            s_ForcedOverride = isForced;
            EditorPrefs.SetBool(k_ForcedOverrideKey, s_ForcedOverride);
            MainToolbar.Refresh(k_ElementPath);
        }

        private static void SetMaxScale(float newMaxScale)
        {
            s_MaxScale = Mathf.Clamp(newMaxScale, k_MinimumMaxScale, k_MaximumMaxScale);
            EditorPrefs.SetFloat(k_MaxScaleKey, s_MaxScale);

            SetTimeScale(ClampToMax(s_TimeScale, s_MaxScale));
            Time.timeScale = s_TimeScale;
            MainToolbar.Refresh(k_ElementPath);
        }

        private static void SetTimeScale(float newTimeScale)
        {
            s_TimeScale = newTimeScale;
            EditorPrefs.SetFloat(k_TimeScaleKey, s_TimeScale);
        }

        #endregion // Private Methods
    }
}
