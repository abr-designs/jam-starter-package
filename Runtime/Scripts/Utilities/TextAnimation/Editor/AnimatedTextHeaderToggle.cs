// Created by Claude (claude-opus-4-8)
// Date: 2026-06-29

using TMPro;
using UnityEditor;
using UnityEngine;

namespace Utilities.TextAnimation.Editor
{
    /// <summary>
    /// Adds an "Animate Text" toggle to the GameObject header of TMP text objects via
    /// <see cref="UnityEditor.Editor.finishedDefaultHeaderGUI"/>, toggling an
    /// <see cref="AnimatedTextMarker"/> without replacing TMP's editor.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-29</remarks>
    [InitializeOnLoad]
    internal static class AnimatedTextHeaderToggle
    {
        //Constructors
        //================================================================================================================//

        #region Constructors

        static AnimatedTextHeaderToggle()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnHeaderGUI;
        }

        #endregion //Constructors

        //Private Methods
        //================================================================================================================//

        #region Private Methods

        private static void OnHeaderGUI(UnityEditor.Editor editor)
        {
            // The header hook targets the GameObject, not its components. Only draw when every selected
            // object is a GameObject carrying a TMP_Text, so the add/remove below stays unambiguous.
            for (int i = 0; i < editor.targets.Length; i++)
            {
                var gameObject = editor.targets[i] as GameObject;
                if (gameObject == null || gameObject.GetComponent<TMP_Text>() == null)
                    return;
            }

            var hasMarker = ((GameObject)editor.target).GetComponent<AnimatedTextMarker>() != null;

            EditorGUI.BeginChangeCheck();
            var animate = EditorGUILayout.ToggleLeft("Animate Text", hasMarker);
            if (EditorGUI.EndChangeCheck() == false)
                return;

            for (int i = 0; i < editor.targets.Length; i++)
            {
                var gameObject = (GameObject)editor.targets[i];
                var marker = gameObject.GetComponent<AnimatedTextMarker>();

                if (animate && marker == null)
                    Undo.AddComponent<AnimatedTextMarker>(gameObject);
                else if (animate == false && marker != null)
                    Undo.DestroyObjectImmediate(marker);
            }
        }

        #endregion //Private Methods
    }
}
