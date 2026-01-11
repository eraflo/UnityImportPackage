using System.Threading.Tasks;

namespace Eraflo.Catalyst
{
    /// <summary>
    /// Interface for loading screen UI to decouple it from the SceneLoaderService.
    /// </summary>
    public interface ILoadingScreen : IGameService
    {
        /// <summary>
        /// Starts the transition (e.g., Fade In).
        /// </summary>
        Task Show();

        /// <summary>
        /// Ends the transition (e.g., Fade Out).
        /// </summary>
        Task Hide();

        /// <summary>
        /// Updates the current loading progress (0.0 to 1.0).
        /// </summary>
        /// <param name="value">The progress value.</param>
        void UpdateProgress(float value);
    }
}
