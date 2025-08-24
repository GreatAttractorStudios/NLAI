using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Action to make the AI wander to a random point within a specified radius.
/// This serves as a fallback behavior when the AI has no other tasks.
/// </summary>
public class Wander : MonoBehaviour, IAction
{
    [Tooltip("A unique name for this action. The LLM uses this to identify the action in the behavior tree.")]
    [SerializeField] private string _name;
    public string Name => _name;

    [Tooltip("The radius around the AI in which to find a random wander point.")]
    [SerializeField] private float wanderRadius = 20f;
    [Tooltip("How close the AI needs to be to the random point to consider it 'arrived'.")]
    [SerializeField] private float stoppingDistance = 1f;
    
    private NavMeshAgent agent;
    private Vector3 destination;
    private bool hasDestination;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Moves the AI towards a random destination.
    /// - Sets a new random destination if it doesn't have one.
    /// - Returns RUNNING while moving.
    /// - Returns SUCCESS upon arrival.
    /// - Returns FAILURE if it can't find a valid point on the NavMesh.
    /// </summary>
    public NodeStatus Execute()
    {
        if (!hasDestination)
        {
            if (RandomPoint(transform.position, wanderRadius, out destination))
            {
                agent.SetDestination(destination);
                hasDestination = true;
            }
            else
            {
                // Could not find a valid point on the NavMesh
                return NodeStatus.FAILURE;
            }
        }

        if (agent.remainingDistance <= stoppingDistance)
        {
            hasDestination = false; // Arrived, clear destination for next time
            return NodeStatus.SUCCESS;
        }

        return NodeStatus.RUNNING;
    }

    // Finds a random point on the NavMesh within a certain radius
    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++) // Try 30 times to find a valid point
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }
}