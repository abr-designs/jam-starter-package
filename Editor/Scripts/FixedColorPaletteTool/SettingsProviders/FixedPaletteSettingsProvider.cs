using Scripts.Utilities.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities.Debugging;

namespace FixedColorPaletteTool.SettingsProviders
{
    public static class FixedPaletteSettingsProvider 
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Project/Fixed Palette Settings", SettingsScope.Project)
            {
                label = "Fixed Palette Settings",
                activateHandler = (_, root) =>
                {
                    SerializedObject serializedObject = FixedPaletteSettings.GetSerializedObject();

                    VisualElement container = new VisualElement
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Column,
                        }
                    };
                    
                    container.Add(new Label("Fixed Palette Settings")
                    {
                        style =
                        {
                            fontSize = 24,
                            unityFontStyleAndWeight = FontStyle.Bold
                        }
                    });
                    container.style.SetMargins(10);

                    var paletteProperty = serializedObject.FindProperty(nameof(FixedPaletteSettings.selectedPalette));
                    container.Add(new PropertyField(paletteProperty));

                    if (FixedPaletteSettings.Instance.selectedPalette != null)
                        DrawPalette(container, FixedPaletteSettings.Instance.selectedPalette);
                    
                    
                    

                    root.Add(container);
                    root.Bind(serializedObject);
                }
            };
        }

        private static void DrawPalette(VisualElement container, ColorPaletteScriptableObject myPalette)
        {
            SerializedObject serializedObject = new SerializedObject(myPalette);
            
            container.Add(new Label(myPalette.name)
            {
                style =
                {
                    fontSize = 20,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    paddingTop = 15,
                }
            });
            
            var colorsProperty = serializedObject.FindProperty(nameof(ColorPaletteScriptableObject.colors));
            var field = new PropertyField(colorsProperty);
            field.Bind(serializedObject);
            container.Add(field);
        }
    }
}