using System;
using UnityEngine;

namespace FixedColorPaletteTool
{
    [Serializable]
    public struct ColorData : IEquatable<ColorData>
    {
        [SerializeField]
        public string name;

        [SerializeField] 
        public Color32 color;

        public ColorData(Color32 color)
        {
            name = $"#{ColorUtility.ToHtmlStringRGB(color)}";
            this.color = color;
        }

        public bool Equals(ColorData other)
        {
            return color.Equals(other.color);
        }

        public override bool Equals(object obj)
        {
            return obj is ColorData other && Equals(other);
        }

        public override int GetHashCode() => color.GetHashCode();
    }
}