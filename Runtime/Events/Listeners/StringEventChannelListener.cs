using UnityEngine;

namespace Eraflo.UnityImportPackage.Events
{
    /// <summary>
    /// Listener for StringEventChannel.
    /// </summary>
    [AddComponentMenu("Events/String Channel Listener")]
    public class StringEventChannelListener : EventChannelListener<StringEventChannel, string> { }
}
