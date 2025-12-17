using UnityEngine;

namespace Eraflo.UnityImportPackage.Events
{
    /// <summary>
    /// Static manager for network event handling.
    /// </summary>
    public static class NetworkEventManager
    {
        private static INetworkEventHandler _handler;

        /// <summary>
        /// The current network event handler.
        /// </summary>
        public static INetworkEventHandler Handler
        {
            get => _handler;
            set => _handler = value;
        }

        /// <summary>
        /// Whether a network handler is configured and connected.
        /// </summary>
        public static bool IsNetworkAvailable => _handler != null && _handler.IsConnected;

        /// <summary>
        /// Whether the local client is the server/host.
        /// </summary>
        public static bool IsServer => _handler?.IsServer ?? true;

        /// <summary>
        /// Registers a network handler. Call this when your network initializes.
        /// </summary>
        public static void RegisterHandler(INetworkEventHandler handler)
        {
            _handler = handler;
            
            if (PackageSettings.Instance.NetworkDebugMode)
            {
                Debug.Log($"[NetworkEventManager] Handler registered: {handler.GetType().Name}");
            }
        }

        /// <summary>
        /// Unregisters the current network handler. Call this on network disconnect.
        /// </summary>
        public static void UnregisterHandler()
        {
            if (PackageSettings.Instance.NetworkDebugMode)
            {
                Debug.Log("[NetworkEventManager] Handler unregistered");
            }
            _handler = null;
        }

        /// <summary>
        /// Sends an event over the network.
        /// </summary>
        internal static void SendEvent(string channelId, byte[] data, NetworkEventTarget target)
        {
            if (_handler == null)
            {
                if (PackageSettings.Instance.NetworkDebugMode)
                {
                    Debug.LogWarning("[NetworkEventManager] No network handler registered");
                }
                return;
            }

            if (!_handler.IsConnected)
            {
                if (PackageSettings.Instance.NetworkDebugMode)
                {
                    Debug.LogWarning("[NetworkEventManager] Network not connected");
                }
                return;
            }

            _handler.SendEvent(channelId, data, target);
        }
    }

    /// <summary>
    /// MonoBehaviour component for NetworkEventManager.
    /// Auto-instantiated if networking is enabled in PackageSettings.
    /// </summary>
    public class NetworkEventManagerBehaviour : MonoBehaviour
    {
        private void Awake()
        {
            if (PackageSettings.Instance.NetworkDebugMode)
            {
                Debug.Log("[NetworkEventManagerBehaviour] Initialized and persistent");
            }
        }

        private void OnDestroy()
        {
            NetworkEventManager.UnregisterHandler();
        }
    }
}
