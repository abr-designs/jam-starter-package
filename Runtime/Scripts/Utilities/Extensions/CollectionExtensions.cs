using System;
using System.Collections.Generic;



namespace Utilities
{
    public static class CollectionExtensions
    {
        private static readonly System.Random Rand = new();
        
        public static T PickRandomElement<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                throw new InvalidOperationException($"Passed collection is null or empty when calling {nameof(PickRandomElement)}!");
            
            var randomIndex = Rand.Next(array.Length);

            return array[randomIndex];
        }

        public static T PickRandomElement<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException($"Passed collection is null or empty when calling {nameof(PickRandomElement)}!");
            
            var randomIndex = Rand.Next(list.Count);

            return list[randomIndex];
        }

        //Based on: https://stackoverflow.com/a/1262619
        public static void Shuffle<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException($"Passed collection is null or empty when calling {nameof(Shuffle)}!");
            
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rand.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
        public static void Shuffle<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                throw new InvalidOperationException($"Passed collection is null or empty when calling {nameof(Shuffle)}!");
            
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = Rand.Next(n + 1);
                (array[k], array[n]) = (array[n], array[k]);
            }
        }
    }
}