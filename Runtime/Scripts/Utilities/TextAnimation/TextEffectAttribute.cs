// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using System;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Marks a <see cref="TextEffect"/> subclass with the link id that selects it from text markup.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TextEffectAttribute : Attribute
    {
        public string Key { get; }

        public TextEffectAttribute(string key)
        {
            Key = key;
        }
    }
}
