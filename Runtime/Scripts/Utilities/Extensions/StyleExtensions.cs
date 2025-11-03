using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.Utilities.Extensions
{
    public static class StyleExtensions
    {
        public static void SetPadding(this IStyle style, float padding)
        {
            style.paddingBottom = padding;
            style.paddingTop = padding;
            style.paddingLeft = padding;
            style.paddingRight = padding;
        }
        public static void SetMargins(this IStyle style, float margins)
        {
            style.marginBottom = margins;
            style.marginTop = margins;
            style.marginLeft = margins;
            style.marginRight = margins;
        }
        public static void SetBorderWidth(this IStyle style, float width)
        {
            style.borderBottomWidth = width;
            style.borderTopWidth = width;
            style.borderLeftWidth = width;
            style.borderRightWidth = width;
        }
        public static void SetBorderColor(this IStyle style, Color color)
        {
            style.borderBottomColor = color;
            style.borderTopColor = color;
            style.borderLeftColor = color;
            style.borderRightColor = color;
        }
        public static void SetBorderRadius(this IStyle style, float radius)
        {
            style.borderBottomLeftRadius = radius;
            style.borderBottomRightRadius = radius;
            style.borderTopLeftRadius = radius;
            style.borderTopRightRadius = radius;
        }
    }
}