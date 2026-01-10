using System.Threading.Tasks;
using UnityEngine;

namespace Eraflo.Catalyst.Assets
{
    /// <summary>
    /// Base interface for asset loading providers.
    /// </summary>
    public interface IAssetProvider
    {
        /// <summary>
        /// Loads an asset of type T asynchronously.
        /// </summary>
        Task<T> LoadAsync<T>(string key) where T : Object;

        /// <summary>
        /// Releases a loaded asset.
        /// </summary>
        void Release(Object asset);
    }
}
