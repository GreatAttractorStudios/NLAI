using UnityEngine;

/// <summary>
/// An IAction that clears the target from a specified IsTargetInLineOfSight component.
/// </summary>
public class ClearTarget : MonoBehaviour, IAction
{
    [Tooltip("A unique name for this action. This is used by the LLM to identify the action.")]
    [SerializeField] private string _name;
    public string Name => _name;
    
    [Tooltip("The BasicNavLogic component whose target should be cleared.")]
    public BasicNavLogic navLogic;

    public NodeStatus Execute()
    {
        if (navLogic != null)
        {
            navLogic.target = null;
        }

        return NodeStatus.SUCCESS;
    }
} 