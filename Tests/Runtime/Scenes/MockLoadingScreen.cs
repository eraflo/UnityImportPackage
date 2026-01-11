using System.Threading.Tasks;

namespace Eraflo.Catalyst.Tests
{
    public class MockLoadingScreen : ILoadingScreen
    {
        public void Initialize() { }
        public void Shutdown() { }

        public bool IsShowing { get; private set; }
        public float Progress { get; private set; }
        public int ShowCount { get; private set; }
        public int HideCount { get; private set; }

        public Task Show()
        {
            IsShowing = true;
            ShowCount++;
            return Task.CompletedTask;
        }

        public Task Hide()
        {
            IsShowing = false;
            HideCount++;
            return Task.CompletedTask;
        }

        public void UpdateProgress(float value)
        {
            Progress = value;
        }
    }
}
