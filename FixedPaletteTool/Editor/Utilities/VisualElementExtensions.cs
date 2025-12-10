namespace FixedColorPaletteTool
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    internal static class VisualElementExtensions
    {
        internal static Rect GetScreenBound(this VisualElement element)
        {
            // Get the worldBound (position within the current EditorWindow)
            var worldBound = element.worldBound;

            // Get the editor window this element belongs to
            var window = element.panel?.contextType == ContextType.Editor
                ? EditorWindow.focusedWindow
                : null;

            if (window == null)
            {
                Debug.LogWarning("Could not determine the EditorWindow for the VisualElement.");
                return worldBound;
            }

            // Convert the window’s position (top-left corner) to screen coordinates
            var windowPos = GUIUtility.GUIToScreenPoint(Vector2.zero);
            var windowTopLeft = new Vector2(window.position.x, window.position.y);

            // Now add the window’s position to the element’s window-relative bounds
            return new Rect(
                worldBound.x + window.position.x,
                worldBound.y + window.position.y,
                worldBound.width,
                worldBound.height
            );
        }
    }

}