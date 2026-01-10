namespace Eraflo.Catalyst.Core.Save
{
    /// <summary>
    /// Interface for components that need to save their state.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// Returns the object representing the state to be saved.
        /// This should be a serializable object.
        /// </summary>
        object SaveState();

        /// <summary>
        /// Restores the state from the provided object.
        /// </summary>
        void LoadState(object state);
    }
}
