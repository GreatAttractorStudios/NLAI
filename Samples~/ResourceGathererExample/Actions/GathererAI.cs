using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages the state and central components for a resource-gathering NPC.
/// This script acts as a central hub that Senses and Actions can query
/// to make decisions, reducing the need for repeated GetComponent calls.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class GathererAI : MonoBehaviour
{
    [Header("State")]
    [Tooltip("How many resources is the AI currently carrying?")]
    public int ResourceCount = 0;

    [Header("Targets")]
    [Tooltip("The current object the AI is moving towards (either a resource or the storehouse).")]
    public Transform CurrentTarget;
    
    [Tooltip("The designated drop-off point for resources.")]
    public Transform Storehouse;

    [Header("Dependencies")]
    [HideInInspector]
    public NavMeshAgent Agent;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }
}