namespace Eraflo.UnityImportPackage.Events
{
    /// <summary>
    /// Interface for network event handlers.
    /// Implement this interface to integrate with your networking solution (Netcode, Mirror, Photon, etc.).
    /// </summary>
    public interface INetworkEventHandler
    {
        /// <summary>
        /// Called when an event needs to be sent over the network.
        /// </summary>
        /// <param name="channelId">Unique identifier for the event channel.</param>
        /// <param name="data">Serialized event data (null for void events).</param>
        /// <param name="target">Who should receive this event.</param>
        void SendEvent(string channelId, byte[] data, NetworkEventTarget target);

        /// <summary>
        /// Whether the local client is the server/host.
        /// </summary>
        bool IsServer { get; }

        /// <summary>
        /// Whether the network is currently connected.
        /// </summary>
        bool IsConnected { get; }
    }

    /// <summary>
    /// Target recipients for network events.
    /// </summary>
    public enum NetworkEventTarget
    {
        /// <summary>Send to all clients (including self).</summary>
        All,
        
        /// <summary>Send to all clients except self.</summary>
        Others,
        
        /// <summary>Send to server only.</summary>
        Server,
        
        /// <summary>Send to specific client (requires additional targeting).</summary>
        Targeted
    }

    /// <summary>
    /// Configuration for how events should be synchronized over the network.
    /// </summary>
    public enum NetworkEventMode
    {
        /// <summary>Event is raised locally only, never sent over network.</summary>
        LocalOnly,
        
        /// <summary>Event is sent to all clients (including self).</summary>
        Broadcast,
        
        /// <summary>Event is sent to all other clients (excluding self).</summary>
        BroadcastOthers,
        
        /// <summary>Event is sent to server only (client to server).</summary>
        ServerOnly,
        
        /// <summary>Event is raised locally and sent to others.</summary>
        LocalAndBroadcast
    }
}
