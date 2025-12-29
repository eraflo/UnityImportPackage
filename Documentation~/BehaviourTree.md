# Behaviour Tree System

A flexible, ScriptableObject-based Behaviour Tree system for AI agents with visual graph editing.

## Quick Start

1. **Create Tree:** Right-click in Project → Create → Behaviour Tree → Behaviour Tree
2. **Open Editor:** Click "Open Behaviour Tree Editor" button in the inspector
3. **Add Nodes:** Right-click in graph or press Space for search window
4. **Connect Nodes:** Drag from output (bottom) to input (top)
5. **Run:** Add `BehaviourTreeRunner` to your agent and assign the tree

---

## Visual Editor

Open via: `Tools > Unity Import Package > Behaviour Tree Editor`

| Feature | Description |
|---------|-------------|
| **Graph View** | Top-to-bottom node layout with drag & zoom |
| **Node Search** | Press Space to search and create nodes |
| **Inspector Panel** | Edit selected node properties |
| **Blackboard Panel** | View/edit shared data |
| **Context Menu** | Right-click to create or set root |

---

## Node Types

### Composites (Blue)
| Node | Description |
|------|-------------|
| `Selector` | OR - succeeds on first child success |
| `Sequence` | AND - fails on first child failure |
| `Parallel` | Runs all children simultaneously |
| `RandomSelector` | Shuffles order before selection |

### Decorators (Purple)
| Node | Description |
|------|-------------|
| `Inverter` | Inverts child result |
| `Repeater` | Repeats N times (0 = infinite) |
| `Succeeder` | Always returns Success |
| `UntilFail` | Repeats until child fails |
| `Cooldown` | Rate-limits execution (uses Timer) |

### Actions (Green)
| Node | Description |
|------|-------------|
| `Wait` | Waits N seconds (uses Timer) |
| `Log` | Debug.Log message |
| `RaiseEvent` | Raises EventChannel |
| `SetBlackboardValue` | Sets blackboard data |

### Conditions (Yellow)
| Node | Description |
|------|-------------|
| `BlackboardCondition` | Checks blackboard value |

---

## Blackboard

```csharp
// Set values
Blackboard.Set("target", transform.position);
Blackboard.Set("health", 100);

// Get values
Vector3 pos = Blackboard.Get<Vector3>("target");

// Check existence
if (Blackboard.Contains("key")) { ... }
```

---

## Custom Nodes

### Action Node
```csharp
[CreateAssetMenu(menuName = "AI/My Action")]
public class MyAction : ActionNode
{
    protected override void OnStart() { }
    
    protected override NodeState OnUpdate()
    {
        // Do work, return Running/Success/Failure
        return NodeState.Success;
    }
    
    protected override void OnStop() { }
}
```

### Condition Node
```csharp
[CreateAssetMenu(menuName = "AI/My Condition")]
public class MyCondition : ConditionNode
{
    protected override bool CheckCondition()
    {
        return Owner.GetComponent<Health>().Value > 0;
    }
}
```

---

## Integration

| System | Usage |
|--------|-------|
| **Timer** | `Wait`, `Cooldown` use Timer.Delay |
| **EventBus** | `RaiseEvent` node raises EventChannel |
| **Pooling** | Custom nodes can use Pool.Spawn |
| **Networking** | `NetworkBehaviourTreeSync` for multiplayer |

---

## Multiplayer

Add `NetworkBehaviourTreeSync` for server-authoritative AI:
- Server evaluates tree, clients receive state
- Blackboard values can be synchronized
- Works with Netcode for GameObjects
