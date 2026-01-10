using System.Threading.Tasks;
using UnityEngine;

namespace Eraflo.Catalyst.Assets
{
    /// <summary>
    /// Provider using Unity's Resources system.
    /// </summary>
    public class ResourcesProvider : IAssetProvider
    {
        public async Task<T> LoadAsync<T>(string key) where T : Object
        {
            var request = Resources.LoadAsync<T>(key);
            while (!request.isDone)
            {
                await Task.Yield();
            }
            return request.asset as T;
        }

        public void Release(Object asset)
        {
            // Resources.UnloadAsset only works for non-GameObject/Component assets.
            // For GameObjects loaded via Resources.Load, we generally don't unload them explicitly 
            // unless they are specific assets like Textures or Meshes.
            if (!(asset is GameObject) && !(asset is Component))
            {
                Resources.UnloadAsset(asset);
            }
        }
    }
}
