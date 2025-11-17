using UnityEditor;
using UnityEngine;

namespace FixedColorPaletteTool.CustomDrawers
{
    [CustomPropertyDrawer(typeof(ColorData))]
    internal class ColorDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Remove indent so it lines up nicely
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Get sub-properties
            var nameProp = property.FindPropertyRelative(nameof(ColorData.name));
            var colorProp = property.FindPropertyRelative(nameof(ColorData.color));
            var colorTypeProp = property.FindPropertyRelative(nameof(ColorData.colorType));

            // Calculate field widths
            float colorWidth = 120f;
            float colorTypeDropdownWidth = 100f;
            float spacing = 5f;
            float nameWidth = position.width - colorWidth - colorTypeDropdownWidth - (spacing * 2f);

            // Draw inline fields
            Rect colorRect = new Rect(position.x, position.y, colorWidth, position.height);
            Rect colorTypeRect = new Rect(position.x + colorWidth + spacing, 
                position.y, 
                colorTypeDropdownWidth, position.height);
            Rect nameRect = new Rect(position.x + colorWidth + colorTypeDropdownWidth + (spacing * 2f),
                position.y, nameWidth, position.height);

            //Here we have to explicitly draw a ColorField, otherwise, if the user decides to use the override all
            //colors option, this would not function as expected!
            colorProp.colorValue = EditorGUI.ColorField(colorRect, colorProp.colorValue);
            DrawColorTypeDropdown(colorTypeRect, colorTypeProp);
            EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);

            EditorGUI.indentLevel = oldIndent;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        //============================================================================================================//
        
        #region Color Type Dropdown

        private static readonly string [] EnumLabels =
        {
            "",
            "Primary",
            "Secondary",
            "Tertiary"
        };
        
        private static void DrawColorTypeDropdown(Rect rect, SerializedProperty property)
        {
            EditorGUI.BeginProperty(rect, new GUIContent(), property);

            // Current index
            int current = property.enumValueIndex;

            // Button showing current label
            string display = EnumLabels[Mathf.Clamp(current, 0, EnumLabels.Length - 1)];
            if (GUI.Button(rect, display, EditorStyles.popup))
            {
                ShowMenu(rect, property, current);
            }

            EditorGUI.EndProperty();

            return;
            
            void ShowMenu(Rect position, SerializedProperty property, int current)
            {
                var menu = new GenericMenu();

                for (int i = 0; i < EnumLabels.Length; i++)
                {
                    int index = i;
                    string label = string.IsNullOrEmpty(EnumLabels[i]) ? "(None)" : EnumLabels[i];

                    menu.AddItem(
                        new GUIContent(label),
                        index == current,
                        () =>
                        {
                            // Toggle-off logic:
                            // selecting the same value again → reset to NONE (0)
                            if (index == current)
                                property.enumValueIndex = 0; // NONE
                            else
                                property.enumValueIndex = index;

                            property.serializedObject.ApplyModifiedProperties();
                        });
                }

                menu.DropDown(position);
            }
        }

        #endregion //Color Type Dropdown
        
        //============================================================================================================//
    }

}