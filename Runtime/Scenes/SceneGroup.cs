using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eraflo.Catalyst
{
    /// <summary>
    /// Represents a group of scenes that should be loaded together.
    /// </summary>
    [Serializable]
    public class SceneGroup
    {
        [Tooltip("The unique name for this scene group.")]
        public string Name;

        [Tooltip("The list of scenes to load additively.")]
        public List<string> Scenes = new List<string>();

        [Tooltip("The scene to set as active after loading. Must be one of the scenes in the list.")]
        public string ActiveScene;
    }
}
