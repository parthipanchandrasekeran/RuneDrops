using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuneDrop.Core
{
    /// <summary>
    /// Lightweight service locator for dependency injection.
    /// Services register on Awake and unregister on OnDestroy.
    /// </summary>
    public static class ServiceLocator
    {
        // ── Storage ─────────────────────────────────────────────────
        private static readonly Dictionary<Type, object> _services = new();

        // ── Register / Unregister ───────────────────────────────────
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Overwriting existing service: {type.Name}");
            }
            _services[type] = service;
        }

        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
            }
        }

        // ── Resolve ─────────────────────────────────────────────────
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            Debug.LogError($"[ServiceLocator] Service not found: {type.Name}");
            return null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }

        // ── Cleanup ─────────────────────────────────────────────────
        public static void Clear()
        {
            _services.Clear();
        }
    }
}
