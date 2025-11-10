using FixedColorPaletteTool;
using FixedColorPaletteTool.Importing;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts.FixedColorPaletteTool.CustomDrawers
{
    [CustomPropertyDrawer(typeof(ColorPaletteScriptableObject))]
    public class ColorPaletteScriptableObjectDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = new VisualElement { style = { flexDirection = FlexDirection.Column, } };

            return container;
            
            //var serializedObject = new SerializedObject(property);
            var paletteNameProperty = property.FindPropertyRelative(nameof(ColorPaletteScriptableObject.paletteName));

            //------------------------------------------------------------------//
            var titleLabel = new Label()
            {
                style =
                {
                    fontSize = 20,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    paddingTop = 15,
                    flexShrink = 0
                }
            };
            titleLabel.BindProperty(paletteNameProperty);
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
            var replaceButton = new Button(() => ColorPaletteImporter.ImportColorFile(FixedPaletteSettings.Instance.selectedPalette, true))
            {
                text = "Replace Colors"
            };
            buttonContainer.Add(replaceButton);
            var button = new Button(() => ColorPaletteImporter.ImportColorFile(FixedPaletteSettings.Instance.selectedPalette, false))
            {
                text = "Add Colors"
            };
            buttonContainer.Add(button);
            
            container.Add(buttonContainer);
            
            //------------------------------------------------------------------//
            var nameProperty = property.FindPropertyRelative(nameof(ColorPaletteScriptableObject.paletteName));
            var nameField = new PropertyField(nameProperty);
            nameField.style.flexShrink = 0;
            nameField.BindProperty(nameProperty);
            container.Add(nameField);
            //------------------------------------------------------------------//

            var scrollRect = new ScrollView()
            {

            };
            
            var colorsProperty = property.FindPropertyRelative(nameof(ColorPaletteScriptableObject.colors));
            var field = new PropertyField(colorsProperty);
            field.BindProperty(colorsProperty);
            scrollRect.Add(field);
            //------------------------------------------------------------------//
            
            container.Add(scrollRect);

            return container;
        }
    }
}