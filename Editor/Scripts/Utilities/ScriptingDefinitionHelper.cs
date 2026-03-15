using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace JamStarter.Editor.Scripts.Utilities
{
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
        public class ScriptDeletionHandler : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
            {
                foreach (var (className, scriptingDefinition) in Definitions)
                {
                    if (imported.Any(p => p.EndsWith($"{className}.cs")))
                    {
                        AddDefineIfMissing(scriptingDefinition);
                    }
                    else if (deleted.Any(p => p.EndsWith($"{className}.cs")))
                    {
                        TryRemoveDefine(scriptingDefinition);
                    }
                }

            }
        }
       
        private static void AddDefineIfMissing(string scriptingDefinition)
        {
            var activeBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var target = NamedBuildTarget.FromBuildTargetGroup(activeBuildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(target, out var defines);

            var values = defines.ToList();
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

            var values = defines.ToList();
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