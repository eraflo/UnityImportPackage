using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eraflo.Catalyst.Assets
{
    /// <summary>
    /// Service for managing assets with reference counting and caching.
    /// Interacts with different providers (Resources, Addressables).
    /// </summary>
    [Service(Priority = 20)]
    public class AssetManager : IGameService
    {
        private readonly Dictionary<string, AssetEntry> _cache = new Dictionary<string, AssetEntry>();
        private IAssetProvider _provider;

        private class AssetEntry
        {
            public Object Asset;
            public int ReferenceCount;
            public TaskCompletionSource<Object> LoadingTask;
        }

        public void Initialize()
        {
            var settings = PackageSettings.Instance;
            switch (settings.AssetProviderType)
            {
                case AssetProviderType.Resources:
                    _provider = new ResourcesProvider();
                    break;
                case AssetProviderType.Addressables:
                    _provider = new AddressablesProvider();
                    break;
                default:
                    _provider = new ResourcesProvider();
                    break;
            }
        }

        public void Shutdown()
        {
            foreach (var entry in _cache.Values)
            {
                if (entry.Asset != null)
                {
                    _provider.Release(entry.Asset);
                }
            }
            _cache.Clear();
        }

        /// <summary>
        /// Switches the current asset provider.
        /// </summary>
        public void SetProvider(IAssetProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Loads an asset asynchronously or returns it from cache if already loaded.
        /// Increments the reference count.
        /// </summary>
        public async Task<AssetHandle<T>> LoadAsync<T>(string key) where T : Object
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.LoadingTask != null)
                {
                    var cachedAsset = await entry.LoadingTask.Task;
                    entry.ReferenceCount++;
                    return new AssetHandle<T>(key, cachedAsset as T);
                }

                entry.ReferenceCount++;
                return new AssetHandle<T>(key, entry.Asset as T);
            }

            // Create new entry
            entry = new AssetEntry
            {
                ReferenceCount = 1,
                LoadingTask = new TaskCompletionSource<Object>()
            };
            _cache[key] = entry;

            try
            {
                var asset = await _provider.LoadAsync<T>(key);
                if (asset == null)
                {
                    _cache.Remove(key);
                    entry.LoadingTask.SetException(new Exception($"Asset not found: {key}"));
                    return null;
                }

                entry.Asset = asset;
                entry.LoadingTask.SetResult(asset);
                entry.LoadingTask = null;
                return new AssetHandle<T>(key, asset);
            }
            catch (Exception e)
            {
                _cache.Remove(key);
                entry.LoadingTask?.SetException(e);
                throw;
            }
        }

        /// <summary>
        /// Decrements the reference count of an asset and unloads it if it reaches zero.
        /// Called automatically by AssetHandle.Dispose().
        /// </summary>
        public void Release(AssetHandle handle)
        {
            if (handle == null || string.IsNullOrEmpty(handle.Key)) return;

            if (_cache.TryGetValue(handle.Key, out var entry))
            {
                entry.ReferenceCount--;
                if (entry.ReferenceCount <= 0)
                {
                    if (entry.Asset != null)
                    {
                        _provider.Release(entry.Asset);
                    }
                    _cache.Remove(handle.Key);
                }
            }
        }
    }
}
