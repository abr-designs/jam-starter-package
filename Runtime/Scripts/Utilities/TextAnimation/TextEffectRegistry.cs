// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Discovers every <see cref="TextEffect"/> subclass tagged with <see cref="TextEffectAttribute"/>
    /// by reflection and caches one shared instance per key. Effects are stateless, so a single
    /// instance is reused across all spans and texts (Flyweight).
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    public static class TextEffectRegistry
    {
        //Fields
        //================================================================================================================//

        #region Fields

        private static Dictionary<string, TextEffect> s_Effects;

        #endregion //Fields

        //Public Methods
        //================================================================================================================//

        #region Public Methods

        /// <summary>
        /// Return the cached effect for a link id, or null when no effect claims that key.
        /// </summary>
        public static TextEffect Get(string key)
        {
            if (s_Effects == null)
                ScanAssemblies();

            return s_Effects.TryGetValue(key, out var effect) ? effect : null;
        }

        #endregion //Public Methods

        //Private Methods
        //================================================================================================================//

        #region Private Methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void WarmCache()
        {
            if (s_Effects == null)
                ScanAssemblies();
        }

        private static void ScanAssemblies()
        {
            s_Effects = new Dictionary<string, TextEffect>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types;
                }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract || typeof(TextEffect).IsAssignableFrom(type) == false)
                        continue;

                    var attribute = (TextEffectAttribute)Attribute.GetCustomAttribute(type, typeof(TextEffectAttribute));
                    if (attribute == null)
                        continue;

                    if (s_Effects.ContainsKey(attribute.Key))
                    {
                        Debug.LogWarning($"Duplicate text effect key '{attribute.Key}' on {type.Name}; keeping the first registered effect.");
                        continue;
                    }

                    s_Effects.Add(attribute.Key, (TextEffect)Activator.CreateInstance(type));
                }
            }
        }

        #endregion //Private Methods
    }
}
