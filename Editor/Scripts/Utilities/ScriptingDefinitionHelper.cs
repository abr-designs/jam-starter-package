using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using ZLinq;

namespace JamStarter.Editor.Scripts.Utilities
{
    [InitializeOnLoad]
    internal static class ScriptingDefinitionHelper
    {
        private static readonly (string className, string scriptingDefinition)[] Definitions = 
        {
            ("GameInputDelegator", "JAM_INPUT_DELEGATOR")
        };
        //============================================================================================================//
        /// <summary>
        /// Adds scripting definitions based on whether particular classes exist.
        /// </summary>
        /// <example>
        /// For the <c>GameInputDelegator</c> we may want to know if this exists so input handling in
        /// the included samples needs to be adjusted <i>(New vs Old input systems)</i>
        /// </example>
        static ScriptingDefinitionHelper()
        {
            foreach (var (className, scriptingDefinition) in Definitions)
            {
                if (ClassExists(className))
                    AddDefineIfMissing(scriptingDefinition);
                else
                    TryRemoveDefine(scriptingDefinition);
            }
        }
        
        private static bool ClassExists(string fullTypeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var definedType in asm.DefinedTypes)
                {
                    if (!definedType.IsClass)
                        continue;

                    if (!definedType.Name.Contains(fullTypeName, StringComparison.OrdinalIgnoreCase))
                        continue;
                    
                    return true;
                }
            }
            return false;
        }
       
        
        private static void AddDefineIfMissing(string scriptingDefinition)
        {
            var activeBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var target = NamedBuildTarget.FromBuildTargetGroup(activeBuildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(target, out var defines);

            var values = defines.AsValueEnumerable().ToList();
            var hasDefinition = values.Contains(scriptingDefinition);

            if (hasDefinition)
                return;

            values.Add(scriptingDefinition);
            PlayerSettings.SetScriptingDefineSymbols(target, values.ToArray());
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Added scripting define: {scriptingDefinition}");
        }
        
        private static void TryRemoveDefine(string scriptingDefinition)
        {
            var activeBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var target = NamedBuildTarget.FromBuildTargetGroup(activeBuildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(target, out var defines);

            var values = defines.AsValueEnumerable().ToList();
            var hasDefinition = values.Contains(scriptingDefinition);

            if (!hasDefinition)
                return;

            values.Remove(scriptingDefinition);
            PlayerSettings.SetScriptingDefineSymbols(target, values.ToArray());
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Removed scripting define: {scriptingDefinition}");
        }
    }
}