using System.Threading.Tasks;

namespace Eraflo.Catalyst.Core.Save
{
    /// <summary>
    /// Interface for data storage backends (Disk, Cloud, etc.).
    /// </summary>
    public interface IStorageBackend
    {
        /// <summary>
        /// Saves the provided data with the given name.
        /// </summary>
        Task SaveAsync(string name, byte[] data);

        /// <summary>
        /// Loads data with the given name.
        /// </summary>
        Task<byte[]> LoadAsync(string name);

        /// <summary>
        /// Deletes the save data with the given name.
        /// </summary>
        Task DeleteAsync(string name);

        /// <summary>
        /// Checks if a save with the given name exists.
        /// </summary>
        bool Exists(string name);
    }
}
