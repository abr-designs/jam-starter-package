using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Samples.MapTiles.ScriptableObjects;
using Samples.MapTiles.Tiles;
using UnityEditor;
using UnityEngine;

namespace Samples.MapTiles.Editor
{
    /// <summary>
    /// Generates one static class of TileType constants per tileset, written to &lt;tileset folder&gt;/Generated/&lt;Class&gt;.cs.
    /// </summary>
    public static class TileTypeGenerator
    {
        public const string RegenerateMenuPath = "Tools/Map Tiles/Regenerate Tile Types";

        private const string k_GeneratedFolderName = "Generated";
        private const string k_DefaultClassSuffix = "Types";
        internal const string k_GeneratedMarker = "// AUTO-GENERATED FILE.";
        internal const string k_TilesetGuidPrefix = "// Tileset GUID: ";

        /// <summary>
        /// Source code generated for one tileset.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        public readonly struct GeneratedSource
        {
            public readonly TilesetScriptableObject Tileset;
            public readonly string TilesetGuid;
            public readonly string FilePath;
            public readonly string Contents;

            public GeneratedSource(TilesetScriptableObject tileset, string tilesetGuid, string filePath,
                string contents)
            {
                Tileset = tileset;
                TilesetGuid = tilesetGuid;
                FilePath = filePath;
                Contents = contents;
            }
        }

        //Generation
        //================================================================================================================//

        #region Generation

        /// <summary>
        /// Regenerates the tile types of every tileset from the menu.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        [MenuItem(RegenerateMenuPath)]
        private static void RegenerateFromMenu()
        {
            if (GenerateAll().Count == 0)
                Debug.Log("Tile types regenerated.");
        }

        /// <summary>
        /// Updates the serialized Ids of every tileset, validates them, writes each generated file whose contents
        /// changed and deletes generated files left behind by deleted, moved or renamed tilesets. A tileset that fails
        /// validation keeps its previous file. Returns the validation errors, which are also logged.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        public static List<string> GenerateAll()
        {
            var tilesets = LoadAllTilesets();

            foreach (var tileset in tilesets)
                UpdateIds(tileset);

            var errors = new List<string>();
            var sources = BuildSources(tilesets, errors);

            foreach (var error in errors)
                Debug.LogError(error);

            var wroteAny = false;
            foreach (var source in sources)
            {
                if (IsUpToDate(source))
                    continue;

                Directory.CreateDirectory(Path.GetDirectoryName(source.FilePath)!);
                File.WriteAllText(source.FilePath, source.Contents, Encoding.UTF8);
                wroteAny = true;
            }

            // [Claude 2026-09-30 start]
            var staleFiles = FindStaleFiles(sources);
            if (staleFiles.Count > 0)
            {
                var failed = new List<string>();
                AssetDatabase.DeleteAssets(staleFiles.ToArray(), failed);

                foreach (var path in failed)
                    Debug.LogError($"Could not delete stale generated file '{path}'.");
            }
            // [Claude 2026-09-30 end]

            if (wroteAny)
                AssetDatabase.Refresh();

            return errors;
        }

        /// <summary>
        /// Finds generated files whose header tileset GUID points to a tileset that no longer exists, or to a valid
        /// tileset whose expected file path is now different. Files of tilesets that failed validation are kept.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        private static List<string> FindStaleFiles(List<GeneratedSource> sources)
        {
            var expectedPathByGuid = sources.ToDictionary(s => s.TilesetGuid, s => s.FilePath);
            var staleFiles = new List<string>();

            foreach (var scriptGuid in AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(scriptGuid);

                if (!path.Contains($"/{k_GeneratedFolderName}/") || !path.EndsWith(".cs", StringComparison.Ordinal))
                    continue;

                var tilesetGuid = ReadTilesetGuid(path);
                if (tilesetGuid == null)
                    continue;

                var tilesetPath = AssetDatabase.GUIDToAssetPath(tilesetGuid);
                var tilesetExists = !string.IsNullOrEmpty(tilesetPath) &&
                                    AssetDatabase.LoadAssetAtPath<TilesetScriptableObject>(tilesetPath) != null;

                if (!tilesetExists)
                    staleFiles.Add(path);
                else if (expectedPathByGuid.TryGetValue(tilesetGuid, out var expectedPath) && expectedPath != path)
                    staleFiles.Add(path);
            }

            return staleFiles;
        }

        /// <summary>
        /// Returns the tileset GUID from a generated file header, or null when the file was not generated by this class.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        private static string ReadTilesetGuid(string path)
        {
            var isGenerated = false;

            foreach (var line in File.ReadLines(path).Take(8))
            {
                var trimmed = line.TrimStart('﻿');

                if (trimmed == k_GeneratedMarker)
                    isGenerated = true;
                else if (isGenerated && trimmed.StartsWith(k_TilesetGuidPrefix, StringComparison.Ordinal))
                    return trimmed.Substring(k_TilesetGuidPrefix.Length).Trim();
            }

            return null;
        }

        /// <summary>
        /// Loads every TilesetScriptableObject asset in the project.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        public static List<TilesetScriptableObject> LoadAllTilesets()
        {
            return AssetDatabase.FindAssets($"t:{nameof(TilesetScriptableObject)}")
                .Select(guid =>
                    AssetDatabase.LoadAssetAtPath<TilesetScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(tileset => tileset != null)
                .ToList();
        }

        /// <summary>
        /// Builds the generated source of every valid tileset without touching the disk or the tilesets. Tilesets that
        /// fail validation, or that collide with another tileset's class or file, are left out and reported in
        /// <paramref name="errors"/>.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        public static List<GeneratedSource> BuildSources(IReadOnlyList<TilesetScriptableObject> tilesets,
            List<string> errors)
        {
            var candidates = new List<(GeneratedSource source, string fullClassName)>();

            foreach (var tileset in tilesets)
            {
                if (TryBuildSource(tileset, errors, out var source, out var fullClassName))
                    candidates.Add((source, fullClassName));
            }

            var clashing = new HashSet<string>();
            ReportClashes(candidates, c => c.fullClassName, "class", errors, clashing);
            ReportClashes(candidates, c => c.source.FilePath, "file", errors, clashing);

            return candidates
                .Where(c => !clashing.Contains(c.source.TilesetGuid))
                .Select(c => c.source)
                .ToList();
        }

        /// <summary>
        /// Returns true when the file on disk matches the generated contents, ignoring line ending differences.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        public static bool IsUpToDate(GeneratedSource source)
        {
            return File.Exists(source.FilePath) &&
                   NormalizeLineEndings(File.ReadAllText(source.FilePath)) == NormalizeLineEndings(source.Contents);
        }

        /// <summary>
        /// Sets each tile Id from its name and saves the tileset only when an Id changed.
        /// </summary>
        /// <remarks>Rewritten by Claude (claude-opus-5-5), 2026-09-30</remarks>
        private static void UpdateIds(TilesetScriptableObject tileset)
        {
            if (tileset.tiles == null)
                return;

            var changed = false;
            foreach (var tile in tileset.tiles)
            {
                if (tile == null)
                    continue;

                var id = TileTypeDefinition.ConvertNameToId(tile.Name);
                if (tile.Id == id)
                    continue;

                tile.Id = id;
                changed = true;
            }

            if (!changed)
                return;

            EditorUtility.SetDirty(tileset);
            AssetDatabase.SaveAssetIfDirty(tileset);
        }

        #endregion //Generation

        //Validation
        //================================================================================================================//

        #region Validation

        /// <summary>
        /// Validates one tileset and builds its source. Returns false and appends to <paramref name="errors"/> when the
        /// tileset location, class name, namespace or tile names cannot produce compiling code.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        private static bool TryBuildSource(TilesetScriptableObject tileset, List<string> errors,
            out GeneratedSource source, out string fullClassName)
        {
            source = default;
            fullClassName = null;

            var assetPath = AssetDatabase.GetAssetPath(tileset);
            var errorCount = errors.Count;

            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
                errors.Add($"Tileset '{assetPath}' is outside Assets/, so its generated file cannot be written.");
            if (assetPath.Contains("/Editor/"))
                errors.Add(
                    $"Tileset '{assetPath}' is in an Editor folder, so its generated class would only exist in the editor.");

            var className = string.IsNullOrWhiteSpace(tileset.generatedClassName)
                ? TileTypeDefinition.ConvertNameToCodeName(tileset.name, false) + k_DefaultClassSuffix
                : tileset.generatedClassName.Trim();
            var namespaceName = tileset.generatedNamespace?.Trim() ?? string.Empty;

            if (!IsValidIdentifier(className))
                errors.Add($"Tileset '{assetPath}': '{className}' is not a valid class name.");
            if (className == nameof(TileType))
                errors.Add(
                    $"Tileset '{assetPath}': class name '{className}' clashes with the {nameof(TileType)} struct.");
            if (namespaceName.Length > 0 && !namespaceName.Split('.').All(IsValidIdentifier))
                errors.Add($"Tileset '{assetPath}': '{namespaceName}' is not a valid namespace.");

            var tiles = tileset.tiles ?? Array.Empty<TileTypeDefinition>();
            var namesByCodeName = new Dictionary<string, string>();
            var codeNamesById = new Dictionary<int, string>();

            for (var i = 0; i < tiles.Length; i++)
            {
                var tile = tiles[i];
                if (tile == null)
                    continue;

                var codeName = TileTypeDefinition.ConvertNameToCodeName(tile.Name);

                if (codeName.Length == 0)
                {
                    errors.Add($"Tileset '{assetPath}': tile {i} has no name.");
                    continue;
                }

                if (!namesByCodeName.TryAdd(codeName, tile.Name))
                {
                    errors.Add(
                        $"Tileset '{assetPath}': tiles '{namesByCodeName[codeName]}' and '{tile.Name}' both become {codeName}.");
                    continue;
                }

                if (codeName == className)
                    errors.Add(
                        $"Tileset '{assetPath}': tile '{tile.Name}' has the same name as its class '{className}'.");

                var id = TileTypeDefinition.ConvertNameToId(tile.Name);
                if (!codeNamesById.TryAdd(id, codeName))
                    errors.Add(
                        $"Tileset '{assetPath}': tiles {codeNamesById[id]} and {codeName} hash to the same Id ({id}). Rename one of them.");
            }

            if (errors.Count > errorCount)
                return false;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var filePath =
                $"{Path.GetDirectoryName(assetPath)!.Replace('\\', '/')}/{k_GeneratedFolderName}/{className}.cs";

            fullClassName = namespaceName.Length > 0 ? $"{namespaceName}.{className}" : className;
            source = new GeneratedSource(tileset, guid, filePath, WriteSource(guid, namespaceName, className, tiles));
            return true;
        }

        /// <summary>
        /// Reports candidates that share a key (class name or file path) and marks their tilesets as clashing.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        private static void ReportClashes(
            List<(GeneratedSource source, string fullClassName)> candidates,
            Func<(GeneratedSource source, string fullClassName), string> key,
            string kind,
            List<string> errors,
            HashSet<string> clashing)
        {
            foreach (var group in candidates.GroupBy(key).Where(g => g.Count() > 1))
            {
                var paths = group.Select(c => AssetDatabase.GetAssetPath(c.source.Tileset));
                errors.Add(
                    $"Tilesets {string.Join(", ", paths)} all generate the {kind} '{group.Key}'. Set a unique generatedClassName or generatedNamespace.");

                foreach (var candidate in group)
                    clashing.Add(candidate.source.TilesetGuid);
            }
        }

        /// <summary>
        /// Returns true when the value is a non-empty code name made of [A-Za-z0-9_] that does not start with a digit.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        private static bool IsValidIdentifier(string value)
        {
            return !string.IsNullOrEmpty(value) && TileTypeDefinition.ConvertNameToCodeName(value, false) == value;
        }

        #endregion //Validation

        //Code Writing
        //================================================================================================================//

        #region Code Writing

        /// <summary>
        /// Writes the generated class with '\n' line endings. Constants use Ids computed from the tile names.
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        private static string WriteSource(string tilesetGuid, string namespaceName, string className,
            TileTypeDefinition[] tiles)
        {
            var output = new StringBuilder();
            var indent = namespaceName.Length > 0 ? "    " : string.Empty;

            output.Append("// ==================================================\n");
            output.Append(k_GeneratedMarker).Append('\n');
            output.Append("// DO NOT EDIT.\n");
            output.Append(k_TilesetGuidPrefix).Append(tilesetGuid).Append('\n');
            output.Append("// ==================================================\n");
            output.Append('\n');

            if (namespaceName.Length > 0)
                output.Append("namespace ").Append(namespaceName).Append("\n{\n");

            output.Append(indent).Append("public static class ").Append(className).Append('\n');
            output.Append(indent).Append("{\n");

            foreach (var tile in tiles)
            {
                if (tile == null)
                    continue;

                output.Append(indent)
                    .Append("    public static readonly global::TileType ")
                    .Append(TileTypeDefinition.ConvertNameToCodeName(tile.Name))
                    .Append(" = new global::TileType(")
                    .Append(TileTypeDefinition.ConvertNameToId(tile.Name))
                    .Append(");\n");
            }

            output.Append(indent).Append("}\n");

            if (namespaceName.Length > 0)
                output.Append("}\n");

            return output.ToString();
        }

        private static string NormalizeLineEndings(string value) => value.Replace("\r\n", "\n");

        #endregion //Code Writing

        //================================================================================================================//
    }
}
