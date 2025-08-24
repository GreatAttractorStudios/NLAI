using UnityEngine;

/// <summary>
/// Sense to check if there is a resource within a specified range.
/// This uses a simple overlap sphere to detect objects with the "Resource" tag.
/// </summary>
public class CanSeeResource : MonoBehaviour, ISense
{
    [Tooltip("A unique name for this sense. The LLM uses this to identify the sense in the behavior tree.")]
    [SerializeField] private string _name;
    public string Name => _name;
    
    [Tooltip("The range within which the AI can detect resources.")]
    [SerializeField] private float detectionRange = 10f;

    /// <summary>
    /// Returns true if any GameObject with the "Resource" tag is within the detection range.
    /// </summary>
    public bool Evaluate()
    {
        // Find all colliders within the detection range on the "Default" layer
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange);
        foreach (var col in colliders)
        {
            // Check if the detected object has the "Resource" tag
            if (col.CompareTag("Resource"))
            {
                return true; // Found a resource
            }
        }
        return false; // No resources found in range
    }
}