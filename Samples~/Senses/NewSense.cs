using UnityEngine;

/// <summary>
/// BOILERPLATE for creating a new Sense.
/// Senses are the "eyes and ears" of your AI - what it can PERCEIVE.
/// Examples: CanSeeEnemy, IsHealthLow, HasKey, IsPlayerNear, IsAtDestination.
/// 
/// CRITICAL BEHAVIOR TREE CONCEPTS:
/// 
/// 1. SENSES ARE CALLED EVERY FRAME: Your Evaluate() is called repeatedly!
///    - In PrioritySelector: ALL senses are checked every frame
///    - In StatefulSequence: Sense is checked once, then action runs until completion
/// 
/// 2. WHEN TO RETURN TRUE/FALSE:
///    - true: "The condition IS met right now" (becomes NodeStatus.SUCCESS)
///    - false: "The condition is NOT met right now" (becomes NodeStatus.FAILURE)
/// 
/// 3. BEHAVIOR TREE FLOW CONTROL:
///    - PrioritySelector: First sense that returns true wins, runs its action
///    - StatefulSequence: If sense returns false, whole sequence fails and resets
/// 
/// 4. SMART SENSES vs SIMPLE SENSES:
///    - Simple: Just check current state (IsHealthLow, CanSeeEnemy)
///    - Smart: Track state over time (HasLostTarget, TimerElapsed)
/// </summary>
public class NewSense : MonoBehaviour, ISense
{
    [Tooltip("A unique name for this sense. The LLM uses this to identify the sense in the behavior tree.")]
    [SerializeField] private string _name;
    public string Name => _name;

    /// <summary>
    /// This method is called by the Behavior Tree to check if a condition is currently met.
    /// 
    /// EXAMPLES OF WHAT TO CHECK:
    /// 
    /// SIMPLE CONDITIONS (check right now):
    /// - IsHealthLow: return currentHealth < threshold
    /// - CanSeeEnemy: return Physics.Raycast shows clear line of sight
    /// - IsPlayerNear: return Vector3.Distance(player.position, transform.position) < range
    /// - HasKey: return inventory.HasItem("Key")
    /// - IsAtDestination: return Vector3.Distance(transform.position, destination) < tolerance
    /// 
    /// STATEFUL CONDITIONS (track over time):
    /// - HasLostTarget: return timeSinceLastSeen > memoryDuration
    /// - TimerElapsed: return Time.time > startTime + duration
    /// - IsStuck: return agent.velocity.magnitude < threshold for too long
    /// 
    /// COMMON PATTERNS:
    /// 
    /// PATTERN 1: Simple distance check
    /// // GameObject target = GameObject.FindWithTag("Player");
    /// // if (target == null) return false;
    /// // return Vector3.Distance(transform.position, target.position) < detectionRange;
    /// 
    /// PATTERN 2: Component state check
    /// // Health health = GetComponent<Health>();
    /// // if (health == null) return false;
    /// // return health.currentHealth < health.maxHealth * 0.3f;
    /// 
    /// PATTERN 3: Raycast/line of sight
    /// // if (Physics.Raycast(transform.position, target.position - transform.position, out hit, maxRange))
    /// //     return hit.collider.CompareTag("Player");
    /// // return false;
    /// 
    /// PATTERN 4: Timer/cooldown
    /// // if (Time.time >= lastCheckTime + checkInterval)
    /// // {
    /// //     lastCheckTime = Time.time;
    /// //     return SomeExpensiveCheck();
    /// // }
    /// // return lastResult;
    /// </summary>
    /// <returns>
    /// true: The condition IS currently met (Behavior tree treats as SUCCESS)
    /// false: The condition is NOT currently met (Behavior tree treats as FAILURE)
    /// </returns>
    public bool Evaluate()
    {
        // TODO: Implement your condition-checking logic here.
        Debug.Log($"Evaluating {Name}");

        // EXAMPLE IMPLEMENTATIONS:

        // EXAMPLE 1: Simple boolean check
        // return someCondition;

        // EXAMPLE 2: Distance-based check
        // GameObject target = GameObject.FindWithTag("Player");
        // return target != null && Vector3.Distance(transform.position, target.position) < 5f;

        // EXAMPLE 3: Component state check
        // Health health = GetComponent<Health>();
        // return health != null && health.currentHealth < health.maxHealth * 0.5f;

        // EXAMPLE 4: Complex condition with multiple factors
        // return IsTargetVisible() && IsInRange() && HasAmmo();

        return true;
    }
} 