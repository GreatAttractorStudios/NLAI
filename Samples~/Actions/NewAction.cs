using UnityEngine;

/// <summary>
/// A boilerplate for creating a new Action.
/// Actions are the "verbs" of your AI, defining what it can DO.
/// Examples: MoveToPoint, PlayAnimation, AttackTarget.
/// </summary>
public class NewAction : MonoBehaviour, IAction
{
    [Tooltip("A unique name for this action. The LLM uses this to identify the action in the behavior tree.")]
    [SerializeField] private string _name;
    public string Name => _name;

    /// <summary>
    /// This method is called by the Behavior Tree. It should contain the core logic for the action.
    /// </summary>
    /// <returns>
    /// NodeStatus.SUCCESS: The action was completed successfully.
    /// NodeStatus.FAILURE: The action failed to complete.
    /// NodeStatus.RUNNING: The action is still in progress (e.g., moving to a point).
    /// </returns>
    public NodeStatus Execute()
    {
        // TODO: Implement your action logic here.
        Debug.Log($"Executing {Name}");

        // For a simple, one-off action, return SUCCESS.
        // For an action that takes time, you would check its status and return RUNNING until it's done.
        return NodeStatus.SUCCESS;
    }
} 