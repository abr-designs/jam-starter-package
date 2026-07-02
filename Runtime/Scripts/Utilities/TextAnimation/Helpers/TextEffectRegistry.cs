// Created by Claude (claude-opus-4-8)
// Date: 2026-06-23

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Utilities.TextAnimation
{
    /// <summary>
    /// Discovers every <see cref="MotionTextEffect"/> and <see cref="ColorTextEffect"/> subclass tagged
    /// with <see cref="TextEffectAttribute"/> by reflection and caches one shared instance per key. Effects
    /// are stateless, so a single instance is reused across all spans and texts (Flyweight). Keys are
    /// resolved per channel, so a motion key and a color key may reuse the same string without clashing.
    /// </summary>
    /// <remarks>Created by Claude (claude-opus-4-8) on 2026-06-23</remarks>
    public static class TextEffectRegistry
    {
        //Fields
        //================================================================================================================//

        #region Fields

        private static Dictionary<string, MotionTextEffect> s_MotionEffects;
        private static Dictionary<string, ColorTextEffect> s_ColorEffects;

        #endregion //Fields

        //Public Methods
        //================================================================================================================//

        #region Public Methods

        /// <summary>
        /// Return the cached motion effect for a key, or null when no motion effect claims that key.
        /// </summary>
        public static MotionTextEffect GetMotion(string key)
        {
            if (s_MotionEffects == null)
                ScanAssemblies();

            return s_MotionEffects.TryGetValue(key, out var effect) ? effect : null;
        }

        /// <summary>
        /// Return the cached color effect for a key, or null when no color effect claims that key.
        /// </summary>
        public static ColorTextEffect GetColor(string key)
        {
            if (s_ColorEffects == null)
                ScanAssemblies();

            return s_ColorEffects.TryGetValue(key, out var effect) ? effect : null;
        }

        #endregion //Public Methods

        //Private Methods
        //================================================================================================================//

        #region Private Methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void WarmCache()
        {
            if (s_MotionEffects == null)
                ScanAssemblies();
        }

        private static void ScanAssemblies()
        {
            s_MotionEffects = new Dictionary<string, MotionTextEffect>();
            s_ColorEffects = new Dictionary<string, ColorTextEffect>();

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

                    if (typeof(MotionTextEffect).IsAssignableFrom(type))
                        Register(s_MotionEffects, attribute.Key, (MotionTextEffect)Activator.CreateInstance(type), "motion");
                    else if (typeof(ColorTextEffect).IsAssignableFrom(type))
                        Register(s_ColorEffects, attribute.Key, (ColorTextEffect)Activator.CreateInstance(type), "color");
                    else
                        Debug.LogWarning($"Text effect '{type.Name}' must extend MotionTextEffect or ColorTextEffect; skipping.");
                }
            }
        }

        private static void Register<T>(Dictionary<string, T> table, string key, T effect, string channel)
            where T : TextEffect
        {
            if (table.ContainsKey(key))
            {
                Debug.LogWarning($"Duplicate {channel} effect key '{key}' on {effect.GetType().Name}; keeping the first registered effect.");
                return;
            }

            table.Add(key, effect);
        }

        #endregion //Private Methods
    }
}
