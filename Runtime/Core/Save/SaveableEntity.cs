using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eraflo.Catalyst.Core.Save
{
    /// <summary>
    /// Identifies a GameObject for saving and collects state from all ISaveable components on it.
    /// </summary>
    [DisallowMultipleComponent]
    public class SaveableEntity : MonoBehaviour
    {
        [SerializeField] private string _guid;

        public string Guid => _guid;

        private void OnEnable()
        {
            App.Get<SaveManager>()?.Register(this);
        }

        private void OnDisable()
        {
            App.Get<SaveManager>()?.Unregister(this);
        }

        private void OnValidate()
        {
            // Generate GUID if empty
            if (string.IsNullOrEmpty(_guid))
            {
                _guid = System.Guid.NewGuid().ToString();
            }
        }

        [ContextMenu("Regenerate GUID")]
        private void RegenerateGuid()
        {
            _guid = System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Captures the state of all ISaveable components on this GameObject.
        /// </summary>
        public Dictionary<string, object> CaptureState()
        {
            var state = new Dictionary<string, object>();
            foreach (var saveable in GetComponents<ISaveable>())
            {
                string typeName = saveable.GetType().AssemblyQualifiedName;
                state[typeName] = saveable.SaveState();
            }
            return state;
        }

        /// <summary>
        /// Restores the state for all ISaveable components on this GameObject.
        /// </summary>
        public void RestoreState(Dictionary<string, object> state)
        {
            foreach (var saveable in GetComponents<ISaveable>())
            {
                string typeName = saveable.GetType().AssemblyQualifiedName;
                if (state.TryGetValue(typeName, out object componentState))
                {
                    saveable.LoadState(componentState);
                }
            }
        }
    }
}
