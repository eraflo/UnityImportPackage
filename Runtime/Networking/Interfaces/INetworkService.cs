namespace Eraflo.Catalyst.Networking
{
    /// <summary>
    /// Interface for checking network authority and state.
    /// </summary>
    public interface INetworkService
    {
        /// <summary>Whether the local instance is the server or host.</summary>
        bool IsServer { get; }

        /// <summary>Whether the local instance is a client.</summary>
        bool IsClient { get; }

        /// <summary>Whether the network is currently connected.</summary>
        bool IsConnected { get; }
    }
}
