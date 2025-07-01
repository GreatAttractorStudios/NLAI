# Behavior Tree Execution Guide

This guide explains how behavior trees work in the NLAI system, when to use different patterns, and how to make your AI behaviors smart and reliable.

## Core Concepts

### 1. The Execution Loop

**Every frame**, Unity calls `Update()` on `NaturalLanguageBehavior`, which executes the entire behavior tree from the root:

```csharp
void Update()
{
    if (behaviorTree != null && behaviorTree.rootNode != null)
    {
        behaviorTree.rootNode.Execute(gameObject);  // Called EVERY FRAME
    }
}
```

**This means your Actions and Senses are called repeatedly - the behavior tree IS your game loop!**

### 2. Node Types and Their Execution Patterns

#### PrioritySelector (Most Common)
- **Checks ALL children from the beginning every frame**
- **Stops at the first child that doesn't return FAILURE**
- **Higher priority behaviors can interrupt lower ones immediately**

```csharp
// PrioritySelector execution:
foreach (var child in children)  // EVERY FRAME, start from the beginning
{
    var childStatus = child.Execute(agent);
    if (childStatus != NodeStatus.FAILURE)  // First non-failure wins
    {
        return status = childStatus;
    }
}
```

**Frame-by-Frame Example:**
```
Frame 1: Health=100%, No enemy → Check IsHealthLow (FAIL) → Check CanSeeEnemy (FAIL) → Run Patrol
Frame 2: Health=100%, Enemy appears → Check IsHealthLow (FAIL) → Check CanSeeEnemy (SUCCESS) → Run Chase
Frame 3: Health=10%, Enemy visible → Check IsHealthLow (SUCCESS) → Run Flee (Chase interrupted!)
```

#### StatefulSequence (For Multi-Step Behaviors)
- **Remembers the last running child**
- **Only re-checks conditions when the sequence resets**
- **Good for "check condition once, then do action until complete"**

```csharp
// StatefulSequence execution:
for (int i = _lastRunningChildIndex; i < children.Count; i++)  // Resume from last position
{
    var childStatus = children[i].Execute(agent);
    
    if (childStatus == NodeStatus.FAILURE)
    {
        _lastRunningChildIndex = 0;  // Reset on failure
        return NodeStatus.FAILURE;
    }
    
    if (childStatus == NodeStatus.RUNNING)
    {
        _lastRunningChildIndex = i;  // Remember this position!
        return NodeStatus.RUNNING;
    }
    // If SUCCESS, continue to next child
}
```

**Example: StatefulSequence[CanSeeEnemy → Chase]**
```
Frame 1: Check CanSeeEnemy (SUCCESS) → Move to Chase, remember position
Frame 2: Skip CanSeeEnemy, run Chase (RUNNING) 
Frame 3: Skip CanSeeEnemy, run Chase (RUNNING)
Frame N: Skip CanSeeEnemy, run Chase (FAILURE when target lost) → Reset sequence
```

## When to Use Each Pattern

### Use PrioritySelector When:
- ✅ You want **reactive behaviors** that can interrupt each other
- ✅ You want **all conditions checked every frame**
- ✅ You want **higher priorities to override lower ones immediately**
- ✅ Examples: "Flee if health low, otherwise chase if enemy seen, otherwise patrol"

### Use StatefulSequence When:
- ✅ You want **"check once, then commit"** behavior
- ✅ You want **multi-step actions** that shouldn't be interrupted mid-step
- ✅ You want **efficiency** (don't re-check expensive conditions)
- ✅ Examples: "If can see enemy, chase until lost", "If at door, open it then walk through"

## Action Patterns

### Pattern 1: One-Shot Actions
Actions that complete immediately:

```csharp
public NodeStatus Execute()
{
    PlaySound();
    SpawnParticles();
    return NodeStatus.SUCCESS;  // Done immediately
}
```

### Pattern 2: Time-Based Actions
Actions that take time to complete:

```csharp
public NodeStatus Execute()
{
    if (Vector3.Distance(transform.position, destination) < 0.5f)
        return NodeStatus.SUCCESS;  // Arrived!
    
    agent.SetDestination(destination);
    return NodeStatus.RUNNING;  // Still moving
}
```

### Pattern 3: Continuous Actions
Actions that run forever until interrupted:

```csharp
public NodeStatus Execute()
{
    DoPatrolMovement();
    return NodeStatus.RUNNING;  // Never ends naturally
}
```

### Pattern 4: Smart Actions (Recommended for Chase)
Actions that check their own conditions:

```csharp
public NodeStatus Execute()
{
    // Check our own condition every frame
    if (!enemyDetector.Evaluate())
        return NodeStatus.FAILURE;  // Lost target, let behavior tree try next priority
    
    agent.SetDestination(target.position);
    return NodeStatus.RUNNING;  // Continue chasing
}
```

### Pattern 5: Conditional Failure
Actions that might fail based on circumstances:

```csharp
public NodeStatus Execute()
{
    if (!agent.hasPath)
        return NodeStatus.FAILURE;  // Can't reach destination
    
    if (agent.remainingDistance < 0.5f)
        return NodeStatus.SUCCESS;  // Arrived
    
    return NodeStatus.RUNNING;  // Still moving
}
```

## Sense Patterns

### Simple Senses (Check Current State)
```csharp
public bool Evaluate()
{
    return health.currentHealth < health.maxHealth * 0.3f;  // Check right now
}
```

### Stateful Senses (Track Over Time)
```csharp
private float lastSeenTime = -1f;

public bool Evaluate()
{
    if (CanSeeTargetRightNow())
    {
        lastSeenTime = Time.time;
        return false;  // Not lost yet
    }
    
    // Lost if enough time has passed
    return Time.time - lastSeenTime > memoryDuration;
}
```

## Common Mistakes and Solutions

### ❌ Problem: Chase Never Stops
**Wrong:** Using StatefulSequence with separate "HasLostTarget" branch
```
PrioritySelector:
├── StatefulSequence[CanSeeEnemy → Chase]  ← Once started, never re-checks CanSeeEnemy
└── StatefulSequence[HasLostTarget → Patrol]  ← Never reached because Chase never ends
```

**Right:** Use Smart Chase action that checks its own conditions
```
PrioritySelector:
├── StatefulSequence[CanSeeEnemy → SmartChase]  ← SmartChase returns FAILURE when target lost
└── Patrol  ← Gets reached when chase fails
```

### ❌ Problem: Actions That Don't Loop Properly
**Wrong:** Using loops inside Execute()
```csharp
public NodeStatus Execute()
{
    while (!reachedDestination)  // ❌ Blocks Unity forever!
    {
        MoveTowardsDestination();
    }
    return NodeStatus.SUCCESS;
}
```

**Right:** Let the behavior tree be your loop
```csharp
public NodeStatus Execute()
{
    if (reachedDestination)
        return NodeStatus.SUCCESS;
    
    MoveTowardsDestination();
    return NodeStatus.RUNNING;  // ✅ Called again next frame
}
```

### ❌ Problem: Expensive Operations Every Frame
**Wrong:** Expensive calculations in Evaluate()
```csharp
public bool Evaluate()
{
    return ExpensivePathfinding();  // ❌ Called every frame!
}
```

**Right:** Cache results with timers
```csharp
private float lastCheckTime;
private bool lastResult;

public bool Evaluate()
{
    if (Time.time - lastCheckTime > 0.5f)  // Check every 0.5 seconds
    {
        lastResult = ExpensivePathfinding();
        lastCheckTime = Time.time;
    }
    return lastResult;
}
```

## Debugging Tips

### Add Debug Logging
```csharp
public NodeStatus Execute()
{
    Debug.Log($"{Name}: Current state = {currentState}, Target = {target?.name}");
    // ... your logic
}
```

### Use OnGUI for Real-Time Status
```csharp
void OnGUI()
{
    GUILayout.Label($"Chase Status: {agent.hasPath}, Distance: {agent.remainingDistance:F1}");
    GUILayout.Label($"Target Visible: {enemyDetector.currentlyVisible}");
}
```

### Color-Code Debug Rays
```csharp
if (showDebugRays)
{
    Color rayColor = currentlyVisible ? Color.green : Color.red;
    Debug.DrawRay(eyeTransform.position, direction * maxRange, rayColor);
}
```

## Best Practices

1. **Make Actions Smart**: Have them check their own continuation conditions
2. **Use PrioritySelector for Reactive AI**: Let higher priorities interrupt lower ones
3. **Use StatefulSequence for Committed Actions**: When you want "check once, then commit"
4. **Return FAILURE Strategically**: Use it to trigger behavior tree transitions
5. **Cache Expensive Operations**: Don't do expensive work every frame
6. **Add Comprehensive Debugging**: Log states, draw rays, show GUI status
7. **Test Edge Cases**: What happens when targets disappear, paths are blocked, components are missing?

Remember: **The behavior tree execution IS your game loop**. Design your Actions and Senses with this in mind! 