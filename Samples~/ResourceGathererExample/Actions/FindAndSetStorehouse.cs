using UnityEngine;

/// <summary>
/// Action to set the AI's storehouse as its current target.
/// </summary>
public class FindAndSetStorehouse : MonoBehaviour, IAction
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
    /// Sets the storehouse from the GathererAI as the CurrentTarget.
    /// Returns SUCCESS if the storehouse is set, FAILURE otherwise.
    /// </summary>
    public NodeStatus Execute()
    {
        if (ai == null || ai.Storehouse == null)
        {
            Debug.LogError("FindAndSetStorehouse action failed: Storehouse is not set in GathererAI or AI component is missing.", this);
            return NodeStatus.FAILURE;
        }

        ai.CurrentTarget = ai.Storehouse;
        return NodeStatus.SUCCESS;
    }
}









