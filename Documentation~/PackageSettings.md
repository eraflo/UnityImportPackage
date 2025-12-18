# Package Settings

Central configuration for the UnityImportPackage.

---

## Location

The configuration file is **automatically created** when the package is imported:

```
Assets/Resources/UnityImportPackageSettings.asset
```

---

## Access

### Via Menu
**Tools > Unity Import Package > Settings**

### Via Code
```csharp
using Eraflo.UnityImportPackage;

var settings = PackageSettings.Instance;
bool networkEnabled = settings.EnableNetworking;
bool debugMode = settings.NetworkDebugMode;
```

---

## Settings

| Setting | Type | Description |
|---------|------|-------------|
| **Enable Networking** | `bool` | Enables automatic instantiation of `NetworkEventManager` |
| **Network Debug Mode** | `bool` | Displays network debug logs in the console |

---

## Runtime Behavior

### If Enable Networking = true

At game launch (`RuntimeInitializeOnLoadMethod`):
1. A `[NetworkEventManager]` GameObject is created
2. `DontDestroyOnLoad` is applied → persists across scenes
3. Ready to receive an `INetworkEventHandler`

```
[PackageInitializer] NetworkEventManager initialized
```

### If Enable Networking = false

No automatic action. You must handle it manually:
```csharp
var go = new GameObject("NetworkEventManager");
go.AddComponent<NetworkEventManagerBehaviour>();
DontDestroyOnLoad(go);
```

---

## Force Reload

If you modify settings during execution:
```csharp
PackageSettings.Reload();
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "No settings found" warning | Create via **Tools > Unity Import Package > Create Settings** |
| Settings not applied | Verify the file is in `Assets/Resources/` |
| NetworkEventManager missing | Enable **Enable Networking** in settings |
