using UnityEngine;

namespace Eraflo.Catalyst.Events
{
    /// <summary>
    /// Event channel raised when a scene transition starts or completes.
    /// Carries the name of the scene group.
    /// </summary>
    [CreateAssetMenu(fileName = "SceneTransitionChannel", menuName = "Catalyst/Events/Scene Transition Channel", order = 10)]
    public class SceneTransitionChannel : EventChannel<string> { }
}
