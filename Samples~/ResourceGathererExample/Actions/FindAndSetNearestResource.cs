using UnityEngine;

/// <summary>
/// Action to find the nearest GameObject with the "Resource" tag and set it as the AI's target.
/// </summary>
public class FindAndSetNearestResource : MonoBehaviour, IAction
{
    [Tooltip("A unique name for this action. The LLM uses this to identify the action in the behavior tree.")]
    [SerializeField] private string _name;
    public string Name => _name;

    private GathererAI ai;

    private void Awake()
    {
        ai = GetComponent<GathererAI>();
    }

    /// <summary>
    /// Finds the closest resource and sets it as the current target.
    /// Returns SUCCESS if a resource is found, FAILURE otherwise.
    /// </summary>
    public NodeStatus Execute()
    {
        GameObject[] resources = GameObject.FindGameObjectsWithTag("Resource");
        GameObject closestResource = null;
        float minDistance = float.MaxValue;

        foreach (var resource in resources)
        {
            float distance = Vector3.Distance(transform.position, resource.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestResource = resource;
            }
        }

        if (closestResource != null)
        {
            ai.CurrentTarget = closestResource.transform;
            return NodeStatus.SUCCESS;
        }

        return NodeStatus.FAILURE;
    }
}