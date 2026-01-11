using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Eraflo.Catalyst
{
    /// <summary>
    /// Interface that wraps Unity's SceneManager for abstraction and testability.
    /// </summary>
    public interface ISceneManager : IGameService
    {
        /// <summary>
        /// Loads the scene with the specified name and mode.
        /// </summary>
        Task LoadSceneAsync(string name, LoadSceneMode mode, Action<float> onProgress = null);

        /// <summary>
        /// Unloads the specified scene.
        /// </summary>
        Task UnloadSceneAsync(Scene scene);

        /// <summary>
        /// Sets the specified scene as the active scene.
        /// </summary>
        void SetActiveScene(Scene scene);

        /// <summary>
        /// Returns the scene at the specified index.
        /// </summary>
        Scene GetSceneAt(int index);

        /// <summary>
        /// Returns the scene with the specified name.
        /// </summary>
        Scene GetSceneByName(string name);

        /// <summary>
        /// Gets the total number of loaded scenes.
        /// </summary>
        int SceneCount { get; }
    }
}
