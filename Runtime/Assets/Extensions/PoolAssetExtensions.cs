using System.Threading.Tasks;
using Eraflo.Catalyst.Pooling;
using UnityEngine;

namespace Eraflo.Catalyst.Assets
{
    /// <summary>
    /// Extensions to bridge Asset Management and Pooling systems.
    /// </summary>
    public static class PoolAssetExtensions
    {
        /// <summary>
        /// Loads a prefab via AssetManager and warms up its pool.
        /// </summary>
        /// <param name="pool">The pool manager.</param>
        /// <param name="key">The asset key.</param>
        /// <param name="prewarmCount">Number of instances to pre-spawn.</param>
        /// <returns>An AssetHandle containing the prefab. Dispose the handle to allow unloading (after clearing the pool if needed).</returns>
        public static async Task<AssetHandle<GameObject>> LoadAndPoolAsync(this Pool pool, string key, int prewarmCount)
        {
            var assetManager = App.Get<AssetManager>();
            if (assetManager == null)
            {
                Debug.LogError("[AssetExtensions] AssetManager service not found.");
                return null;
            }

            var handle = await assetManager.LoadAsync<GameObject>(key);
            
            if (handle != null && handle.Result != null)
            {
                pool.WarmupObject(handle.Result, prewarmCount);
            }
            
            return handle;
        }
    }
}
