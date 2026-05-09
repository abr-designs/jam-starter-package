// Created by Claude (claude-sonnet-4-6)
// Date: 2026-05-06
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FixedColorPaletteTool
{
#if FIXED_COLOR_INSPECTOR
    [InitializeOnLoad]
    internal static class ColorPickerInterceptor
    {
        private static readonly FieldInfo s_InstanceField;       // static ColorPicker s_Instance
        private static readonly FieldInfo s_CallbackField;       // Action<Color> m_ColorChangedCallback
        private static readonly FieldInfo s_DelegateViewField;   // GUIView m_DelegateView
        private static readonly PropertyInfo s_ColorProperty;    // static Color color { get; set; }
        private static readonly MethodInfo s_SendEventMethod;    // GUIView.SendEvent(Event e)

        private static readonly bool s_ReflectionOk;
        private static bool s_Intercepting;
        private static EditorWindow s_OffscreenPicker;  // kept alive for GUIView path
        private static EditorWindow s_DetectedPicker;   // picker seen last frame; wait one frame before intercepting

        static ColorPickerInterceptor()
        {
            var editorAsm       = typeof(EditorWindow).Assembly;
            var colorPickerType = editorAsm.GetType("UnityEditor.ColorPicker");
            var guiViewType     = editorAsm.GetType("UnityEditor.GUIView");

            if (colorPickerType == null)
            {
                Debug.LogWarning("[FixedPalette] UnityEditor.ColorPicker not found — color picker interception disabled.");
                return;
            }

            var instFlags  = BindingFlags.Instance | BindingFlags.NonPublic;
            var staticPriv = BindingFlags.Static   | BindingFlags.NonPublic;
            var staticPub  = BindingFlags.Static   | BindingFlags.Public;

            s_InstanceField     = colorPickerType.GetField("s_Instance",             staticPriv);
            s_CallbackField     = colorPickerType.GetField("m_ColorChangedCallback", instFlags);
            s_DelegateViewField = colorPickerType.GetField("m_DelegateView",         instFlags);
            s_ColorProperty     = colorPickerType.GetProperty("color",               staticPub);
            s_SendEventMethod   = guiViewType?.GetMethod("SendEvent",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, new[] { typeof(Event) }, null);

            s_ReflectionOk = s_InstanceField  != null
                          && s_CallbackField  != null
                          && s_ColorProperty  != null;

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
                }
                return;
            }

            if (!HasActivePalette()) return;

            var instance = s_InstanceField.GetValue(null) as EditorWindow;
            if (instance == null || !instance)
            {
                s_DetectedPicker = null;
                return;
            }

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

            var callback     = s_CallbackField.GetValue(instance) as Action<Color>;
            var delegateView = s_DelegateViewField?.GetValue(instance);
            var pickerPos    = instance.position;

            if (callback != null)
            {
                // Callback path — color is delivered via delegate, picker not needed after
                instance.Close();
            }
            else
            {
                // GUIView path — inspector reads ColorPicker.color synchronously inside
                // SendEvent, so the picker must stay alive until that call completes.
                // Move it off-screen so it's invisible; closed in the selected callback.
                instance.position = new Rect(-10000, -10000, pickerPos.width, pickerPos.height);
                s_OffscreenPicker = instance;
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
                    else if (delegateView != null && s_SendEventMethod != null)
                    {
                        // Picker is still alive (off-screen) — SetValue calls SetColor which
                        // needs the native object intact. Inspector reads color synchronously
                        // inside SendEvent before we close.
                        s_ColorProperty.SetValue(null, chosenColor);
                        var evt = EditorGUIUtility.CommandEvent("ColorPickerChanged");
                        s_SendEventMethod.Invoke(delegateView, new object[] { evt });
                        CloseOffscreenPicker();
                    }

                    s_Intercepting = false;
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
                s_OffscreenPicker.Close();
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
