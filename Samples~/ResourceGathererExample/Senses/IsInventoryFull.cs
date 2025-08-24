using UnityEngine;

/// <summary>
/// Sense to check if the AI's resource count has reached a specific threshold.
/// This allows for behavior that triggers when an inventory is "full".
/// </summary>
public class IsInventoryFull : MonoBehaviour, ISense
{
    [Tooltip("A unique name for this sense. The LLM uses this to identify the sense in the behavior tree.")]
    [SerializeField] private string _name;
    public string Name => _name;

    [Tooltip("The number of resources the AI must have for this sense to return true.")]
    [SerializeField] private int requiredResourceCount = 3;

    private GathererAI ai;

    private void Awake()
    {
        ai = GetComponent<GathererAI>();
    }

    /// <summary>
    /// Returns true if the GathererAI's ResourceCount is greater than or equal to the required amount.
    /// </summary>
    public bool Evaluate()
    {
        if (ai == null)
        {
            Debug.LogError("GathererAI component not found on this GameObject.", this);
            return false;
        }
        return ai.ResourceCount >= requiredResourceCount;
    }
}









