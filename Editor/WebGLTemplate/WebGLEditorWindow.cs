using System.IO;
using UnityEditor;
using UnityEngine;

namespace JamStarter.Editor.WebGLTemplate
{
    public class WebGLEditorWindow : EditorWindow
    {
        private static readonly string DIRECTORY_NAME = "WebGLTemplates";
        
        //private static WebGLEditorWindow window;
        /*string myString = "Hello World";
        bool groupEnabled;
        bool myBool = true;
        float myFloat = 1.23f;*/

        // Add menu named "My Window" to the Window menu
        /*[MenuItem("WebGL/Template Wizard")]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            window = (WebGLEditorWindow)EditorWindow.GetWindow(typeof(WebGLEditorWindow));
            window.Show();
        }*/
        
        /*private void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            /*myString = EditorGUILayout.TextField("Text Field", myString);

            groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
            myBool = EditorGUILayout.Toggle("Toggle", myBool);
            myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);#1#
            EditorGUILayout.EndToggleGroup();
        }*/
        
        [MenuItem("WebGL/Create Template")]
        private static void CreateTemplate()
        {
            var sourcePath =   Path.GetFullPath("Packages/com.abrds.jam-starter/WebGLTemplates~");
            var destinationPath =   Path.GetFullPath($"Assets/{DIRECTORY_NAME}");

            if (Directory.Exists(destinationPath))
                return;

            Directory.CreateDirectory(destinationPath);


            CopyDirectory(sourcePath, destinationPath, true);

            AssetDatabase.Refresh();
        }
        
        [MenuItem("WebGL/Create Template", true)]
        private static bool ValidateCreateTemplate()
        {
#if UNITY_6000_0_OR_NEWER
            return true;
#else
            return false;
#endif
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