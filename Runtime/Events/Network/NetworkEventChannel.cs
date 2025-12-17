using System;
using UnityEngine;

namespace Eraflo.UnityImportPackage.Events
{
    /// <summary>
    /// Network-aware EventChannel for void events.
    /// Can be configured to automatically send events over the network.
    /// </summary>
    [CreateAssetMenu(fileName = "NewNetworkEventChannel", menuName = "Events/Network/Event Channel", order = 100)]
    public class NetworkEventChannel : EventChannel
    {
        [Header("Network Settings")]
        [SerializeField, Tooltip("How this event should be synchronized over the network.")]
        private NetworkEventMode _networkMode = NetworkEventMode.LocalOnly;

        [SerializeField, Tooltip("Unique identifier for network synchronization. Auto-generated from asset name if empty.")]
        private string _channelId = "";

        /// <summary>
        /// The network synchronization mode.
        /// </summary>
        public NetworkEventMode NetworkMode
        {
            get => _networkMode;
            set => _networkMode = value;
        }

        /// <summary>
        /// Unique identifier for this channel on the network.
        /// </summary>
        public string ChannelId => string.IsNullOrEmpty(_channelId) ? name : _channelId;

        /// <summary>
        /// Raises this event with network synchronization based on NetworkMode.
        /// </summary>
        public new void Raise()
        {
            switch (_networkMode)
            {
                case NetworkEventMode.LocalOnly:
                    base.Raise();
                    break;

                case NetworkEventMode.Broadcast:
                    SendOverNetwork(NetworkEventTarget.All);
                    break;

                case NetworkEventMode.BroadcastOthers:
                    SendOverNetwork(NetworkEventTarget.Others);
                    break;

                case NetworkEventMode.ServerOnly:
                    SendOverNetwork(NetworkEventTarget.Server);
                    break;

                case NetworkEventMode.LocalAndBroadcast:
                    base.Raise();
                    SendOverNetwork(NetworkEventTarget.Others);
                    break;
            }
        }

        /// <summary>
        /// Raises this event locally only, ignoring network settings.
        /// Use this when receiving events from the network.
        /// </summary>
        public void RaiseLocal()
        {
            base.Raise();
        }

        private void SendOverNetwork(NetworkEventTarget target)
        {
            if (!NetworkEventManager.IsNetworkAvailable)
            {
                // Fallback to local if no network
                base.Raise();
                return;
            }

            NetworkEventManager.SendEvent(ChannelId, null, target);
        }
    }

    /// <summary>
    /// Generic network-aware EventChannel that carries typed data.
    /// </summary>
    /// <typeparam name="T">The type of data the event carries. Must be serializable.</typeparam>
    public abstract class NetworkEventChannel<T> : EventChannel<T>
    {
        [Header("Network Settings")]
        [SerializeField, Tooltip("How this event should be synchronized over the network.")]
        private NetworkEventMode _networkMode = NetworkEventMode.LocalOnly;

        [SerializeField, Tooltip("Unique identifier for network synchronization. Auto-generated from asset name if empty.")]
        private string _channelId = "";

        /// <summary>
        /// The network synchronization mode.
        /// </summary>
        public NetworkEventMode NetworkMode
        {
            get => _networkMode;
            set => _networkMode = value;
        }

        /// <summary>
        /// Unique identifier for this channel on the network.
        /// </summary>
        public string ChannelId => string.IsNullOrEmpty(_channelId) ? name : _channelId;

        /// <summary>
        /// Raises this event with network synchronization based on NetworkMode.
        /// </summary>
        public new void Raise(T value)
        {
            switch (_networkMode)
            {
                case NetworkEventMode.LocalOnly:
                    base.Raise(value);
                    break;

                case NetworkEventMode.Broadcast:
                    SendOverNetwork(value, NetworkEventTarget.All);
                    break;

                case NetworkEventMode.BroadcastOthers:
                    SendOverNetwork(value, NetworkEventTarget.Others);
                    break;

                case NetworkEventMode.ServerOnly:
                    SendOverNetwork(value, NetworkEventTarget.Server);
                    break;

                case NetworkEventMode.LocalAndBroadcast:
                    base.Raise(value);
                    SendOverNetwork(value, NetworkEventTarget.Others);
                    break;
            }
        }

        /// <summary>
        /// Raises this event locally only, ignoring network settings.
        /// Use this when receiving events from the network.
        /// </summary>
        public void RaiseLocal(T value)
        {
            base.Raise(value);
        }

        private void SendOverNetwork(T value, NetworkEventTarget target)
        {
            if (!NetworkEventManager.IsNetworkAvailable)
            {
                // Fallback to local if no network
                base.Raise(value);
                return;
            }

            byte[] data = SerializeValue(value);
            NetworkEventManager.SendEvent(ChannelId, data, target);
        }

        /// <summary>
        /// Serializes the value to bytes for network transmission.
        /// Override this for custom serialization.
        /// </summary>
        protected virtual byte[] SerializeValue(T value)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// Deserializes bytes back to the value type.
        /// Override this for custom deserialization.
        /// </summary>
        public virtual T DeserializeValue(byte[] data)
        {
            string json = System.Text.Encoding.UTF8.GetString(data);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
}
