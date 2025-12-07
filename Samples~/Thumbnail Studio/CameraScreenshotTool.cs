#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Utilities
{
    [ExecuteInEditMode]
    public class CameraScreenshotTool : MonoBehaviour
    {
        [Header("Screenshot Settings")] public Camera targetCamera;
        public string outputDirectory = "Screenshots";

        public int resolutionWidth = 512;
        public int resolutionHeight = 512;

        [ContextMenu("Capture Screenshot")]
        public Texture2D CaptureScreenshot(string filename, bool refreshOnSuccess)
        {
            if (targetCamera == null)
            {
                Debug.LogError("CameraScreenshotTool: No camera assigned.");
                return null;
            }

            var path = Path.Join(Application.dataPath, outputDirectory);

            // Ensure directory exists
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Create a temporary RenderTexture
            RenderTexture rt = new RenderTexture(resolutionWidth, resolutionHeight, 24);
            targetCamera.targetTexture = rt;

            // Render camera to texture
            targetCamera.Render();

            // Copy to Texture2D
            RenderTexture.active = rt;
            Texture2D screenshot = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGBA32, false);
            screenshot.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
            screenshot.Apply();

            // Cleanup
            targetCamera.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);

            // Save as PNG
            string fullPath = Path.Combine(path, $"{filename}.png");
            byte[] bytes = screenshot.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);

            Debug.Log($"Screenshot saved to: {fullPath}");
            
            if(refreshOnSuccess)
                AssetDatabase.Refresh();
            

            return screenshot;
        }
    }
}
#endif