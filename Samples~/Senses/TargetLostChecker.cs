using UnityEngine;

/// <summary>
/// Checks if enough time has passed since we last saw a target.
/// Works in conjunction with EnemyDetector.
/// </summary>
public class TargetLostChecker : MonoBehaviour, ISense
{
    [Tooltip("The unique name for this sense")]
    [SerializeField] private string _name = "HasLostTarget";
    public string Name => _name;

    [Header("Settings")]
    [Tooltip("Reference to the EnemyDetector component")]
    public EnemyDetector enemyDetector;
    
    [Tooltip("How long to remember the target after losing sight (seconds)")]
    public float memoryTime = 3f;

    void Start()
    {
        if (enemyDetector == null)
            enemyDetector = GetComponent<EnemyDetector>();
    }

    public bool Evaluate()
    {
        if (enemyDetector == null)
        {
            Debug.LogError("TargetLostChecker needs a reference to an EnemyDetector component!", this);
            return false;
        }

        // If we never saw a target, we can't have "lost" it
        if (enemyDetector.lastSeenTime < 0)
        {
            return false;
        }

        // If we can currently see the target, it's not lost
        if (enemyDetector.currentlyVisible)
        {
            return false;
        }

        // Check if enough time has passed since we last saw it
        float timeSinceLastSeen = Time.time - enemyDetector.lastSeenTime;
        if (timeSinceLastSeen >= memoryTime)
        {
            return true; // Yes, the target is lost
        }

        return false; // Still within memory time
    }
} 