#if UNITY_EDITOR
//#define LOGGING

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FixedColorPaletteTool;
using FixedColorPaletteTool.Enums;
using YamlDotNet.RepresentationModel;
using Debug = UnityEngine.Debug;

/// <summary>
/// Parses the Unity Asset Yaml for FixedPaletteSettings to extract the colors associated with that palette. 
/// <remarks>
/// This is
/// to resolve an order-of-operations issue that is experienced when attempting to access the palette via a class constructor
/// or field initializer.
/// </remarks>
/// <example>
/// <c>
/// Color32 primaryColor = PaletteUtility.Primary;
/// </c>
/// </example>
/// </summary>
public static class UnityPaletteParser
{
    private class UnityYamlDocument
    {
        public string anchor;      // The &fileID
        public string rawText;     // Raw YAML for this document
    }

    public static bool  TryParsePaletteYaml(string filePath, out List<ColorData> parsedData)
    {
        parsedData = null;
            
        
        var fullText = File.ReadAllText(filePath);

        // --------------------------------------------------------------------
        // STEP 1 — Split Unity YAML into documents & extract anchors
        // --------------------------------------------------------------------
        var docs = SplitUnityYamlDocuments(fullText);

        // --------------------------------------------------------------------
        // STEP 2 — Find selectedPalette.fileID anywhere in the YAML
        // --------------------------------------------------------------------
        var selectedFileID = FindSelectedPaletteFileID(fullText);

        if (selectedFileID == null)
        {
            LogError("selectedPalette.fileID not found.");
            return false;
        }

        Log($"Selected palette fileID = {selectedFileID}");

        // --------------------------------------------------------------------
        // STEP 3 — Find the document whose anchor == selected fileID
        // --------------------------------------------------------------------
        var paletteDoc = docs.FirstOrDefault(d => d.anchor == selectedFileID);

        if (paletteDoc == null)
        {
            LogError($"No document with anchor &{selectedFileID} found.");//
            return false;
        }

        // --------------------------------------------------------------------
        // STEP 4 — Load that document with YamlDotNet and parse colors
        // --------------------------------------------------------------------
        const string prefix = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- ";
        var yaml = new YamlStream();
        yaml.Load(new StringReader(prefix+paletteDoc.rawText));

        var root = (YamlMappingNode)yaml.Documents[0].RootNode;
        var mono = (YamlMappingNode)root.Children["MonoBehaviour"];
        var colorsNode = (YamlSequenceNode)mono.Children["colors"];

        Log($"Found {colorsNode.Children.Count} colors in this palette.");

        if (colorsNode.Children.Count <= 0)
            return false;

        parsedData = new List<ColorData>();

        foreach (YamlMappingNode entry in colorsNode)
        {
            var name = entry["name"].ToString().Trim('"');
            var colorType = int.Parse(entry["colorType"].ToString());

            var colorData = (YamlMappingNode)entry["color"];
            var rgba = UInt32.Parse(colorData["rgba"].ToString());

            var unityColor = RgbaToUnityColor(rgba);
            
            parsedData.Add(new ColorData
            {
                name = name,
                color = unityColor,
                colorType = (COLOR)colorType
            });

            Log($"Color {name} | Type {colorType} | Unity {unityColor}");
        }

        return true;
    }

    // ------------------------------------------------------------------------
    // Convert Unity RGBA uint → UnityEngine.Color
    // ------------------------------------------------------------------------
    private static Color32 RgbaToUnityColor(uint rgba)
    {
        var a = (byte)((rgba >> 24) & 0xFF);
        var b = (byte)((rgba >> 16) & 0xFF);
        var g = (byte)((rgba >> 8) & 0xFF);
        var r = (byte)(rgba & 0xFF);

        return new Color32(r, g, b, a);
    }

    // ------------------------------------------------------------------------
    // Extract selectedPalette fileID from raw text
    // ------------------------------------------------------------------------
    private static string FindSelectedPaletteFileID(string text)
    {
        const string key = "selectedPalette:";
        var idx = text.IndexOf(key, StringComparison.Ordinal);
        if (idx < 0) return null;

        var line = text.Substring(idx, Mathf.Min(text.Length - idx, 200)); // read enough chars
        var fileID = line
            .Replace("selectedPalette: {","")
            .Replace("fileID: ", "")
            .Replace("{","")
            .Replace("}","")
            .Replace("\n","");
        
        
        /*var start = line.IndexOf("fileID: ", StringComparison.Ordinal) + 8;

        var fileID = new string(line.Skip(start)
            .TakeWhile(Char.IsDigit).ToArray());*/

        return fileID;
    }

    // ------------------------------------------------------------------------
    // Split Unity YAML into documents and capture anchors (&xxxxx)
    // ------------------------------------------------------------------------
    private static List<UnityYamlDocument> SplitUnityYamlDocuments(string fullText)
    {
        var docs = new List<UnityYamlDocument>();

        var chunks = fullText.Split(new[] { "\n---" }, StringSplitOptions.None);

        foreach (var chunk in chunks)
        {
            var trimmed = chunk.Trim();
            if (!trimmed.StartsWith("!u!")) continue;

            // First line looks like:
            // !u!114 &6698199279741290727
            var firstLine = trimmed.Split('\n')[0];
            var anchor = firstLine.Contains("&")
                ? firstLine.Split('&')[1].Trim()
                : null;

            docs.Add(new UnityYamlDocument
            {
                anchor = anchor,
                rawText = trimmed
            });
        }

        return docs;
    }

    //================================================================================================================//
    [Conditional("LOGGING")]
    private static void Log(string log) => Debug.Log(log);

    [Conditional("LOGGING")]
    private static void LogError(string logError) => Debug.LogError(logError);
    //================================================================================================================//
}

#endif