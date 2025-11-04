using Scripts.Utilities.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Utilities.Debugging;
using Button = UnityEngine.UIElements.Button;

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
                activateHandler = ActivateHandler
            };
        }

        private static void ActivateHandler(string _, VisualElement root)
        {
            SerializedObject serializedObject = FixedPaletteSettings.GetSerializedObject();

            VisualElement container = new VisualElement { style = { flexDirection = FlexDirection.Column, } };

            container.Add(new Label("Fixed Palette Settings") { style = { fontSize = 24, unityFontStyleAndWeight = FontStyle.Bold } });
            container.style.SetMargins(10);

            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.flexGrow = 1;

            var newPaletteButton = new Button(CreateNewPalette)
            {
                text = "new"
            };
            var deletePaletteButton = new Button(DeleteCurrentPalette)
            {
                text = "Delete"
            };
            //-----------------------------------------------------------------------//
            
            var paletteProperty = serializedObject.FindProperty(nameof(FixedPaletteSettings.selectedPalette));
            var palettePropertyField = new PropertyField(paletteProperty)
            {
                style = { flexGrow = 1 }
            };
            headerContainer.Add(palettePropertyField);
            
            headerContainer.Add(newPaletteButton);
            headerContainer.Add(deletePaletteButton);
            container.Add(headerContainer);

            //-----------------------------------------------------------------------//

            if (FixedPaletteSettings.Instance.selectedPalette != null) 
                DrawPalette(container, FixedPaletteSettings.Instance.selectedPalette);

            //-----------------------------------------------------------------------//
            root.Add(container);
            root.Bind(serializedObject);
        }

        private static void DrawPalette(VisualElement container, ColorPaletteScriptableObject myPalette)
        {
            var serializedObject = new SerializedObject(myPalette);

            //------------------------------------------------------------------//
            var titleLabel = new Label(myPalette.paletteName)
            {
                bindingPath = nameof(ColorPaletteScriptableObject.paletteName),
                style =
                {
                    fontSize = 20,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    paddingTop = 15,
                    flexShrink = 0
                }
            };
            titleLabel.Bind(serializedObject);
            container.Add(titleLabel);

            var buttonContainer = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexShrink = 0
                }
            };

            //------------------------------------------------------------------//
            var replaceButton = new Button(() => ColorPaletteImporter.ImportColorFile(myPalette, true))
            {
                text = "Replace Colors"
            };
            buttonContainer.Add(replaceButton);
            var button = new Button(() => ColorPaletteImporter.ImportColorFile(myPalette, false))
            {
                text = "Add Colors"
            };
            buttonContainer.Add(button);
            
            container.Add(buttonContainer);
            
            //------------------------------------------------------------------//
            var nameProperty = serializedObject.FindProperty(nameof(ColorPaletteScriptableObject.paletteName));
            var nameField = new PropertyField(nameProperty);
            nameField.style.flexShrink = 0;
            nameField.Bind(serializedObject);
            container.Add(nameField);
            //------------------------------------------------------------------//

            var scrollRect = new ScrollView()
            {

            };
            
            var colorsProperty = serializedObject.FindProperty(nameof(ColorPaletteScriptableObject.colors));
            var field = new PropertyField(colorsProperty);
            field.Bind(serializedObject);
            scrollRect.Add(field);
            //------------------------------------------------------------------//
            
            container.Add(scrollRect);
        }

        private static void CreateNewPalette()
        {
            var selectedOption = EditorUtility.DisplayDialogComplex(
                "Create Palette File",
                "Did you want to create a blank file or import a palette?",
                "Import",
                "Cancel",
                "Create Blank");

            //On Cancel, exit
            if (selectedOption == 1)
                return;

            var newPalette = FixedPaletteSettings.AddNewPalette();

            if (selectedOption == 2)
                return;
            
            ColorPaletteImporter.ImportColorFile(newPalette, true);
        }

        private static void DeleteCurrentPalette()
        {
            var palette = FixedPaletteSettings.Instance.selectedPalette;
            
            if(palette == null)
                return;

            if (!EditorUtility.DisplayDialog($"Delete {palette.paletteName}?", "Delete the currently selected palette?",
                    "Delete", "Cancel"))
                return;
            
            FixedPaletteSettings.DeletePalette(palette);
        }
    }
}