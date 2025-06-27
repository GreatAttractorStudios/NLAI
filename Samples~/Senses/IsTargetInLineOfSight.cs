using UnityEngine;

/// <summary>
/// An ISense that checks if a specific target is within line of sight,
/// meaning there are no obstacles between this character and the target.
/// </summary>
public class IsTargetInLineOfSight : MonoBehaviour, ISense
{
    [Tooltip("A unique name for this sense. This is used by the LLM to identify the sense.")]
    [SerializeField] private string _name;
    public string Name => _name;

    [Tooltip("The agent's eye transform.")]
    public Transform agentEyes;

    [Tooltip("The tag of the GameObjects to consider as targets.")]
    public string targetTag = "Player";

    [Tooltip("The layer mask to consider as obstacles. Typically 'Default', 'Walls', etc.")]
    public LayerMask obstacleLayerMask;

    [Tooltip("The vertical offset from this object's pivot to cast the ray from (e.g., eye level).")]
    public float eyeLevelOffset = 1.5f;

    [Tooltip("The maximum distance the character can see.")]
    public float maxSightDistance = 20f;

    [Tooltip("Enable to draw debug rays in the Scene view.")]
    public bool enableDebugRays = true;

    [Tooltip("The last time the target was successfully seen. Updated automatically.")]
    [ReadOnly]
    public float lastTimeSeen = -1f;

    private Transform target;

    public bool Evaluate()
    {
        if (target == null)
        {
            GameObject targetObject = GameObject.FindWithTag(targetTag);
            if (targetObject != null)
            {
                target = targetObject.transform;
            }
        }
        
        if (target == null)
        {
            return false;
        }

        Vector3 startPoint = transform.position + new Vector3(0, eyeLevelOffset, 0);
        Vector3 direction = (target.position - startPoint).normalized;
        float distanceToTarget = Vector3.Distance(startPoint, target.position);

        // Check if the target is beyond the maximum sight distance
        if (distanceToTarget > maxSightDistance)
        {
            if (enableDebugRays)
            {
                Debug.DrawRay(startPoint, direction * maxSightDistance, Color.yellow);
            }
            //Debug.Log($"Target {target.name} is beyond maximum sight distance of {maxSightDistance} units.");
            return false;
        }

        // Perform the raycast
        if (Physics.Raycast(startPoint, direction, out RaycastHit hit, distanceToTarget, obstacleLayerMask))
        {
            // We hit an obstacle before we hit the target.
            if (enableDebugRays)
            {
                Debug.DrawLine(startPoint, hit.point, Color.red);
                //Debug.Log($"Line of sight to {target.name} blocked by {hit.collider.name}", hit.collider.gameObject);
            }
            return false;
        }

        // No obstacles detected, we have a clear line of sight.
        if (enableDebugRays)
        {
            Debug.DrawRay(startPoint, direction * distanceToTarget, Color.green);
        }
        lastTimeSeen = Time.time;
        return true;
    }
} 