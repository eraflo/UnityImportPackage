namespace Eraflo.Catalyst
{
    /// <summary>
    /// Base interface for all services managed by the Service Locator.
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Called when the service is registered and instantiated.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Called when the application shuts down or the service is removed.
        /// </summary>
        void Shutdown();
    }
}
