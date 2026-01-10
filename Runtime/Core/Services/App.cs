namespace Eraflo.Catalyst
{
    /// <summary>
    /// Static entry point to access registered services.
    /// </summary>
    public static class App
    {
        /// <summary>
        /// Retrieves a service of type T.
        /// </summary>
        /// <typeparam name="T">The type or interface of the service.</typeparam>
        /// <returns>The service instance, or null if not found.</returns>
        public static T Get<T>() where T : class
        {
            return ServiceLocator.Get<T>();
        }
    }
}
