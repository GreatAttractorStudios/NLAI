using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// An IAction that moves a NavMeshAgent through a series of waypoints.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Patrol : MonoBehaviour, IAction
{
    [Tooltip("The unique name for this action, e.g., 'PatrolWaypoints'")]
    [SerializeField] private string _name;
    public string Name => _name;

    [Tooltip("The list of transforms representing the patrol waypoints.")]
    public List<Transform> waypoints = new List<Transform>();

    [Tooltip("How close the agent needs to be to a waypoint to consider it reached.")]
    public float destinationTolerance = 1.0f;

    private NavMeshAgent _agent;
    private int _currentWaypointIndex = -1;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public NodeStatus Execute()
    {
        if (waypoints.Count == 0) return NodeStatus.FAILURE;

        // If we don't have a destination or we've reached the current one, find the next one.
        if (_currentWaypointIndex == -1 || !_agent.pathPending && _agent.remainingDistance <= destinationTolerance)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Count;
            _agent.SetDestination(waypoints[_currentWaypointIndex].position);
        }

        return NodeStatus.RUNNING;
    }
} 