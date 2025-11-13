using System;
using System.Linq;
using FixedColorPaletteTool.Importing;
using Scripts.Utilities.Extensions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

using Button = UnityEngine.UIElements.Button;

namespace FixedColorPaletteTool.SettingsProviders
{
    public static class FixedPaletteSettingsProvider
    {
        private const string FORCE_INSPECTOR_SYMBOL = "FIXED_COLOR_INSPECTOR";
        
        public static event Action OnPaletteChanged;

        private static VisualElement s_paletteDrawerContainer;
        private static Button s_deletePaletteButton;
        
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

            var toggleAllInspectors = new Toggle("Use for all Color Inspectors")
            {
                value = HasScriptingDefine(FORCE_INSPECTOR_SYMBOL)
            };
            toggleAllInspectors.RegisterValueChangedCallback(a =>
            {
                if(a.newValue)
                    TryAddScriptingDefine(FORCE_INSPECTOR_SYMBOL);
                else
                    TryRemoveScriptingDefine(FORCE_INSPECTOR_SYMBOL);
                
                toggleAllInspectors.value = a.newValue;
            });

            container.Add(toggleAllInspectors);

            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.flexGrow = 1;

            var newPaletteButton = new Button(CreateNewPalette)
            {
                text = "new"
            };
            s_deletePaletteButton = new Button(DeleteCurrentPalette)
            {
                text = "Delete"
            };
            //-----------------------------------------------------------------------//
            
            var paletteProperty = serializedObject.FindProperty(nameof(FixedPaletteSettings.selectedPalette));
            var palettePropertyField = new PropertyField(paletteProperty)
            {
                style = { flexGrow = 1 }
            };
            palettePropertyField.RegisterValueChangeCallback(OnPaletteChangedEvent);
            headerContainer.Add(palettePropertyField);
            
            headerContainer.Add(newPaletteButton);
            headerContainer.Add(s_deletePaletteButton);
            container.Add(headerContainer);

            //-----------------------------------------------------------------------//

            s_paletteDrawerContainer = new VisualElement();
            OnPaletteChangedEvent(paletteProperty);

            //-----------------------------------------------------------------------//
            
            container.Add(s_paletteDrawerContainer);
            root.Add(container);
            root.Bind(serializedObject);
        }

        private static VisualElement DrawPalette(SerializedObject serializedPaletteObject)
        {
            VisualElement container = new VisualElement { style = { flexDirection = FlexDirection.Column, } };

            var paletteNameProperty = serializedPaletteObject.FindProperty(nameof(ColorPaletteScriptableObject.paletteName));
            
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
            var nameProperty = serializedPaletteObject.FindProperty(nameof(ColorPaletteScriptableObject.paletteName));
            var nameField = new PropertyField(nameProperty);
            nameField.style.flexShrink = 0;
            nameField.BindProperty(nameProperty);
            container.Add(nameField);
            
            //------------------------------------------------------------------//

            var scrollRect = new VisualElement()
            {

            };
            
            var colorsProperty = serializedPaletteObject.FindProperty(nameof(ColorPaletteScriptableObject.colors));
            var field = new PropertyField(colorsProperty)
            {
                style =
                {
                    flexGrow = 1
                }
            };
            field.BindProperty(colorsProperty);
            scrollRect.Add(field);
            //------------------------------------------------------------------//
            
            container.Add(scrollRect);

            return container;
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
        
        //============================================================================================================//

        private static bool HasScriptingDefine(string symbol)
        {
            var currentBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            //var symbols = new string[100];

            PlayerSettings.GetScriptingDefineSymbols(currentBuildTarget, out var symbols);

            return symbols.Any(x => x.Equals(symbol));
        }

        private static void TryAddScriptingDefine(string symbol)
        {
            if (HasScriptingDefine(symbol))
                return;
            
            var currentBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            
            PlayerSettings.GetScriptingDefineSymbols(currentBuildTarget, out var symbols);
            var hashedSymbols = symbols.ToHashSet();
            hashedSymbols.Add(symbol);
            
            PlayerSettings.SetScriptingDefineSymbols(currentBuildTarget, hashedSymbols.ToArray());
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
        
        private static void TryRemoveScriptingDefine(string symbol)
        {
            if (!HasScriptingDefine(symbol))
                return;
            
            var currentBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            
            PlayerSettings.GetScriptingDefineSymbols(currentBuildTarget, out var symbols);
            var hashedSymbols = symbols.ToHashSet();
            hashedSymbols.Remove(symbol);
            
            PlayerSettings.SetScriptingDefineSymbols(currentBuildTarget, hashedSymbols.ToArray());
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
        
        //============================================================================================================//

        private static void OnPaletteChangedEvent(SerializedPropertyChangeEvent evt) => OnPaletteChangedEvent(evt.changedProperty);

        private static void OnPaletteChangedEvent(SerializedProperty paletteProperty)
        {
            OnPaletteChanged?.Invoke();

            s_paletteDrawerContainer.Clear();

            var hasValue = paletteProperty.objectReferenceValue != null;
            
            s_deletePaletteButton.SetEnabled(hasValue);

            if (!hasValue)
                return;
            
            var paletteObject = new SerializedObject(paletteProperty.objectReferenceValue as ScriptableObject);
            s_paletteDrawerContainer.Add(DrawPalette(paletteObject));
        }
        
        //============================================================================================================//
        
    }
}