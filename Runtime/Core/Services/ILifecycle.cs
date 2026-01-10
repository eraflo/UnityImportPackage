namespace Eraflo.Catalyst
{
    /// <summary>
    /// Interface for services that require a frame-rate dependent update.
    /// maps to Unity's Update phase.
    /// </summary>
    public interface IUpdatable
    {
        void OnUpdate();
    }

    /// <summary>
    /// Interface for services that require a fixed-rate update.
    /// maps to Unity's FixedUpdate phase.
    /// </summary>
    public interface IFixedUpdatable
    {
        void OnFixedUpdate();
    }
}
