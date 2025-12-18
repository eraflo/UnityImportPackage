# Timer System

A high-performance, handle-based timer system with automatic backend selection and Burst support.

## Quick Start

```csharp
using Eraflo.UnityImportPackage.Timers;

// Create timers
var countdown = Timer.Create<CountdownTimer>(5f);
var stopwatch = Timer.Create<StopwatchTimer>(0f);

// Simple delay
Timer.Delay(2f, () => Debug.Log("Done!"));

// Events (generic callback system)
Timer.On<OnComplete>(countdown, () => Debug.Log("Finished!"));
Timer.On<OnTick, float>(countdown, (dt) => Debug.Log($"Tick: {dt}"));

// Control
Timer.Pause(countdown);
Timer.Resume(countdown);
Timer.Cancel(countdown);

// Query
float progress = Timer.GetProgress(countdown);
bool done = Timer.IsFinished(countdown);
```

## File Structure

```
Runtime/Timers/
├── Core/
│   ├── Timer.cs              # Core API (Create, Control, Query)
│   ├── Timer.Events.cs       # Event registration (On<T>, Off<T>)
│   ├── Timer.Easing.cs       # Easing methods (Lerp, GetEasedProgress)
│   ├── TimerHandle.cs        # Handle struct
│   ├── ITimer.cs             # Interface + ICallbackCollector
│   ├── TimerCallbacks.cs     # Extensible callback registry
│   └── TimerBootstrapper.cs  # PlayerLoop integration
├── Types/
│   ├── CountdownTimer.cs     # Counts down (1 → 0)
│   ├── StopwatchTimer.cs     # Counts up indefinitely
│   ├── DelayTimer.cs         # One-shot countdown
│   ├── RepeatingTimer.cs     # Repeats every interval
│   └── FrequencyTimer.cs     # N ticks per second
├── Features/
│   ├── TimerChain.cs         # Fluent chaining API
│   ├── TimerGroup.cs         # Batch operations
│   └── NetworkTimerSync.cs   # Multiplayer sync
├── Backends/
│   ├── ITimerBackend.cs
│   ├── StandardBackend.cs    # Thread-safe managed backend
│   └── BurstBackend.cs       # Burst-compiled (optional)
└── Debugging/
    └── TimerDebugger.cs      # Runtime overlay (F5)
```

## Timer Types

| Type | Description | Supported Callbacks |
|------|-------------|---------------------|
| `CountdownTimer` | Counts down to 0 | OnComplete, OnTick, OnPause, OnResume, OnReset, OnCancel |
| `StopwatchTimer` | Counts up indefinitely | OnTick, OnPause, OnResume, OnReset, OnCancel |
| `DelayTimer` | One-shot countdown | OnComplete, OnTick, OnCancel |
| `RepeatingTimer` | Ticks every interval | OnComplete, OnRepeat, OnTick, OnPause, OnResume, OnReset, OnCancel |
| `FrequencyTimer` | N ticks per second | OnTick, OnPause, OnResume, OnReset, OnCancel |

## Extensible Callbacks

### Built-in Callbacks

```csharp
Timer.On<OnComplete>(handle, () => { });           // Timer finished
Timer.On<OnTick, float>(handle, (dt) => { });      // Every frame
Timer.On<OnRepeat, int>(handle, (count) => { });   // Repeat interval
Timer.On<OnPause>(handle, () => { });              // Paused
Timer.On<OnResume>(handle, () => { });             // Resumed
Timer.On<OnReset>(handle, () => { });              // Reset
Timer.On<OnCancel>(handle, () => { });             // Cancelled
```

### Custom Callbacks

```csharp
// 1. Define callback type
public struct OnDamage : ITimerCallback { }

// 2. Define data (any type)
public struct DamageData { public int Amount; public string Source; }

// 3. Create timer supporting the callback
public struct DamageTimer : ITimer, ISupportsCallback<OnDamage>
{
    private DamageData _pending;
    private bool _shouldFire;

    // ... ITimer implementation ...

    public void CollectCallbacks(ICallbackCollector collector)
    {
        if (_shouldFire)
        {
            collector.Trigger<OnDamage, DamageData>(_pending);
            _shouldFire = false;
        }
    }
}

// 4. Use it
var handle = Timer.Create<DamageTimer>(5f);
Timer.On<OnDamage, DamageData>(handle, (data) => {
    ApplyDamage(data.Amount, data.Source);
});
```

### Composite Interfaces

```csharp
// Use these instead of listing each callback:
public struct MyTimer : ITimer, ISupportsStandardCallbacks { }     // All 6 lifecycle callbacks
public struct MyTimer : ITimer, ISupportsIndefiniteCallbacks { }   // No OnComplete
public struct MyTimer : ITimer, ISupportsOneShotCallbacks { }      // OnComplete, OnTick, OnCancel
public struct MyTimer : ITimer, ISupportsRepeatingCallbacks { }    // Standard + OnRepeat
```

## Easing Integration

```csharp
using Eraflo.UnityImportPackage.EasingSystem;

var handle = Timer.Create<CountdownTimer>(2f);

// Eased progress
float eased = Timer.GetEasedProgress(handle, EasingType.ElasticOut);

// Lerp with easing
float value = Timer.Lerp(handle, 0f, 100f, EasingType.QuadInOut);
Vector2 pos2D = Timer.Lerp(handle, startPos, endPos, EasingType.SineInOut);
Vector3 pos3D = Timer.Lerp(handle, startPos, endPos, EasingType.BounceOut);
Quaternion rot = Timer.Lerp(handle, startRot, endRot, EasingType.BackOut);
Color color = Timer.Lerp(handle, Color.red, Color.blue, EasingType.Linear);

// Unclamped (for Elastic/Back)
float unclamped = Timer.LerpUnclamped(handle, 0f, 10f, EasingType.ElasticOut);
```

**Available Easing Types:** Linear, Quad, Cubic, Quart, Quint, Sine, Expo, Circ, Elastic, Back, Bounce (In/Out/InOut variants)

## Timer Chaining

```csharp
Timer.Chain()
    .Delay(1f)
    .Then(() => Debug.Log("Step 1"))
    .Delay(2f)
    .Then(() => Debug.Log("Step 2"))
    .Loop(3, 0.5f, (i) => Debug.Log($"Loop {i}"))
    .Start();

// Control
var chain = Timer.Chain().Delay(5f).Then(DoSomething).Start();
chain.Pause();
chain.Resume();
chain.Cancel();
```

## Timer Groups

```csharp
var group = Timer.CreateGroup("UI Timers");

var h1 = group.Create<CountdownTimer>(5f);
var h2 = group.Delay(3f, () => Debug.Log("Done"));

// Batch operations
group.PauseAll();
group.ResumeAll();
group.SetTimeScaleAll(0.5f);
group.CancelAll();
group.CleanupFinished();
```

## Multiplayer Support

```csharp
// Create networked timer
var (handle, netId) = Timer.CreateNetworked<CountdownTimer>(10f);

// Server: send sync data
NetworkTimerSync.OnSendSyncData = (data) => MyNetwork.Send(data);
NetworkTimerSync.BroadcastAllSyncData();

// Client: receive sync data
void OnReceive(NetworkTimerSyncData data) => NetworkTimerSync.ApplySyncData(data);
```

## Backends

### StandardBackend (Default)
- Thread-safe with queue-based async operations
- Works everywhere, no additional dependencies

### BurstBackend (High Performance)
- Uses Unity.Burst (`[BurstCompile]`) for parallel timer updates
- Uses Unity.Collections (`NativeList`, `NativeHashMap`) for cache-friendly storage
- Parallel job processing via `IJobParallelFor`

**Dependencies (included in package.json):**
```json
"com.unity.burst": "1.8.12",
"com.unity.collections": "2.2.1"
```

Enable in **PackageSettings** → **Use Burst Timers**

## Thread Safety

- ✅ Create timers from any thread
- ✅ Query state from any thread
- ✅ Control (Pause/Resume/Cancel) from any thread
- Operations from non-main threads are queued and processed on next Update

## API Reference

### Creation
| Method | Description |
|--------|-------------|
| `Timer.Create<T>(float)` | Create timer of type T |
| `Timer.Create<T>(TimerConfig)` | Create with config |
| `Timer.Delay(float, Action)` | One-shot delay |

### Control
| Method | Description |
|--------|-------------|
| `Timer.Pause(handle)` | Pause timer |
| `Timer.Resume(handle)` | Resume timer |
| `Timer.Cancel(handle)` | Cancel and remove |
| `Timer.Reset(handle)` | Reset to initial state |
| `Timer.SetTimeScale(handle, float)` | Set time multiplier |
| `Timer.Clear()` | Remove all timers |

### Query
| Method | Description |
|--------|-------------|
| `Timer.GetCurrentTime(handle)` | Current time value |
| `Timer.GetProgress(handle)` | Progress 0-1 |
| `Timer.GetEasedProgress(handle, EasingType)` | Eased progress |
| `Timer.IsFinished(handle)` | Check if finished |
| `Timer.IsRunning(handle)` | Check if running |
| `Timer.Count` | Active timer count |

### Events
| Method | Description |
|--------|-------------|
| `Timer.On<TCallback>(handle, Action)` | Register callback |
| `Timer.On<TCallback, TArg>(handle, Action<TArg>)` | Register with parameter |
| `Timer.Off<TCallback>(handle)` | Unregister callback |

### Easing
| Method | Description |
|--------|-------------|
| `Timer.Lerp(handle, from, to, easing)` | Interpolate float/Vector2/Vector3/Quaternion/Color |
| `Timer.LerpUnclamped(handle, from, to, easing)` | Allow values outside 0-1 |
