using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Eraflo.Catalyst.Core.Save
{
    /// <summary>
    /// Implementation of IStorageBackend for local disk storage.
    /// </summary>
    public class LocalDiskStorage : IStorageBackend
    {
        private string RootPath => Path.Combine(Application.persistentDataPath, "Saves");

        public LocalDiskStorage()
        {
            if (!Directory.Exists(RootPath))
            {
                Directory.CreateDirectory(RootPath);
            }
        }

        public Task SaveAsync(string name, byte[] data)
        {
            string path = GetPath(name);
            File.WriteAllBytes(path, data);
            return Task.CompletedTask;
        }

        public Task<byte[]> LoadAsync(string name)
        {
            string path = GetPath(name);
            if (!File.Exists(path)) return Task.FromResult<byte[]>(null);
            return Task.FromResult(File.ReadAllBytes(path));
        }

        public Task DeleteAsync(string name)
        {
            string path = GetPath(name);
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }

        public bool Exists(string name)
        {
            return File.Exists(GetPath(name));
        }

        private string GetPath(string name) => Path.Combine(RootPath, $"{name}.save");
    }
}
