using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FixedColorPaletteTool.Preprocessors
{
    //FIXME I need to ensure that the required sub-assets get included & the others are excluded to save memory
    public class BuildPreProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        // List the assets you want to force-include (relative to project root)
        // You can edit these paths or load them from somewhere else if you'd like.
        private static readonly string[] SourcePaths =
        {
            FixedPaletteSettings.AssetPath,
        };
        
        // Temporary resources folder where assets will be copied for the build
        private const string TempResourcesFolder = "Assets/ForceIncludedResources";
        
        // Inside a Resources folder to guarantee inclusion
        // still under Assets/, but we will create a Resources folder inside if needed
        private const string TempResourcesResourcesSubfolder = TempResourcesFolder + "/ResourcesForBuild"; 
        
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("[ForceInclude] Preprocess build — copying forced assets into temporary Resources folder.");

            // Ensure temp folder exists
            if (!AssetDatabase.IsValidFolder(TempResourcesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "ForceIncludedResources");
            }

            // Make a Resources subfolder so Unity packs it under Resources
            string resourcesPath = $"{TempResourcesFolder}/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder(TempResourcesFolder, "Resources");
            }

            foreach (var src in SourcePaths)
            {
                if (!File.Exists(src))
                {
                    Debug.LogWarning($"[ForceInclude] Source asset not found: {src}");
                    continue;
                }

                // Destination: Assets/ForceIncludedResources/Resources/<filename>.asset
                string fileName = Path.GetFileName(src);
                string dest = $"{resourcesPath}/{fileName}";

                // If dest already exists, delete it first (avoid copy failure)
                if (AssetDatabase.LoadAssetAtPath<Object>(dest) != null)
                {
                    bool deleted = AssetDatabase.DeleteAsset(dest);
                    if (!deleted)
                    {
                        Debug.LogWarning($"[ForceInclude] Failed to delete existing asset at {dest} before copying.");
                        continue;
                    }
                }

                bool success = AssetDatabase.CopyAsset(src, dest);
                if (!success)
                {
                    Debug.LogWarning($"[ForceInclude] Failed to copy {src} -> {dest}");
                    continue;
                }

                Debug.Log($"[ForceInclude] Copied {src} -> {dest}");
            }

            // Ensure AssetDatabase knows about the new files
            AssetDatabase.Refresh();
        }
        
        // Called after the build completes (successful or not)
        public void OnPostprocessBuild(BuildReport report)
        {
            Debug.Log("[ForceInclude] Postprocess build — cleaning up temporary resources.");

            // Remove copied assets and the temp folder if empty
            string resourcesPath = $"{TempResourcesFolder}/Resources";

            if (AssetDatabase.IsValidFolder(resourcesPath))
            {
                // Enumerate files and delete them
                var guids = AssetDatabase.FindAssets("", new[] { resourcesPath });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    // Don't try to delete meta files directly; AssetDatabase.DeleteAsset handles meta pairs.
                    bool deleted = AssetDatabase.DeleteAsset(path);
                    if (deleted)
                    {
                        Debug.Log($"[ForceInclude] Deleted temp asset: {path}");
                    }
                }

                // Delete the Resources folder if empty
                // AssetDatabase.DeleteAsset can delete folders as well
                AssetDatabase.DeleteAsset(resourcesPath);
            }

            // Delete the parent temp folder if empty
            if (AssetDatabase.IsValidFolder(TempResourcesFolder))
            {
                // If folder is empty, DeleteAsset will remove it; otherwise it'll fail silently.
                AssetDatabase.DeleteAsset(TempResourcesFolder);
            }

            AssetDatabase.Refresh();
        }
    }
}