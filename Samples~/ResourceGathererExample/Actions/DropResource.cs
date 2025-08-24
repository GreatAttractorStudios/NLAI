using UnityEngine;

/// <summary>
/// Action to "drop" a resource at the storehouse. This simply
/// updates the AI's state to indicate it is no longer carrying a resource.
/// </summary>
public class DropResource : MonoBehaviour, IAction
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
    /// Sets the HasResource flag to false.
    /// Returns SUCCESS.
    /// </summary>
    public NodeStatus Execute()
    {
        // Optional: Add logic here to increment the storehouse's resource count
        Debug.Log($"Dropped off {ai.ResourceCount} resources at storehouse!");
        ai.ResourceCount = 0;
        return NodeStatus.SUCCESS;
    }
}