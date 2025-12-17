using UnityEngine;

namespace Eraflo.UnityImportPackage
{
    /// <summary>
    /// Automatically initializes package systems based on PackageSettings.
    /// </summary>
    public static class PackageInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var settings = PackageSettings.Instance;

            if (settings.EnableNetworking)
            {
                InitializeNetworking(settings);
            }
        }

        private static void InitializeNetworking(PackageSettings settings)
        {
            // Create persistent NetworkEventManager
            var go = new GameObject("[NetworkEventManager]");
            go.AddComponent<Events.NetworkEventManagerBehaviour>();
            Object.DontDestroyOnLoad(go);

            if (settings.NetworkDebugMode)
            {
                Debug.Log("[PackageInitializer] NetworkEventManager initialized");
            }
        }
    }
}
