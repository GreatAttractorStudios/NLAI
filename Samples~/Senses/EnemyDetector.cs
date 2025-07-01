using UnityEngine;

/// <summary>
/// A simple, reliable enemy detection component.
/// Tracks when an enemy is currently visible and when it was last seen.
/// </summary>
public class EnemyDetector : MonoBehaviour, ISense
{
    [Tooltip("The unique name for this sense")]
    [SerializeField] private string _name = "CanSeeEnemy";
    public string Name => _name;

    [Header("Detection Settings")]
    [Tooltip("The tag to look for (e.g., 'Player', 'Enemy')")]
    public string targetTag = "Player";
    
    [Tooltip("Maximum distance to detect targets")]
    public float detectionRange = 15f;
    
    [Tooltip("What layers block line of sight")]
    public LayerMask obstacleLayerMask = 1; // Default layer
    
    [Tooltip("Where the 'eyes' are located")]
    public Transform eyeTransform;
    
    [Header("Debug")]
    [Tooltip("Show debug rays in scene view")]
    public bool showDebugRays = true;
    
    [Header("Status (Read Only)")]
    [Tooltip("The last time an enemy was successfully detected")]
    public float lastSeenTime = -1f;
    
    [Tooltip("Is an enemy currently visible?")]
    public bool currentlyVisible = false;

    void Start()
    {
        if (eyeTransform == null)
            eyeTransform = transform;
    }

    public bool Evaluate()
    {
        // Find the target
        GameObject target = GameObject.FindWithTag(targetTag);
        if (target == null)
        {
            currentlyVisible = false;
            return false;
        }

        // Check distance
        float distance = Vector3.Distance(eyeTransform.position, target.transform.position);
        if (distance > detectionRange)
        {
            currentlyVisible = false;
            if (showDebugRays)
                Debug.DrawLine(eyeTransform.position, eyeTransform.position + (target.transform.position - eyeTransform.position).normalized * detectionRange, Color.yellow);
            return false;
        }

        // Check line of sight
        Vector3 directionToTarget = target.transform.position - eyeTransform.position;
        if (Physics.Raycast(eyeTransform.position, directionToTarget.normalized, out RaycastHit hit, distance, obstacleLayerMask))
        {
            // Something is blocking our view
            currentlyVisible = false;
            if (showDebugRays)
                Debug.DrawLine(eyeTransform.position, hit.point, Color.red);
            return false;
        }

        // Clear line of sight!
        currentlyVisible = true;
        lastSeenTime = Time.time;
        if (showDebugRays)
            Debug.DrawLine(eyeTransform.position, target.transform.position, Color.green);
        
        return true;
    }

    /// <summary>
    /// Reset the detection state (forget we ever saw anything)
    /// </summary>
    public void ForgetTarget()
    {
        lastSeenTime = -1f;
        currentlyVisible = false;
    }
} 