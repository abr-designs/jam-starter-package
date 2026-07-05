// Created by Claude (claude-opus-4-8)
// Date: 2026-06-29

using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Registers its <see cref="TMP_Text"/> with the <see cref="TextAnimator"/> while enabled and
    /// unregisters on disable. Add it to opt a label in; the &lt;anim&gt; markup selects the effect.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-29</remarks>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class AnimatedTextMarker : MonoBehaviour
    {
        //Fields
        //================================================================================================================//

        #region Fields

        // Lets the extension methods resolve a marker without a GetComponent scan. Kept in sync by the
        // OnEnable/OnDisable pair below, which fire for every add, remove, destroy & scene reload.
        private static readonly Dictionary<TMP_Text, AnimatedTextMarker> s_markersByText =
            new Dictionary<TMP_Text, AnimatedTextMarker>();

        private TMP_Text m_textComponent;

        #endregion //Fields

        //Unity Methods
        //================================================================================================================//

        #region Unity Methods

        private void Awake()
        {
            m_textComponent = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            if (m_textComponent == null)
                return;

            s_markersByText[m_textComponent] = this;
            TextAnimator.PlayTextAnimation(m_textComponent);
        }

        private void OnDisable()
        {
            if (m_textComponent == null)
                return;

            s_markersByText.Remove(m_textComponent);
            TextAnimator.StopTextAnimation(m_textComponent);
        }

        #endregion //Unity Methods

        //Internal Methods
        //================================================================================================================//

        #region Internal Methods

        // Resolves the marker on a text component via the lookup table, or null if none is registered.
        internal static AnimatedTextMarker Find(TMP_Text textComponent)
        {
            if (textComponent == null)
                return null;

            return s_markersByText.TryGetValue(textComponent, out var marker) ? marker : null;
        }

        #endregion //Internal Methods
    }
}
