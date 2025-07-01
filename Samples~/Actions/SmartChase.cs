using UnityEngine;
using UnityEngine.AI;
using System.Linq;

/// <summary>
/// An intelligent Chase action that continuously checks line of sight while chasing.
/// Returns FAILURE when the target is lost, allowing the behavior tree to move to the next priority.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class SmartChase : MonoBehaviour, IAction
{
    [Tooltip("The unique name for this action")]
    [SerializeField] private string _name = "Chase";
    public string Name => _name;

    [Header("Chase Settings")]
    [Tooltip("Reference to the EnemyDetector to get current target and check visibility")]
    public EnemyDetector enemyDetector;
    
    [Tooltip("How close to get to the target")]
    public float chaseDistance = 2.0f;
    
    [Tooltip("How often to update the destination (in seconds)")]
    public float updateFrequency = 0.5f;

    private NavMeshAgent _agent;
    private float _lastUpdateTime;
    private Transform _currentTarget;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        
        if (enemyDetector == null)
            enemyDetector = GetComponent<EnemyDetector>();
    }

    public NodeStatus Execute()
    {
        if (_agent == null || enemyDetector == null)
        {
            Debug.LogError("SmartChase requires NavMeshAgent and EnemyDetector components!", this);
            return NodeStatus.FAILURE;
        }

        // Check if we can still see the target by calling the detector's Evaluate method
        if (!enemyDetector.Evaluate())
        {
            Debug.Log("SmartChase: Lost sight of target - stopping chase");
            _agent.ResetPath(); // Stop moving
            return NodeStatus.FAILURE; // This will cause the behavior tree to try the next priority
        }

        // Find the target using the same method as the detector
        GameObject targetGO = GameObject.FindWithTag(enemyDetector.targetTag);
        if (targetGO == null)
        {
            Debug.Log("SmartChase: No target to chase");
            return NodeStatus.FAILURE;
        }
        
        Transform target = targetGO.transform;

        // Update destination periodically or when target changes
        if (_currentTarget != target || Time.time - _lastUpdateTime > updateFrequency)
        {
            _currentTarget = target;
            _lastUpdateTime = Time.time;
            
            // Set new destination
            Vector3 targetPosition = target.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 5.0f, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
                Debug.Log($"SmartChase: Chasing {target.name}");
            }
            else
            {
                Debug.LogWarning("SmartChase: Could not find valid NavMesh position near target");
                return NodeStatus.FAILURE;
            }
        }

        // Check if we're close enough to the target
        if (Vector3.Distance(transform.position, target.position) <= chaseDistance)
        {
            Debug.Log("SmartChase: Reached target");
            return NodeStatus.SUCCESS;
        }

        // Still chasing
        return NodeStatus.RUNNING;
    }
} 