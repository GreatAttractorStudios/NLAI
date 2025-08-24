using UnityEngine;

/// <summary>
/// Sense to check if the AI is currently holding a resource.
/// It queries the GathererAI state script.
/// </summary>
public class HasResource : MonoBehaviour, ISense
{
    [Tooltip("A unique name for this sense. The LLM uses this to identify the sense in the behavior tree.")]
    [SerializeField] private string _name;
    public string Name => _name;

    private GathererAI ai;

    private void Awake()
    {
        ai = GetComponent<GathererAI>();
    }

    /// <summary>
    /// Returns true if the GathererAI's ResourceCount is greater than zero.
    /// </summary>
    public bool Evaluate()
    {
        if (ai == null)
        {
            Debug.LogError("GathererAI component not found on this GameObject.", this);
            return false;
        }
        return ai.ResourceCount > 0;
    }
} 