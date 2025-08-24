using UnityEngine;

/// <summary>
/// Action to move the AI to its currently assigned target using the NavMeshAgent.
/// </summary>
public class MoveTo : MonoBehaviour, IAction
{
    [Tooltip("A unique name for this action. The LLM uses this to identify the action in the behavior tree.")]
    [SerializeField] private string _name;
    public string Name => _name;

    [Tooltip("How close the AI needs to be to the target to consider it 'arrived'.")]
    [SerializeField] private float stoppingDistance = 0.5f;

    private GathererAI ai;

    private void Awake()
    {
        ai = GetComponent<GathererAI>();
    }

    /// <summary>
    /// Moves the AI towards the CurrentTarget.
    /// - Returns RUNNING while the AI is moving.
    /// - Returns SUCCESS when the AI reaches the destination.
    /// - Returns FAILURE if there is no target or path.
    /// </summary>
    public NodeStatus Execute()
    {
        if (ai.CurrentTarget == null)
        {
            // Silently fail if the target is not set.
            // This can happen if the sequence is resumed incorrectly by the tree.
            return NodeStatus.FAILURE;
        }

        // Set the destination on the NavMeshAgent
        ai.Agent.SetDestination(ai.CurrentTarget.position);

        // Check if the agent has reached the destination
        // We check remainingDistance and that the path is not still being calculated (pathPending)
        if (!ai.Agent.pathPending && ai.Agent.remainingDistance <= stoppingDistance)
        {
            return NodeStatus.SUCCESS;
        }

        return NodeStatus.RUNNING;
    }
} 