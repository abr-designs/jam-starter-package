#if UNITY_EDITOR
using System;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace Utilities
{
    public class ScreenshotUtility : MonoBehaviour
    {
        private CameraScreenshotTool m_cameraScreenshotTool;

        [SerializeField] 
        private GameObject[] prefabs;

        [SerializeField]
        private Transform pedestalTransform;

        [Button]
        private void CreateThumbnails()
        {
            m_cameraScreenshotTool ??= FindAnyObjectByType<CameraScreenshotTool>();
            
            EditorUtility.DisplayProgressBar("Creating Thumbnails", "Setting up...", 0f);

            try
            {
                for (var i = 0; i < prefabs.Length; i++)
                {
                    var prefab = prefabs[i];
                    EditorUtility.DisplayProgressBar("Creating Thumbnails", 
                        $"Generating {prefab.name} thumbnail [{i + 1}/{prefabs.Length}]",
                        (i + 1) / (float)prefabs.Length);

                    var instance = Instantiate(prefab, pedestalTransform, false);
                    m_cameraScreenshotTool.CaptureScreenshot($"{prefab.name}_Thumbnail", false);

                    DestroyImmediate(instance);
                }
            }
            catch (Exception)
            {
                EditorUtility.ClearProgressBar();
                throw;
            }
            
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }
}

#endif