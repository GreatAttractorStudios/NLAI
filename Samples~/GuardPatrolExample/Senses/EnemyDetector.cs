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
    public LayerMask obstacleLayerMask = -1; // Everything by default
    
    [Tooltip("Where the 'eyes' are located")]
    public Transform eyeTransform;
    
    [Header("Debug")]
    [Tooltip("Show debug rays in scene view")]
    public bool showDebugRays = true;
    
    [Tooltip("Show debug info in console")]
    public bool showDebugLogs = true;
    
    [Tooltip("Show debug GUI on screen")]
    public bool showDebugGUI = true;
    
    [Header("Status (Read Only)")]
    [Tooltip("The last time an enemy was successfully detected")]
    public float lastSeenTime = -1f;
    
    [Tooltip("Is an enemy currently visible?")]
    public bool currentlyVisible = false;

    // Debug info
    private string lastDebugMessage = "";
    private GameObject currentTarget;

    void Start()
    {
        if (eyeTransform == null)
            eyeTransform = transform;
    }

    public bool Evaluate()
    {
        // Find the target
        GameObject target = GameObject.FindWithTag(targetTag);
        currentTarget = target;
        
        if (target == null)
        {
            currentlyVisible = false;
            lastDebugMessage = $"No target found with tag '{targetTag}'";
            if (showDebugLogs) Debug.Log($"[{name}] {lastDebugMessage}");
            return false;
        }

        // Check distance
        float distance = Vector3.Distance(eyeTransform.position, target.transform.position);
        if (distance > detectionRange)
        {
            currentlyVisible = false;
            lastDebugMessage = $"Target '{target.name}' too far: {distance:F1}m (max: {detectionRange}m)";
            if (showDebugLogs) Debug.Log($"[{name}] {lastDebugMessage}");
            if (showDebugRays)
                Debug.DrawLine(eyeTransform.position, eyeTransform.position + (target.transform.position - eyeTransform.position).normalized * detectionRange, Color.yellow);
            return false;
        }

        // Check line of sight
        Vector3 directionToTarget = target.transform.position - eyeTransform.position;
        
        // Debug: Show detailed raycast info
        if (showDebugLogs)
        {
            Debug.Log($"[{name}] RAYCAST DEBUG:");
            Debug.Log($"  Start: {eyeTransform.position}");
            Debug.Log($"  End: {target.transform.position}");
            Debug.Log($"  Direction: {directionToTarget.normalized}");
            Debug.Log($"  Distance: {distance:F2}m");
            Debug.Log($"  Layer Mask: {obstacleLayerMask.value} (binary: {System.Convert.ToString(obstacleLayerMask.value, 2)})");
        }
        
        // Test if ANYTHING is hit by this raycast (regardless of layer)
        bool anyHit = Physics.Raycast(eyeTransform.position, directionToTarget.normalized, out RaycastHit anyHitInfo, distance);
        if (showDebugLogs && anyHit)
        {
            Debug.Log($"[{name}] RAYCAST HIT SOMETHING: '{anyHitInfo.collider.name}' on layer {anyHitInfo.collider.gameObject.layer} ({LayerMask.LayerToName(anyHitInfo.collider.gameObject.layer)})");
        }
        
        // Now do the actual filtered raycast
        if (Physics.Raycast(eyeTransform.position, directionToTarget.normalized, out RaycastHit hit, distance, obstacleLayerMask))
        {
            // Something is blocking our view
            currentlyVisible = false;
            lastDebugMessage = $"Line of sight blocked by '{hit.collider.name}' on layer '{LayerMask.LayerToName(hit.collider.gameObject.layer)}' at {hit.distance:F1}m";
            if (showDebugLogs) Debug.Log($"[{name}] {lastDebugMessage}");
            if (showDebugRays)
                Debug.DrawLine(eyeTransform.position, hit.point, Color.red);
            return false;
        }

        // Clear line of sight!
        currentlyVisible = true;
        lastSeenTime = Time.time;
        lastDebugMessage = $"Target '{target.name}' visible at {distance:F1}m";
        if (showDebugLogs) 
        {
            Debug.Log($"[{name}] {lastDebugMessage}");
            if (anyHit)
            {
                Debug.Log($"[{name}] NOTE: Raycast hit '{anyHitInfo.collider.name}' but it's not on obstacle layers");
            }
            else
            {
                Debug.Log($"[{name}] NOTE: Raycast hit nothing at all - completely clear path");
            }
        }
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
        lastDebugMessage = "Target forgotten";
        if (showDebugLogs) Debug.Log($"[{name}] {lastDebugMessage}");
    }

    void OnGUI()
    {
        if (!showDebugGUI) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 250));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label($"<b>Enemy Detector Debug ({name})</b>");
        GUILayout.Label($"Target Tag: {targetTag}");
        GUILayout.Label($"Detection Range: {detectionRange}m");
        
        // Show which layers are included in the mask
        string layerNames = "";
        for (int i = 0; i < 32; i++)
        {
            if ((obstacleLayerMask.value & (1 << i)) != 0)
            {
                if (layerNames != "") layerNames += ", ";
                layerNames += LayerMask.LayerToName(i);
            }
        }
        GUILayout.Label($"Obstacle Layers: {layerNames}");
        
        GUILayout.Space(5);
        
        GUILayout.Label($"<b>Current Status:</b>");
        GUILayout.Label($"Currently Visible: {(currentlyVisible ? "<color=green>YES</color>" : "<color=red>NO</color>")}");
        GUILayout.Label($"Last Seen: {(lastSeenTime < 0 ? "Never" : $"{Time.time - lastSeenTime:F1}s ago")}");
        GUILayout.Label($"Current Target: {(currentTarget ? currentTarget.name : "None")}");
        
        GUILayout.Space(5);
        
        GUILayout.Label($"<b>Last Check:</b>");
        GUILayout.Label(lastDebugMessage);
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
} 