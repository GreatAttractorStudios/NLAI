using UnityEngine;

/// <summary>
/// A boilerplate for creating a new Sense.
/// Senses are the "eyes and ears" of your AI, allowing it to check conditions.
/// Examples: IsEnemyVisible, IsHealthLow, HasKey.
/// </summary>
public class NewSense : MonoBehaviour, ISense
{
    [Tooltip("A unique name for this sense. The LLM uses this to identify the sense in the behavior tree.")]
    [SerializeField] private string _name;
    public string Name => _name;

    /// <summary>
    /// This method is called by the Behavior Tree. It should check a condition and return true or false.
    /// </summary>
    /// <returns>
    /// true: The condition is met (Sense evaluates to SUCCESS).
    /// false: The condition is not met (Sense evaluates to FAILURE).
    /// </returns>
    public bool Evaluate()
    {
        // TODO: Implement your condition-checking logic here.
        Debug.Log($"Evaluating {Name}");

        // Return true or false based on the condition you want to check.
        return true;
    }
} 