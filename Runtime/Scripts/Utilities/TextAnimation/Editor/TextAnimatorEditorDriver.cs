// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Utilities.TextAnimation.Editor
{
    /// <summary>
    /// Ticks marker-registered text animations in edit mode. The runtime <see cref="TextAnimator"/> ticks
    /// from the PlayerLoop, which only runs in Play mode, so this editor-only driver ticks from
    /// <see cref="EditorApplication.update"/> using a realtime clock and repaints the views each frame.
    /// Discovery is owned by <see cref="AnimatedTextMarker"/> via <c>ExecuteAlways</c>; the driver only
    /// advances and repaints whatever the markers have registered.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    [InitializeOnLoad]
    internal static class TextAnimatorEditorDriver
    {
        //Constructors
        //================================================================================================================//

        #region Constructors

        static TextAnimatorEditorDriver()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        #endregion //Constructors

        //Private Methods
        //================================================================================================================//

        #region Private Methods

        private static void OnEditorUpdate()
        {
            if (Application.isPlaying)
                return;

            if (TextAnimator.HasActiveTexts == false)
                return;

            TextAnimator.TickAll(Time.realtimeSinceStartup);
            InternalEditorUtility.RepaintAllViews();
        }

        #endregion //Private Methods
    }
}
