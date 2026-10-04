using System;
using System.Text;
using JamStarter.Utilities.Attributes;
using UnityEngine;

namespace Samples.MapTiles.Tiles
{
    /// <summary>
    /// Extend this class to include custom functionality. Used as the main definition for any tiles.
    /// </summary>
    [Serializable]
    public class TileTypeDefinition : ITileType
    {
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField, ReadOnly] public int Id { get; set; }

        [TextArea] public string description;
        [Range(0, 100)] public int spawnWeight;
        public BaseTile[] tileVariants;

        //Tile Type Naming
        //================================================================================================================//

        #region Tile Type Naming

        /// <summary>
        /// Converts a tile or tileset name into a name that compiles as C# code. Trims the name, optionally upper-cases
        /// it, replaces every character other than letters, digits and '_' with '_', and adds a leading '_' when the
        /// name starts with a digit. Returns an empty string for a null or blank name.
        /// <code>
        /// ConvertNameToCodeName("Grass")             // "GRASS"
        /// ConvertNameToCodeName(" deep water ")      // "DEEP_WATER"
        /// ConvertNameToCodeName("Orc-Camp's")        // "ORC_CAMP_S"
        /// ConvertNameToCodeName("9 lives")           // "_9_LIVES"
        /// ConvertNameToCodeName("Forest Set", false) // "Forest_Set"
        /// </code>
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        public static string ConvertNameToCodeName(string name, bool upperCase = true)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            var source = upperCase ? name.Trim().ToUpperInvariant() : name.Trim();
            var builder = new StringBuilder(source.Length + 1);

            if (IsDigit(source[0]))
                builder.Append('_');

            foreach (var c in source)
                builder.Append(IsDigit(c) || c == '_' || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ? c : '_');

            return builder.ToString();

            static bool IsDigit(char c) => c >= '0' && c <= '9';
        }

        /// <summary>
        /// Stable FNV-1a 32-bit hash of the name after <see cref="ConvertNameToCodeName"/>, so "grass" and "GRASS" match. The Id depends only on the name:
        /// tiles with the same name in different tilesets share an Id, and renaming a tile changes its Id.
        /// Returns 0 only for an empty name; a name whose hash is 0 maps to 1 so it never equals default(TileType).
        /// </summary>
        /// <remarks>Created by Claude (claude-opus-5-5), 2026-09-30</remarks>
        public static int ConvertNameToId(string name)
        {
            var codeName = ConvertNameToCodeName(name);

            if (codeName.Length == 0)
                return 0;

            // FNV-1a: start from the standard 32-bit seed, then for each character mix it in (XOR) and multiply by the
            // standard 32-bit FNV prime. Both numbers are fixed by the FNV spec; changing them changes every Id.
            var hash = 2166136261u;

            unchecked
            {
                foreach (var c in codeName)
                {
                    hash ^= c;
                    hash *= 16777619u;
                }

                var id = (int)hash;
                return id == 0 ? 1 : id;
            }
        }

        #endregion //Tile Type Naming

        //================================================================================================================//
    }
}