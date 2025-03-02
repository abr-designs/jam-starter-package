using System.IO;
using UnityEditor;
using UnityEngine;

namespace JamStarter.Editor.WebGLTemplate
{
    public class WebGLEditorWindow : EditorWindow
    {
        private static readonly string DIRECTORY_NAME = "WebGLTemplates";

        private const float previewHeight = 150f; // Height of the color preview box
        private const float progressBarWidth = 80f; // Height of the color preview box
        private const float progressBarHeight = 20f; // Height of the color preview box


        // Color settings
        private Color bgColor;
        private Color progressBorder;
        private Color progressBackground;
        private Color progressLeft;
        private Color progressRight;

        // Loading message
        private string loadingText;
        private Color loadingTextColor;
        private GUIStyle loadingTextStyle;
        private bool loadingTextNeedsUpdate = true;

        // Progress bar gradient texture
        private Texture2D progressGradientTexture;


        private static WebGLEditorWindow window;

        //Add menu named "My Window" to the Window menu
        [MenuItem("WebGL/Template Wizard")]
        private static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            window = (WebGLEditorWindow)EditorWindow.GetWindow(typeof(WebGLEditorWindow));
            window.LoadSettings();
            window.Show();
        }

        [MenuItem("WebGL/Template Wizard", true)]
        private static bool ValidateTemplateWizard()
        {
#if UNITY_6000_0_OR_NEWER
            return true;
#else
            return false;
#endif
        }

        private void LoadSettings()
        {
            if (!ColorUtility.TryParseHtmlString(PlayerSettings.GetTemplateCustomValue("BACKGROUND"), out window.bgColor))
                window.bgColor = Color.black;

            loadingText = PlayerSettings.GetTemplateCustomValue("LOAD_TEXT");
            if (loadingText.Length == 0)
                loadingText = "LOADING...";

            if (!ColorUtility.TryParseHtmlString(PlayerSettings.GetTemplateCustomValue("LOAD_TEXT_COLOR"), out window.loadingTextColor))
                window.loadingTextColor = Color.white;


            if (!ColorUtility.TryParseHtmlString(PlayerSettings.GetTemplateCustomValue("PROGRESS_BORDER_COLOR"), out window.progressBorder))
                window.progressBorder = Color.white;

            if (!ColorUtility.TryParseHtmlString(PlayerSettings.GetTemplateCustomValue("PROGRESS_BACKGROUND_COLOR"), out window.progressBackground))
                window.progressBackground = window.bgColor;
            if (!ColorUtility.TryParseHtmlString(PlayerSettings.GetTemplateCustomValue("PROGRESS_LEFT_COLOR"), out window.progressLeft))
                window.progressLeft = new Color(0, 0.1f, 0.2f, 1);
            if (!ColorUtility.TryParseHtmlString(PlayerSettings.GetTemplateCustomValue("PROGRESS_RIGHT_COLOR"), out window.progressRight))
                window.progressRight = new Color(0, 0.3f, 0.8f, 1);

            progressGradientTexture = null;

        }

        private void SaveSettings()
        {
            // Save WebGL template variables
            PlayerSettings.SetTemplateCustomValue("BACKGROUND", $"#{ColorUtility.ToHtmlStringRGBA(bgColor)}");
            PlayerSettings.SetTemplateCustomValue("LOAD_TEXT", loadingText);
            PlayerSettings.SetTemplateCustomValue("LOAD_TEXT_COLOR", $"#{ColorUtility.ToHtmlStringRGBA(loadingTextColor)}");
            PlayerSettings.SetTemplateCustomValue("PROGRESS_BORDER_COLOR", $"#{ColorUtility.ToHtmlStringRGBA(progressBorder)}");
            PlayerSettings.SetTemplateCustomValue("PROGRESS_BACKGROUND_COLOR", $"#{ColorUtility.ToHtmlStringRGBA(progressBackground)}");
            PlayerSettings.SetTemplateCustomValue("PROGRESS_LEFT_COLOR", $"#{ColorUtility.ToHtmlStringRGBA(progressLeft)}");
            PlayerSettings.SetTemplateCustomValue("PROGRESS_RIGHT_COLOR", $"#{ColorUtility.ToHtmlStringRGBA(progressRight)}");

            Debug.Log("WebGL Template settings saved.");
        }

        private void OnGUI()
        {

            DrawTemplatePreview();

            // EditorGUILayout.LabelField("Template Settings", EditorStyles.boldLabel);
            bgColor = EditorGUILayout.ColorField("Background", bgColor);
            EditorGUILayout.LabelField("Progress Bar", EditorStyles.boldLabel);
            progressBorder = EditorGUILayout.ColorField("Border", progressBorder);
            progressBackground = EditorGUILayout.ColorField("Background", progressBackground);

            EditorGUI.BeginChangeCheck();
            progressLeft = EditorGUILayout.ColorField("Left", progressLeft);
            progressRight = EditorGUILayout.ColorField("Right", progressRight);
            if (EditorGUI.EndChangeCheck())
            {
                // Clear gradient texture to be recreated
                progressGradientTexture = null;
            }

            EditorGUI.BeginChangeCheck();
            loadingText = EditorGUILayout.TextField("Loading Text", loadingText);
            loadingTextColor = EditorGUILayout.ColorField("Loading Color", loadingTextColor);
            if (EditorGUI.EndChangeCheck())
            {
                loadingTextNeedsUpdate = true;
            }


            if (GUILayout.Button("Create Template"))
            {
                CreateTemplate();
                PlayerSettings.WebGL.template = "PROJECT:JamTemplate";
                SaveSettings();

                EditorUtility.DisplayDialog("Template Saved", "JAM WebGL Template settings have been saved.  It is now set as your default template.", "OK");
                window.Close();
                SettingsService.OpenProjectSettings("Project/Player");

            }

        }

        // Show the template preview based on current settings
        private void DrawTemplatePreview()
        {
            // Get the rect for the preview box
            // Rect previewRect = GUILayoutUtility.GetRect(position.width - 20, previewHeight);
            float playerWidth = PlayerSettings.defaultScreenWidth;
            float playerHeight = PlayerSettings.defaultScreenHeight;
            float ratio = playerWidth / playerHeight;

            Rect previewRect = GUILayoutUtility.GetRect(position.width, previewHeight + 10);
            previewRect.width = previewHeight * ratio;
            previewRect.x = (position.width / 2f) - (previewRect.width / 2f);
            previewRect.y += 5;
            previewRect.height -= 10;


            // Draw background box
            EditorGUI.DrawRect(previewRect, bgColor);
            // Draw loading text
            DrawLoadingText(previewRect, loadingText);

            // Draw progess bar
            DrawProgressBar(previewRect);

        }

        private void DrawLoadingText(Rect rect, string text)
        {
            if (loadingTextNeedsUpdate)
            {
                loadingTextStyle = new GUIStyle(EditorStyles.boldLabel);
                loadingTextStyle.alignment = TextAnchor.MiddleCenter; // Center alignment
                loadingTextStyle.normal.textColor = loadingTextColor; // Set text color
                loadingTextStyle.active.textColor = loadingTextColor;
                loadingTextStyle.fontSize = 12;
                loadingTextNeedsUpdate = false;
            }

            Vector2 textSize = loadingTextStyle.CalcSize(new GUIContent(text)); // Measure text size

            // Adjust rect to center text inside
            Rect textRect = new Rect(
                rect.x + (rect.width - textSize.x) / 2,
                rect.y + (rect.height - textSize.y) / 2 - textSize.y,
                textSize.x,
                textSize.y
            );

            GUI.Label(textRect, text, loadingTextStyle);
        }

        private void DrawProgressBar(Rect rect)
        {
            Rect outR = new Rect(
                rect.x + (rect.width - progressBarWidth) / 2,
                rect.y + (rect.height - progressBarHeight) / 2 + progressBarHeight / 2,
                progressBarWidth,
                progressBarHeight
            );
            Rect bgR = new Rect(
                outR.x + 2,
                outR.y + 2,
                outR.width - 4,
                outR.height - 4
            );
            Rect insideR = new Rect(
                bgR.x + 2,
                bgR.y + 2,
                bgR.width - 4,
                bgR.height - 4
            );

            EditorGUI.DrawRect(outR, progressBorder);
            EditorGUI.DrawRect(bgR, progressBackground);

            if (progressGradientTexture == null)
            {
                progressGradientTexture = CreateGradientTexture((int)insideR.width, (int)insideR.height, progressLeft, progressRight);
            }

            GUI.DrawTexture(insideR, progressGradientTexture);

        }

        private Texture2D CreateGradientTexture(int width, int height, Color left, Color right)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int x = 0; x < width; x++)
            {
                float t = (float)x / (width - 1); // Normalized 0 to 1
                Color color = Color.Lerp(left, right, t);
                for (int y = 0; y < height; y++)
                    texture.SetPixel(x, y, color);
            }
            texture.Apply();
            return texture;
        }

        private static bool CheckAndDeleteFolder(string path)
        {
            if (Directory.Exists(path))
            {
                bool userConfirmed = EditorUtility.DisplayDialog(
                    "Delete Template?",
                    $"The template ({path}) already exists. Do you want to delete it?",
                    "Delete",
                    "Cancel"
                );

                if (userConfirmed)
                {
                    // Delete the folder
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.Refresh(); // Refresh to reflect the changes
                }

                return userConfirmed;
            }
            else
            {
                return true;
            }
        }

        private static void CreateTemplate()
        {
            var sourcePath = Path.GetFullPath("Packages/com.abrds.jam-starter/WebGLTemplates~");
            var destinationPath = Path.GetFullPath($"Assets/{DIRECTORY_NAME}");

            if (Directory.Exists(destinationPath))
                return;
            // if (!CheckAndDeleteFolder($"Assets/{DIRECTORY_NAME}"))
            // return;

            Directory.CreateDirectory(destinationPath);

            CopyDirectory(sourcePath, destinationPath, true);

            AssetDatabase.Refresh();
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (!recursive)
                return;

            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }


    }
}