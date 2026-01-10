using System;
using UnityEngine;
using Eraflo.Catalyst;

namespace Eraflo.Catalyst.Networking
{
    /// <summary>
    /// Central network manager facade.
    /// </summary>
    /// <summary>
    /// Central network manager API.
    /// Can be used as a static facade or as a service via Service Locator.
    /// </summary>
    [Service(Priority = 20)]
    public class NetworkManager : IGameService, INetworkService
    {
        private INetworkBackend _backend;
        private readonly NetworkBackendRegistry _backends = new NetworkBackendRegistry();
        private readonly NetworkMessageRouter _router = new NetworkMessageRouter();
        private readonly NetworkHandlerRegistry _handlers = new NetworkHandlerRegistry();

        public INetworkBackend Backend => _backend;
        public NetworkBackendRegistry Backends => _backends;
        public NetworkMessageRouter Router => _router;
        public NetworkHandlerRegistry Handlers => _handlers;

        public bool HasBackend => _backend != null;
        public bool IsServer => _backend?.IsServer ?? true;
        public bool IsClient => _backend?.IsClient ?? false;
        public bool IsConnected => _backend?.IsConnected ?? false;
        public bool IsHost => IsServer && IsClient;
        public ulong LocalClientId => _backend?.LocalClientId ?? 0;

        public event Action<INetworkBackend> OnBackendChanged
        {
            add => _onBackendChanged += value;
            remove => _onBackendChanged -= value;
        }

        public event Action OnConnected
        {
            add => _onConnected += value;
            remove => _onConnected -= value;
        }

        public event Action OnDisconnected
        {
            add => _onDisconnected += value;
            remove => _onDisconnected -= value;
        }

        #region IGameService

        void IGameService.Initialize()
        {
            // Initialization logic if needed
        }

        void IGameService.Shutdown()
        {
            Reset();
        }

        #endregion


        #region Instance Events

        private event Action<INetworkBackend> _onBackendChanged;
        private event Action _onConnected;
        private event Action _onDisconnected;

        #endregion

        #region Instance Methods

        public bool SetBackendById(string id)
        {
            var backend = _backends.Create(id);
            if (backend == null)
            {
                Debug.LogWarning($"[NetworkManager] Backend not found: {id}");
                return false;
            }
            SetBackend(backend);
            return true;
        }

        public void SetBackend(INetworkBackend backend)
        {
            bool wasConnected = _backend?.IsConnected ?? false;

            if (_backend != null)
            {
                if (wasConnected) _handlers.NotifyDisconnected();
                _backend.Shutdown();
            }

            _backend = backend;

            if (_backend != null)
            {
                _backend.Initialize();
                
                // Wire router to backend
                _router.OnTypeRegistered += msgId =>
                {
                    if (_backend != null)
                        _backend.RegisterHandler(msgId, (data, senderId) => _router.Route(msgId, data, senderId));
                };
                _router.OnTypeUnregistered += msgId =>
                {
                    if (_backend != null)
                        _backend.UnregisterHandler(msgId);
                };
                
                if (_backend.IsConnected) _handlers.NotifyConnected();
            }

            _onBackendChanged?.Invoke(_backend);

            if (PackageSettings.Instance.NetworkDebugMode)
            {
                Debug.Log($"[NetworkManager] Backend: {backend?.GetType().Name ?? "none"}");
            }
        }

        public void Send<T>(T message, NetworkTarget target = NetworkTarget.All) where T : struct, INetworkMessage
        {
            if (_backend == null || !_backend.IsConnected) return;

            var msgId = _router.GetId<T>();
            var data = NetworkSerializer.Serialize(message);
            _backend.Send(msgId, data, target);

            if (PackageSettings.Instance.NetworkDebugMode)
            {
                Debug.Log($"[NetworkManager] Sent {typeof(T).Name}");
            }
        }

        public void SendToClient<T>(T message, ulong clientId) where T : struct, INetworkMessage
        {
            if (_backend == null || !_backend.IsConnected || !_backend.IsServer) return;

            var msgId = _router.GetId<T>();
            var data = NetworkSerializer.Serialize(message);
            _backend.SendToClient(msgId, data, clientId);

            if (PackageSettings.Instance.NetworkDebugMode)
            {
                Debug.Log($"[NetworkManager] Sent {typeof(T).Name} to client {clientId}");
            }
        }

        public void SendToClients<T>(T message, params ulong[] clientIds) where T : struct, INetworkMessage
        {
            if (_backend == null || !_backend.IsConnected || !_backend.IsServer) return;

            var msgId = _router.GetId<T>();
            var data = NetworkSerializer.Serialize(message);
            _backend.SendToClients(msgId, data, clientIds);

            if (PackageSettings.Instance.NetworkDebugMode)
            {
                Debug.Log($"[NetworkManager] Sent {typeof(T).Name} to {clientIds.Length} clients");
            }
        }

        public void SendToServer<T>(T message) where T : struct, INetworkMessage
            => Send(message, NetworkTarget.Server);

        public void SendToClients<T>(T message) where T : struct, INetworkMessage
            => Send(message, NetworkTarget.Clients);

        public void On<T>(Action<T> handler) where T : struct, INetworkMessage
            => _router.On(handler);

        public void Off<T>(Action<T> handler) where T : struct, INetworkMessage
            => _router.Off(handler);

        public void NotifyConnected()
        {
            _handlers.NotifyConnected();
            _onConnected?.Invoke();
        }

        public void NotifyDisconnected()
        {
            _handlers.NotifyDisconnected();
            _onDisconnected?.Invoke();
        }

        public void Reset()
        {
            _handlers.Clear();
            _router.Clear();
            SetBackend(null);
            _backends.Clear();
        }

        #endregion
    }
}
