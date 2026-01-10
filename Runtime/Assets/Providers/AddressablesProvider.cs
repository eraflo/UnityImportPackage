using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Eraflo.Catalyst.Assets
{
    /// <summary>
    /// Provider using Unity's Addressables system.
    /// </summary>
    public class AddressablesProvider : IAssetProvider
    {
        private readonly Dictionary<Object, AsyncOperationHandle> _handles = new Dictionary<Object, AsyncOperationHandle>();

        public async Task<T> LoadAsync<T>(string key) where T : Object
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            var result = await handle.Task;
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _handles[result] = handle;
                return result;
            }
            
            Addressables.Release(handle);
            throw new System.Exception($"Failed to load addressable: {key}");
        }

        public void Release(Object asset)
        {
            if (_handles.TryGetValue(asset, out var handle))
            {
                Addressables.Release(handle);
                _handles.Remove(asset);
            }
            else
            {
                // Fallback for cases where handle wasn't tracked or managed differently
                Addressables.Release(asset);
            }
        }
    }
}
