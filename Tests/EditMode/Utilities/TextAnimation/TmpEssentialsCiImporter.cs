// Created by Claude (claude-opus-4-8)
// Date: 2026-06-30

#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Tests.Utilities.TextAnimation
{
    /// <summary>
    /// Imports the TMP Essential Resources during headless CI so the mesh-dependent text-animation
    /// tests have a default font to build a real mesh from. Runs only in batch mode & only when the
    /// essentials are missing, so it never touches a developer's editor session. It lives in the
    /// editor test assembly, so it ships with the tests rather than the package.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-30</remarks>
    [InitializeOnLoad]
    internal static class TmpEssentialsCiImporter
    {
        static TmpEssentialsCiImporter()
        {
            if (Application.isBatchMode == false)
                return;

            // A populated TMP_Settings means a prior run already imported the essentials.
            if (TMP_Settings.instance != null)
                return;

            ImportEssentials();
        }

        private static void ImportEssentials()
        {
            // TMP_PackageResourceImporter is internal, so reach it the same way the importer window does:
            // resolve the type by name across loaded assemblies & invoke ImportResources via reflection.
            var importerType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("TMPro.TMP_PackageResourceImporter"))
                .FirstOrDefault(type => type != null);

            if (importerType == null)
            {
                Debug.LogWarning("TMP essentials CI import skipped: TMP_PackageResourceImporter type not found.");
                return;
            }

            var importMethod = importerType.GetMethod(
                "ImportResources",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (importMethod == null)
            {
                Debug.LogWarning("TMP essentials CI import skipped: ImportResources method not found.");
                return;
            }

            var importer = Activator.CreateInstance(importerType, nonPublic: true);

            // ImportResources(importEssentials, importExamples, ...): the trailing flag varies by TMP
            // version, so size the argument list from the signature & default everything but essentials off.
            var parameterCount = importMethod.GetParameters().Length;
            var arguments = parameterCount >= 3
                ? new object[] { true, false, false }
                : new object[] { true, false };

            importMethod.Invoke(importer, arguments);
            AssetDatabase.Refresh();

            Debug.Log("Imported TMP Essential Resources for the CI test run.");
        }
    }
}
#endif
