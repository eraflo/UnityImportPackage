using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eraflo.UnityImportPackage.BehaviourTree
{
    /// <summary>
    /// A shared data container for passing information between nodes in a behaviour tree.
    /// Thread-safe when PackageRuntime.IsThreadSafe is enabled.
    /// </summary>
    [Serializable]
    public class Blackboard
    {
        [Serializable]
        private class BlackboardEntry
        {
            public string Key;
            public string TypeName;
            public string JsonValue;
            
            [NonSerialized] public object CachedValue;
            [NonSerialized] public bool IsCached;
        }
        
        [SerializeField] private List<BlackboardEntry> _entries = new();
        
        private readonly Dictionary<string, object> _runtimeData = new();
        private readonly object _lock = new();
        
        private static bool IsThreadSafe => Core.PackageRuntime.IsThreadSafe;
        
        /// <summary>
        /// Sets a value in the blackboard.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="key">Key to store the value under.</param>
        /// <param name="value">The value to store.</param>
        public void Set<T>(string key, T value)
        {
            if (IsThreadSafe)
            {
                lock (_lock)
                {
                    _runtimeData[key] = value;
                }
            }
            else
            {
                _runtimeData[key] = value;
            }
        }
        
        /// <summary>
        /// Gets a value from the blackboard.
        /// </summary>
        /// <typeparam name="T">Expected type of the value.</typeparam>
        /// <param name="key">Key to retrieve.</param>
        /// <returns>The value, or default if not found.</returns>
        public T Get<T>(string key)
        {
            if (TryGet<T>(key, out var value))
            {
                return value;
            }
            return default;
        }
        
        /// <summary>
        /// Tries to get a value from the blackboard.
        /// </summary>
        /// <typeparam name="T">Expected type of the value.</typeparam>
        /// <param name="key">Key to retrieve.</param>
        /// <param name="value">The retrieved value.</param>
        /// <returns>True if the key exists and the value is of the correct type.</returns>
        public bool TryGet<T>(string key, out T value)
        {
            object obj;
            
            if (IsThreadSafe)
            {
                lock (_lock)
                {
                    if (!_runtimeData.TryGetValue(key, out obj))
                    {
                        value = default;
                        return false;
                    }
                }
            }
            else
            {
                if (!_runtimeData.TryGetValue(key, out obj))
                {
                    value = default;
                    return false;
                }
            }
            
            if (obj is T typedValue)
            {
                value = typedValue;
                return true;
            }
            
            value = default;
            return false;
        }
        
        /// <summary>
        /// Checks if a key exists in the blackboard.
        /// </summary>
        /// <param name="key">Key to check.</param>
        /// <returns>True if the key exists.</returns>
        public bool Contains(string key)
        {
            if (IsThreadSafe)
            {
                lock (_lock)
                {
                    return _runtimeData.ContainsKey(key);
                }
            }
            return _runtimeData.ContainsKey(key);
        }
        
        /// <summary>
        /// Removes a key from the blackboard.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        /// <returns>True if the key was removed.</returns>
        public bool Remove(string key)
        {
            if (IsThreadSafe)
            {
                lock (_lock)
                {
                    return _runtimeData.Remove(key);
                }
            }
            return _runtimeData.Remove(key);
        }
        
        /// <summary>
        /// Clears all data from the blackboard.
        /// </summary>
        public void Clear()
        {
            if (IsThreadSafe)
            {
                lock (_lock)
                {
                    _runtimeData.Clear();
                }
            }
            else
            {
                _runtimeData.Clear();
            }
        }
        
        /// <summary>
        /// Gets a snapshot of all keys in the blackboard.
        /// </summary>
        /// <returns>Array of all keys.</returns>
        public string[] GetAllKeys()
        {
            if (IsThreadSafe)
            {
                lock (_lock)
                {
                    var keys = new string[_runtimeData.Count];
                    _runtimeData.Keys.CopyTo(keys, 0);
                    return keys;
                }
            }
            else
            {
                var keys = new string[_runtimeData.Count];
                _runtimeData.Keys.CopyTo(keys, 0);
                return keys;
            }
        }
        
        /// <summary>
        /// Creates a copy of this blackboard.
        /// </summary>
        /// <returns>A new Blackboard with copied data.</returns>
        public Blackboard Clone()
        {
            var clone = new Blackboard();
            
            if (IsThreadSafe)
            {
                lock (_lock)
                {
                    foreach (var kvp in _runtimeData)
                    {
                        clone._runtimeData[kvp.Key] = kvp.Value;
                    }
                }
            }
            else
            {
                foreach (var kvp in _runtimeData)
                {
                    clone._runtimeData[kvp.Key] = kvp.Value;
                }
            }
            
            return clone;
        }
    }
}
