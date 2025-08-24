using UnityEngine;

/// <summary>
/// Action to "gather" a resource. This destroys the target GameObject
/// and updates the AI's state to indicate it is carrying a resource.
/// </summary>
public class GatherResource : MonoBehaviour, IAction
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
    /// Destroys the CurrentTarget and sets HasResource to true.
    /// Returns SUCCESS if it successfully gathers the resource.
    /// Returns FAILURE if there is no target or the target is not a resource.
    /// </summary>
    public NodeStatus Execute()
    {
        if (ai == null || ai.CurrentTarget == null || !ai.CurrentTarget.CompareTag("Resource"))
        {
            Debug.LogError("GatherResource action failed: CurrentTarget is not a valid resource or AI component is missing.", this);
            return NodeStatus.FAILURE;
        }

        // "Gather" the resource by destroying it
        Destroy(ai.CurrentTarget.gameObject);

        // Update AI state
        ai.ResourceCount++;
        ai.CurrentTarget = null; // Clear the target after gathering

        return NodeStatus.SUCCESS;
    }
}
