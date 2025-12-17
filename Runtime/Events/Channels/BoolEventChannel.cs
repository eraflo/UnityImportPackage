using UnityEngine;

namespace Eraflo.UnityImportPackage.Events
{
    /// <summary>
    /// Event channel that carries a bool value.
    /// Create via Assets > Create > Events > Bool Channel.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBoolChannel", menuName = "Events/Bool Channel", order = 4)]
    public class BoolEventChannel : EventChannel<bool> { }
}
