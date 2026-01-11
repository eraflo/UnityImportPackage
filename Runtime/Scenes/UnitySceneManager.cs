using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eraflo.Catalyst
{
    /// <summary>
    /// Default implementation of ISceneManager that wraps UnityEngine.SceneManagement.SceneManager.
    /// </summary>
    public class UnitySceneManager : ISceneManager
    {
        public void Initialize() { }
        public void Shutdown() { }

        public async Task LoadSceneAsync(string name, LoadSceneMode mode, Action<float> onProgress = null)
        {
            var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name, mode);
            if (op == null) return;

            while (!op.isDone)
            {
                onProgress?.Invoke(op.progress);
                await Task.Yield();
            }
            onProgress?.Invoke(1.0f);
        }

        public async Task UnloadSceneAsync(Scene scene)
        {
            var op = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
            if (op == null) return;

            while (!op.isDone)
            {
                await Task.Yield();
            }
        }

        public void SetActiveScene(Scene scene)
        {
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
        }

        public Scene GetSceneAt(int index)
        {
            return UnityEngine.SceneManagement.SceneManager.GetSceneAt(index);
        }

        public Scene GetSceneByName(string name)
        {
            return UnityEngine.SceneManagement.SceneManager.GetSceneByName(name);
        }

        public int SceneCount => UnityEngine.SceneManagement.SceneManager.sceneCount;
    }
}
