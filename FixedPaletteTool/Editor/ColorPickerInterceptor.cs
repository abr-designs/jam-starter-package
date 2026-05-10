// Created by Claude (claude-sonnet-4-6)
// Date: 2026-05-06
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FixedColorPaletteTool
{
    // Runs unconditionally to repair the "CPickerHeight" EditorPref if it was
    // corrupted to 0 by an earlier version of ColorPickerInterceptor that collapsed
    // the picker's size before closing it. A 0-height pref prevents ColorPicker
    // from ever rendering (SetHeight is gated on rect.height > 0 in OnGUI).
    [InitializeOnLoad]
    internal static class ColorPickerHeightPrefGuard
    {
        private const string k_HeightPrefKey = "CPickerHeight";
        private const int k_MinValidHeight = 50;

        static ColorPickerHeightPrefGuard()
        {
            var saved = EditorPrefs.GetInt(k_HeightPrefKey, k_MinValidHeight);
            if (saved < k_MinValidHeight)
                EditorPrefs.DeleteKey(k_HeightPrefKey);
        }
    }

#if FIXED_COLOR_INSPECTOR
    [InitializeOnLoad]
    internal static class ColorPickerInterceptor
    {
        private static readonly FieldInfo s_InstanceField;    // static ColorPicker s_Instance
        private static readonly FieldInfo s_CallbackField;    // Action<Color> m_ColorChangedCallback
        private static readonly PropertyInfo s_ColorProperty; // static Color color { get; set; }

        private static readonly bool s_ReflectionOk;
        private static bool s_Intercepting;
        private static EditorWindow s_OffscreenPicker;      // kept alive for GUIView path
        private static Vector2 s_PickerOriginalMinSize;     // saved before we collapse picker to zero
        private static Vector2 s_PickerOriginalMaxSize;
        private static Rect s_PickerOriginalPosition;       // saved before we move picker off-screen
        private static EditorWindow s_DetectedPicker;       // picker seen last frame; wait one frame before intercepting
        private static EditorWindow s_InterceptedPicker;    // instance we already handled; skip re-detection until cleared

        static ColorPickerInterceptor()
        {
            var editorAsm       = typeof(EditorWindow).Assembly;
            var colorPickerType = editorAsm.GetType("UnityEditor.ColorPicker");

            if (colorPickerType == null)
            {
                Debug.LogWarning("[FixedPalette] UnityEditor.ColorPicker not found — color picker interception disabled.");
                return;
            }

            s_InstanceField = colorPickerType.GetField("s_Instance",             BindingFlags.Static   | BindingFlags.NonPublic);
            s_CallbackField = colorPickerType.GetField("m_ColorChangedCallback", BindingFlags.Instance | BindingFlags.NonPublic);
            s_ColorProperty = colorPickerType.GetProperty("color",               BindingFlags.Static   | BindingFlags.Public);

            s_ReflectionOk = s_InstanceField != null
                          && s_CallbackField != null
                          && s_ColorProperty != null;

            if (s_ReflectionOk)
                EditorApplication.update += OnUpdate;
            else
                Debug.LogWarning("[FixedPalette] ColorPicker reflection incomplete — color picker interception disabled.");
        }

        private static void OnUpdate()
        {
            if (s_Intercepting)
            {
                if (Resources.FindObjectsOfTypeAll<ColorSelectDropdownWindow>().Length == 0)
                {
                    // Palette dismissed without selection — close any off-screen picker
                    CloseOffscreenPicker();
                    s_Intercepting = false;
                    s_InterceptedPicker = null;
                }
                return;
            }

            if (!HasActivePalette()) return;

            var instance = s_InstanceField.GetValue(null) as EditorWindow;
            if (instance == null || !instance)
            {
                s_DetectedPicker = null;
                s_InterceptedPicker = null;
                return;
            }

            // Skip the instance we already intercepted — it may still be alive
            // (GUIView path) or closing asynchronously (callback path). A new picker
            // will be a different object reference.
            if (instance == s_InterceptedPicker) return;

            // Wait one frame after first detecting the picker before intercepting.
            // ShowAsDropDown fails silently when called on the same frame the picker
            // first appears; one settled frame is enough to avoid this.
            if (s_DetectedPicker != instance)
            {
                s_DetectedPicker = instance;
                return;
            }
            s_DetectedPicker = null;

            Color currentColor;
            try   { currentColor = (Color)s_ColorProperty.GetValue(null); }
            catch { return; }

            s_Intercepting = true;
            s_InterceptedPicker = instance;

            var callback  = s_CallbackField.GetValue(instance) as Action<Color>;
            var pickerPos = instance.position;

            if (callback != null)
            {
                // Callback path — color is delivered via delegate, picker not needed after
                instance.Close();
            }
            else
            {
                // GUIView path — ColorPicker.color setter calls SetColor → OnColorChanged →
                // m_DelegateView.SendEvent, so the picker must stay alive until SetValue is
                // called. Collapse to zero size; Unity 6 clamps position so off-screen alone
                // won't hide it. Save all original dimensions to restore before closing so
                // neither EditorPrefs ("CPickerHeight") nor the layout file saves bad state.
                s_PickerOriginalMinSize  = instance.minSize;
                s_PickerOriginalMaxSize  = instance.maxSize;
                s_PickerOriginalPosition = instance.position;
                instance.minSize     = Vector2.zero;
                instance.maxSize     = Vector2.zero;
                instance.position    = new Rect(-10000, -10000, 0, 0);
                s_OffscreenPicker    = instance;
            }

            var colorSelectMode = FixedPaletteSettings.Instance.materialColorSelect;
            var window = ScriptableObject.CreateInstance<ColorSelectDropdownWindow>();

            window.Init(
                colorSelectMode,
                FixedPaletteSettings.Instance.selectedPalette.colors,
                new ColorData((Color32)currentColor),
                selected =>
                {
                    var chosenColor = (Color)(Color32)selected.color;

                    if (callback != null)
                    {
                        callback(chosenColor);
                    }
                    else
                    {
                        // ColorPicker.color setter calls SetColor → OnColorChanged →
                        // m_DelegateView.SendEvent (notifies inspector) → GUIUtility.ExitGUI().
                        // ExitGUI throws ExitGUIException, wrapped by reflection in
                        // TargetInvocationException. The color is set and the inspector is
                        // notified before the throw, so we catch and ignore it.
                        try
                        {
                            s_ColorProperty.SetValue(null, chosenColor);
                        }
                        catch (ExitGUIException) { }
                        catch (TargetInvocationException e) when (e.InnerException is ExitGUIException) { }

                        CloseOffscreenPicker();
                    }

                    s_Intercepting = false;
                    // Do NOT null s_InterceptedPicker here — let OnUpdate clear it once the
                    // picker is confirmed destroyed. Nulling early allows re-detection of a
                    // still-alive off-screen picker on the very next frame.
                }
            );

            ColorSelectDropdownWindow.GetExpectedSize(colorSelectMode, out var width, out var height);
            // Anchor at the picker's original screen position with a 1px-tall strip so
            // ShowAsDropDown places the palette just below where the picker appeared,
            // which is close to the colour field that was clicked.
            window.ShowAsDropDown(new Rect(pickerPos.x, pickerPos.y, pickerPos.width, 1), new Vector2(width, height));
        }

        private static void CloseOffscreenPicker()
        {
            if (s_OffscreenPicker != null && s_OffscreenPicker)
            {
                // Restore original dimensions before closing so that:
                // 1. OnDisable saves the correct height to EditorPrefs ("CPickerHeight").
                // 2. The layout system saves the original position rather than (-10000,-10000),
                //    ensuring the next ShowAuxWindow call places the picker correctly.
                s_OffscreenPicker.minSize  = s_PickerOriginalMinSize;
                s_OffscreenPicker.maxSize  = s_PickerOriginalMaxSize;
                s_OffscreenPicker.position = s_PickerOriginalPosition;
                s_OffscreenPicker.Close();
            }
            s_OffscreenPicker = null;
        }

        private static bool HasActivePalette()
        {
            var palette = FixedPaletteSettings.Instance?.selectedPalette;
            return palette != null && palette.colors is { Count: > 0 };
        }
    }
#endif
}
