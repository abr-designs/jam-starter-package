using System;
using UnityEditor;
using UnityEngine;

namespace JamStarter.Editor.Scripts.Utilities
{
    public static class TagsAndLayersEditor
    {
        private const int MIN_LAYER_INDEX = 5;
        private const int MAX_TAG_COUNT = 10000;
        private const int MAX_LAYER_INDEX = 31;

        /// <summary>
        /// Adds the tag.
        /// </summary>
        /// <returns><c>true</c>, if tag was added, <c>false</c> otherwise.</returns>
        /// <param name="tagName">Tag name.</param>
        public static bool CreateTag(string tagName)
        {
            // Open tag manager
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            // Tags Property
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            if (tagsProp.arraySize >= MAX_TAG_COUNT)
            {
                Debug.Log("No more tags can be added to the Tags property. You have " + tagsProp.arraySize + " tags");
                return false;
            }

            // if not found, add it
            if (!PropertyExists(tagsProp, 0, tagsProp.arraySize, tagName))
            {
                int index = tagsProp.arraySize;
                // Insert new array element
                tagsProp.InsertArrayElementAtIndex(index);
                SerializedProperty sp = tagsProp.GetArrayElementAtIndex(index);
                // Set array element to tagName
                sp.stringValue = tagName;
                Debug.Log("Tag: " + tagName + " has been added");
                // Save settings
                tagManager.ApplyModifiedProperties();

                return true;
            }
            else
            {
                //Debug.Log ("Tag: " + tagName + " already exists");
            }

            return false;
        }

        public static string NewTag(string newTag)
        {
            CreateTag(newTag);

            if (string.IsNullOrWhiteSpace(newTag))
            {
                newTag = "Untagged";
            }

            return newTag;
        }

        /// <summary>
        /// Removes the tag.
        /// </summary>
        /// <returns><c>true</c>, if tag was removed, <c>false</c> otherwise.</returns>
        /// <param name="tagName">Tag name.</param>
        ///
        public static bool RemoveTag(string tagName)
        {

            // Open tag manager
            SerializedObject tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // Tags Property
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            if (PropertyExists(tagsProp, 0, tagsProp.arraySize, tagName))
            {
                SerializedProperty sp;

                for (int i = 0, j = tagsProp.arraySize; i < j; i++)
                {

                    sp = tagsProp.GetArrayElementAtIndex(i);
                    if (sp.stringValue == tagName)
                    {
                        tagsProp.DeleteArrayElementAtIndex(i);
                        Debug.Log("Tag: " + tagName + " has been removed");
                        // Save settings
                        tagManager.ApplyModifiedProperties();
                        return true;
                    }

                }
            }

            return false;

        }

        /// <summary>
        /// Checks to see if tag exists.
        /// </summary>
        /// <returns><c>true</c>, if tag exists, <c>false</c> otherwise.</returns>
        /// <param name="tagName">Tag name.</param>
        public static bool TagExists(string tagName)
        {
            // Open tag manager
            SerializedObject tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // Layers Property
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            return PropertyExists(tagsProp, 0, MAX_TAG_COUNT, tagName);
        }

        /// <summary>
        /// Adds the layer.
        /// </summary>
        /// <returns><c>true</c>, if layer was added, <c>false</c> otherwise.</returns>
        /// <param name="layerName">Layer name.</param>
        private static bool CreateLayer(string layerName)
        {
            // Open tag manager
            SerializedObject tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            // Layers Property
            SerializedProperty layersProp = tagManager.FindProperty("layers");
            if (!PropertyExists(layersProp, 0, MAX_LAYER_INDEX, layerName))
            {
                SerializedProperty sp;
                // Start at layer 9th index -> 8 (zero based) => first 8 reserved for unity / greyed out
                for (int i = MIN_LAYER_INDEX, j = MAX_LAYER_INDEX; i < j; i++)
                {
                    sp = layersProp.GetArrayElementAtIndex(i);
                    if (sp.stringValue == "")
                    {
                        // Assign string value to layer
                        sp.stringValue = layerName;
                        Debug.Log("Layer: " + layerName + " has been added");
                        // Save settings
                        tagManager.ApplyModifiedProperties();
                        return true;
                    }

                    if (i == j)
                        Debug.Log("All allowed layers have been filled");
                }
            }
            else
            {
                Debug.Log ("Layer: " + layerName + " already exists");
            }

            return false;
        }

        /// <summary>
        /// Adds the layer.
        /// </summary>
        /// <returns><c>true</c>, if layer was added, <c>false</c> otherwise.</returns>
        /// <param name="layerName">Layer name.</param>
        /// <param name="targetIndex"></param>
        private static bool CreateLayer(string layerName, int targetIndex)
        {
            // Open tag manager
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            // Layers Property
            SerializedProperty layersProp = tagManager.FindProperty("layers");
            
            if (!PropertyExists(layersProp, 0, MAX_LAYER_INDEX, layerName))
            {
                var sp = layersProp.GetArrayElementAtIndex(targetIndex);
                if (sp.stringValue != "") 
                    return false;
                
                // Assign string value to layer
                sp.stringValue = layerName;
                Debug.Log("Layer: " + layerName + " has been added");
                // Save settings
                tagManager.ApplyModifiedProperties();
                return true;
            }

            Debug.Log ("Layer: " + layerName + " already exists");

            return false;
        }

        public static string NewLayer(string newLayer)
        {
            if (string.IsNullOrWhiteSpace(newLayer))
            {
                CreateLayer(newLayer);
            }

            return newLayer;
        }
        
        public static string NewLayer(string newLayer, int targetIndex)
        {
            if (targetIndex < MIN_LAYER_INDEX || targetIndex > MAX_LAYER_INDEX)
                throw new IndexOutOfRangeException($"[{targetIndex}] must be > {MIN_LAYER_INDEX} & < {MAX_LAYER_INDEX}");
            
            if (string.IsNullOrWhiteSpace(newLayer))
            {
                CreateLayer(newLayer, targetIndex);
            }

            return newLayer;
        }

        /// <summary>
        /// Removes the layer.
        /// </summary>
        /// <returns><c>true</c>, if layer was removed, <c>false</c> otherwise.</returns>
        /// <param name="layerName">Layer name.</param>
        public static bool RemoveLayer(string layerName)
        {
            // Open tag manager
            SerializedObject tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // Tags Property
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            if (PropertyExists(layersProp, 0, layersProp.arraySize, layerName))
            {
                SerializedProperty sp;

                for (int i = 0, j = layersProp.arraySize; i < j; i++)
                {

                    sp = layersProp.GetArrayElementAtIndex(i);

                    if (sp.stringValue == layerName)
                    {
                        sp.stringValue = "";
                        Debug.Log("Layer: " + layerName + " has been removed");
                        // Save settings
                        tagManager.ApplyModifiedProperties();
                        return true;
                    }

                }
            }

            return false;

        }

        /// <summary>
        /// Checks to see if layer exists.
        /// </summary>
        /// <returns><c>true</c>, if layer exists, <c>false</c> otherwise.</returns>
        /// <param name="layerName">Layer name.</param>
        public static bool LayerExists(string layerName)
        {
            return LayerMask.NameToLayer(layerName) >= 0;
        }

        /// <summary>
        /// Checks if the value exists in the property.
        /// </summary>
        /// <returns><c>true</c>, if exists was propertyed, <c>false</c> otherwise.</returns>
        /// <param name="property">Property.</param>
        /// <param name="start">Start.</param>
        /// <param name="end">End.</param>
        /// <param name="value">Value.</param>
        private static bool PropertyExists(SerializedProperty property, int start, int end, string value)
        {
            for (int i = start; i < end; i++)
            {
                SerializedProperty t = property.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        /*public static void AddNewTag(string name)
        {
            CreateTag(name);
        }

        public static void DeleteTag(string name)
        {
            RemoveTag(name);
        }

        public static void AddNewLayer(string name)
        {
            CreateLayer(name);
        }

        public static void DeleteLayer(string name)
        {
            RemoveLayer(name);
        }*/
    }
}