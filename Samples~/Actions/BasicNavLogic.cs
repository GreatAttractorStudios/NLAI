using UnityEngine;
using UnityEngine.AI;
using System.Linq;

/// <summary>
/// An IAction that moves a NavMeshAgent to a specified target.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class BasicNavLogic : MonoBehaviour, IAction
{
    [Tooltip("The unique name for this action, e.g., 'GoToPointA' or 'ChaseEnemy'")]
    [SerializeField] private string _name;
    public string Name => _name;

    [Tooltip("The target transform to move towards. If null, will use targetPosition.")]
    public Transform target;

    [Tooltip("If Target is null, this position will be used as the destination.")]
    [SerializeField] private Vector3 targetPosition;

    [Tooltip("If true, this action returns SUCCESS immediately after setting the destination. If false, it returns RUNNING until the destination is reached.")]
    public bool returnSuccessImmediately = false;
    
    [Tooltip("How far from the target's actual position we are willing to search for a valid point on the NavMesh.")]
    public float navMeshSearchRadius = 2.0f;

    [Tooltip("How close the agent needs to be to the destination to be considered successful.")]
    [SerializeField] private float destinationTolerance = 1.0f;

    [SerializeField] private float tolerance = 1.0f;

    private NavMeshAgent _agent;
    private Vector3? _lastSetDestination;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public NodeStatus Execute()
    {
        if (_agent == null)
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent == null)
            {
                Debug.LogError("BasicNavLogic requires a NavMeshAgent component.", this);
                return NodeStatus.FAILURE;
            }
        }

        Vector3 potentialDestination = target != null ? target.position : targetPosition;

        // Only set the destination if it has changed since the last time.
        if (_lastSetDestination == null || Vector3.Distance(_lastSetDestination.Value, potentialDestination) > 0.1f)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(potentialDestination, out hit, navMeshSearchRadius, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
                _lastSetDestination = hit.position;
            }
            else
            {
                return NodeStatus.FAILURE;
            }
        }
        
        if (_agent.pathPending)
        {
            return NodeStatus.RUNNING;
        }

        if (!_agent.hasPath || _agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            return NodeStatus.FAILURE;
        }

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            _lastSetDestination = null; // Clear the destination to allow re-triggering.
            return NodeStatus.SUCCESS;
        }

        return NodeStatus.RUNNING;
    }
} 