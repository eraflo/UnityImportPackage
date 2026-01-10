using System;
using Object = UnityEngine.Object;

namespace Eraflo.Catalyst.Assets
{
    /// <summary>
    /// Base class for asset handles to allow non-generic reference.
    /// </summary>
    public abstract class AssetHandle : IDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Key { get; internal set; }
        public abstract Object RawResult { get; }
        public abstract void Dispose();
    }

    /// <summary>
    /// Lightweight handle for a loaded asset.
    /// Implements IDisposable for easy release.
    /// </summary>
    /// <typeparam name="T">Type of the asset.</typeparam>
    public class AssetHandle<T> : AssetHandle where T : Object
    {
        private T _result;
        private bool _disposed;

        public T Result => _result;
        public override Object RawResult => _result;

        internal AssetHandle(string key, T result)
        {
            Key = key;
            _result = result;
        }

        public override void Dispose()
        {
            if (_disposed) return;
            
            var assetManager = App.Get<AssetManager>();
            if (assetManager != null)
            {
                assetManager.Release(this);
            }
            
            _disposed = true;
            _result = null;
        }
    }
}
