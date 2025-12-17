# Event Bus System

A unified, thread-safe event system for Unity. Works via **code**, **inspector**, or **network**.

---

## Table of Contents

1. [Package Settings](#package-settings)
2. [Quick Start](#quick-start)
3. [Architecture](#architecture)
4. [Built-in Channels](#built-in-channels)
5. [Auto-Subscribe Attribute](#auto-subscribe-attribute)
6. [Creating Custom Channels](#creating-custom-channels)
7. [Network Events](#network-events)
8. [Netcode for GameObjects Integration](#netcode-for-gameobjects-integration)
9. [API Reference](#api-reference)
10. [Best Practices](#best-practices)

---

## Package Settings

A configuration ScriptableObject is **automatically created** when you import the package.

**Location**: `Assets/Resources/UnityImportPackageSettings.asset`

**Access**: Menu > **Tools > Unity Import Package > Settings**

### Settings

| Setting | Description |
|---------|-------------|
| **Enable Networking** | Auto-instantiate `NetworkEventManager` singleton on game start |
| **Debug Mode** | Log network event messages to console |

When **Enable Networking** is checked:
- A `NetworkEventManager` singleton is created automatically at runtime
- It persists across scenes with `DontDestroyOnLoad`
- No manual setup required!

---

## Quick Start

### Via Code
```csharp
using Eraflo.UnityImportPackage.Events;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] IntEventChannel onScoreChanged;

    void OnEnable() => onScoreChanged.Subscribe(OnScore);
    void OnDisable() => onScoreChanged.Unsubscribe(OnScore);
    
    void OnScore(int score) => Debug.Log($"Score: {score}");
    
    public void AddScore(int points) => onScoreChanged.Raise(points);
}
```

### Via Inspector
1. **Create Channel**: Right-click > Create > Events > [Type] Channel
2. **Add Listener**: Add Component > Events > [Type] Channel Listener
3. **Configure**: Drag channel asset, set up UnityEvent response
4. **Raise**: Call `channel.Raise()` from any script

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│               EventBus (static)                     │
│          Thread-safe subscription manager           │
└──────────────────────┬──────────────────────────────┘
                       │
         ┌─────────────┼─────────────┐
         ▼             ▼             ▼
┌────────────────┐ ┌────────────────┐ ┌────────────────┐
│ Code Subscribe │ │ Inspector      │ │ Network        │
│ channel.Sub()  │ │ Listener       │ │ Sync           │
└────────────────┘ └────────────────┘ └────────────────┘
```

### Safety Features

| Feature | Implementation |
|---------|----------------|
| **Thread-safe** | `lock` on all dictionary operations |
| **Async-safe** | Copy subscribers before iteration |
| **Editor-safe** | Raise button disabled outside Play mode |

---

## Built-in Channels

### Local Channels

| Type | Channel | Listener | Menu |
|------|---------|----------|------|
| void | `EventChannel` | `EventChannelListener` | Events/Event Channel |
| int | `IntEventChannel` | `IntEventChannelListener` | Events/Int Channel |
| float | `FloatEventChannel` | `FloatEventChannelListener` | Events/Float Channel |
| string | `StringEventChannel` | `StringEventChannelListener` | Events/String Channel |
| bool | `BoolEventChannel` | `BoolEventChannelListener` | Events/Bool Channel |
| Vector3 | `Vector3EventChannel` | `Vector3EventChannelListener` | Events/Vector3 Channel |

### Network Channels

| Type | Channel | Menu |
|------|---------|------|
| void | `NetworkEventChannel` | Events/Network/Event Channel |
| int | `NetworkIntEventChannel` | Events/Network/Int Channel |
| float | `NetworkFloatEventChannel` | Events/Network/Float Channel |
| string | `NetworkStringEventChannel` | Events/Network/String Channel |
| bool | `NetworkBoolEventChannel` | Events/Network/Bool Channel |
| Vector3 | `NetworkVector3EventChannel` | Events/Network/Vector3 Channel |

### Addressables Integration

EventChannels are **automatically registered** to Addressables when created.

**Auto-generated structure:**
- **Group**: `EventChannels`
- **Address**: `Events/{Type}/{AssetName}` (e.g., `Events/Int/OnScoreChanged`)

**Manual registration**: Menu > **Tools > Unity Import Package > Register All EventChannels to Addressables**

#### Exemple concret : Système de Mods

Imaginons un jeu avec des mods. Chaque mod peut ajouter ses propres événements dans un fichier JSON :

```json
// mod_config.json
{
    "events": [
        "Events/Int/OnCustomBossDefeated",
        "Events/Void/OnSecretFound"
    ]
}
```

Le `EventChannelLoader` permet de charger ces events dynamiquement :

```csharp
using Eraflo.UnityImportPackage.Events;
using UnityEngine;
using System.Collections.Generic;

public class ModLoader : MonoBehaviour
{
    private List<IntEventChannel> _loadedEvents = new();

    void Start()
    {
        // Lire le fichier de config du mod
        var modConfig = LoadModConfig("mod_config.json");
        
        // Charger chaque event dynamiquement
        foreach (string eventAddress in modConfig.events)
        {
            EventChannelLoader.LoadAsync<IntEventChannel>(eventAddress, channel =>
            {
                if (channel != null)
                {
                    _loadedEvents.Add(channel);
                    channel.Subscribe(OnModEvent);
                    Debug.Log($"Mod event loaded: {eventAddress}");
                }
            });
        }
    }

    void OnModEvent(int value)
    {
        Debug.Log($"Mod event triggered with value: {value}");
    }
}
```

> **Note**: Si tu utilises uniquement `[SerializeField]` pour référencer tes events, tu n'as **pas besoin** du `EventChannelLoader`. Il est uniquement utile pour le chargement dynamique.

---

## Auto-Subscribe Attribute

Simplify subscription with `[SubscribeTo]` attribute:

```csharp
using Eraflo.UnityImportPackage.Events;

public class PlayerUI : EventSubscriber  // Inherit from EventSubscriber
{
    [SerializeField] IntEventChannel onHealthChanged;
    [SerializeField] EventChannel onPlayerDied;

    [SubscribeTo(nameof(onHealthChanged))]
    void OnHealthChanged(int health)
    {
        healthBar.value = health;
    }

    [SubscribeTo(nameof(onPlayerDied))]
    void OnPlayerDied()
    {
        gameOverScreen.SetActive(true);
    }
}
```

> **Note**: No need for `OnEnable`/`OnDisable` - subscription is handled automatically!

---

## Creating Custom Channels

### Step 1: Create the Channel

```csharp
using UnityEngine;
using Eraflo.UnityImportPackage.Events;

[CreateAssetMenu(menuName = "Events/Player Data Channel")]
public class PlayerDataChannel : EventChannel<PlayerData> { }

[System.Serializable]
public struct PlayerData
{
    public string Name;
    public int Score;
    public Vector3 Position;
}
```

### Step 2: Create the Listener (Optional)

```csharp
using Eraflo.UnityImportPackage.Events;

public class PlayerDataChannelListener : EventChannelListener<PlayerDataChannel, PlayerData> { }
```

> **Note**: A custom editor is automatically applied to all `EventChannel<T>` types!

---

## Network Events

### Network Modes

| Mode | Behavior |
|------|----------|
| `LocalOnly` | No network sync (default) |
| `Broadcast` | Send to all clients (including self) |
| `BroadcastOthers` | Send to all except self |
| `ServerOnly` | Client → Server only |
| `LocalAndBroadcast` | Raise locally + send to others |

### Setup

1. Create a network channel: **Create > Events > Network > [Type] Channel**
2. Set the **Network Mode** in the inspector
3. Implement `INetworkEventHandler` for your networking solution
4. Register the handler on network start

### Receiving Network Events

```csharp
// When receiving an event from the network, use RaiseLocal
// to avoid re-sending it over the network
networkChannel.RaiseLocal(receivedValue);
```

---

## Netcode for GameObjects Integration

### Step 1: Create the Handler

```csharp
using Unity.Netcode;
using Eraflo.UnityImportPackage.Events;

public class NetcodeEventHandler : NetworkBehaviour, INetworkEventHandler
{
    public static NetcodeEventHandler Instance { get; private set; }

    public bool IsServer => NetworkManager.Singleton?.IsServer ?? false;
    public bool IsConnected => NetworkManager.Singleton?.IsConnectedClient ?? false;

    public override void OnNetworkSpawn()
    {
        Instance = this;
        NetworkEventManager.RegisterHandler(this);
    }

    public override void OnNetworkDespawn()
    {
        NetworkEventManager.UnregisterHandler();
        Instance = null;
    }

    public void SendEvent(string channelId, byte[] data, NetworkEventTarget target)
    {
        switch (target)
        {
            case NetworkEventTarget.All:
                BroadcastEventClientRpc(channelId, data);
                break;
            case NetworkEventTarget.Others:
                BroadcastToOthersClientRpc(channelId, data);
                break;
            case NetworkEventTarget.Server:
                SendToServerServerRpc(channelId, data);
                break;
        }
    }

    [ClientRpc]
    private void BroadcastEventClientRpc(string channelId, byte[] data)
    {
        HandleReceivedEvent(channelId, data);
    }

    [ClientRpc]
    private void BroadcastToOthersClientRpc(string channelId, byte[] data)
    {
        // Server already raised locally, skip
        if (!IsServer)
        {
            HandleReceivedEvent(channelId, data);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendToServerServerRpc(string channelId, byte[] data)
    {
        HandleReceivedEvent(channelId, data);
    }

    private void HandleReceivedEvent(string channelId, byte[] data)
    {
        // Find the channel by ID and raise locally
        // You can use a registry or Resources.FindObjectsOfTypeAll
        var channels = Resources.FindObjectsOfTypeAll<NetworkEventChannel>();
        foreach (var channel in channels)
        {
            if (channel.ChannelId == channelId)
            {
                channel.RaiseLocal();
                return;
            }
        }

        // For typed channels, implement similar logic
    }
}
```

### Step 2: Add to Scene

1. Create an empty GameObject named "NetworkEventHandler"
2. Add the `NetcodeEventHandler` component
3. Make sure it spawns with your NetworkManager

### Step 3: Use Network Channels

```csharp
[SerializeField] NetworkIntEventChannel onScoreChanged;

void AddScore(int points)
{
    // Automatically syncs based on NetworkMode setting
    onScoreChanged.Raise(points);
}
```

---

## API Reference

### EventChannel / EventChannel\<T\>

| Method | Description |
|--------|-------------|
| `Raise()` / `Raise(T value)` | Notifies all subscribers |
| `Subscribe(Action)` / `Subscribe(Action<T>)` | Adds a subscriber |
| `Unsubscribe(Action)` / `Unsubscribe(Action<T>)` | Removes a subscriber |
| `SubscriberCount` | Number of current subscribers |

### NetworkEventChannel / NetworkEventChannel\<T\>

| Method | Description |
|--------|-------------|
| `Raise()` / `Raise(T value)` | Raises with network sync based on mode |
| `RaiseLocal()` / `RaiseLocal(T value)` | Raises locally only (for incoming network events) |
| `NetworkMode` | Get/set the sync mode |
| `ChannelId` | Unique ID for network identification |

### EventBus (Static)

| Method | Description |
|--------|-------------|
| `ClearAll()` | Removes all subscriptions |
| `Clear(channel)` | Removes subscriptions for a specific channel |

### NetworkEventManager (Static)

| Method | Description |
|--------|-------------|
| `RegisterHandler(INetworkEventHandler)` | Sets the network handler |
| `UnregisterHandler()` | Removes the network handler |
| `IsNetworkAvailable` | Whether network is connected |
| `IsServer` | Whether local client is server |

---

## Best Practices

1. **Always unsubscribe** in `OnDisable()` to prevent memory leaks (or use `EventSubscriber`)
2. **Use typed channels** for data-carrying events
3. **Call `EventBus.ClearAll()`** on scene transitions if needed
4. **Keep channels as assets** in a dedicated folder (e.g., `Assets/Events/`)
5. **Use `RaiseLocal()`** when receiving network events to avoid infinite loops
6. **Set meaningful Channel IDs** for network channels to ensure proper routing
