using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// An IAction that finds the farthest point from a specified enemy and flees there.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class FleeToFarthestPoint : MonoBehaviour, IAction
{
    [Tooltip("A unique name for this action. This is used by the LLM to identify the action.")]
    [SerializeField] private string _name;
    public string Name => _name;

    [Tooltip("The enemy to flee from.")]
    public Transform enemyTransform;

    [Tooltip("The list of possible points to flee to.")]
    public List<Transform> fleePoints = new List<Transform>();

    private NavMeshAgent _agent;
    private Transform _currentDestination;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public NodeStatus Execute()
    {
        if (enemyTransform == null || fleePoints.Count == 0)
        {
            return NodeStatus.FAILURE;
        }

        // Find the farthest flee point from the enemy
        Transform farthestPoint = null;
        float maxDistanceSqr = -1;
        foreach (Transform point in fleePoints)
        {
            if (point == null) continue;
            float distSqr = (point.position - enemyTransform.position).sqrMagnitude;
            if (distSqr > maxDistanceSqr)
            {
                maxDistanceSqr = distSqr;
                farthestPoint = point;
            }
        }

        // If we have a valid destination, and it's different from our current one, set a new path
        if (farthestPoint != null && farthestPoint != _currentDestination)
        {
            _agent.SetDestination(farthestPoint.position);
            _currentDestination = farthestPoint;
            Debug.Log($"Fleeing to new farthest point: {_currentDestination.name}", this);
        }

        // If we are still moving, return RUNNING
        if (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance)
        {
            return NodeStatus.RUNNING;
        }

        // We have arrived, so we have successfully "fled" for now.
        _currentDestination = null;
        return NodeStatus.SUCCESS;
    }
} 