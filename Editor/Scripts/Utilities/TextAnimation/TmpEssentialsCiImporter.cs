// Created by Claude (claude-opus-4-8)
// Date: 2026-06-30

#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace JamStarter.Editor.TextAnimation
{
    /// <summary>
    /// Imports the TMP Essential Resources during headless CI so the mesh-dependent text-animation
    /// tests have a default font to build a real mesh from. Runs only in batch mode & only when the
    /// essentials are missing, so it never touches a developer's editor session.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-30</remarks>
    [InitializeOnLoad]
    internal static class TmpEssentialsCiImporter
    {
        // Canonical marker the importer window itself checks to decide whether essentials are present.
        private const string k_EssentialsMarkerPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        static TmpEssentialsCiImporter()
        {
            if (Application.isBatchMode == false)
                return;

            // A prior run in this session may have already imported the essentials.
            if (File.Exists(k_EssentialsMarkerPath))
                return;

            // TMP_PackageResourceImporter & ImportResources are both public, so call them directly.
            // ImportResources(importEssentials, importExamples, interactive): essentials only, silent.
            TMP_PackageResourceImporter.ImportResources(true, false, false);

            Debug.Log("Requested TMP Essential Resources import for the CI test run.");
        }
    }
}
#endif
