using UnityEngine;

/// <summary>
/// An ISense that checks if a character's health is below a certain percentage.
/// Requires a component on this GameObject that has a 'currentHealth' and 'maxHealth' public property.
/// </summary>
public class IsHealthBelowPercentage : MonoBehaviour, ISense
{
    [Tooltip("A unique name for this sense. This is used by the LLM to identify the sense.")]
    [SerializeField] private string _name;
    public string Name => _name;

    [Tooltip("The Health component to check.")]
    public Health healthComponent;

    [Tooltip("The health percentage threshold (0 to 1).")]
    [Range(0f, 1f)]
    public float healthThreshold = 0.25f;

    public bool Evaluate()
    {
        if (healthComponent == null)
        {
            Debug.LogWarning("IsHealthBelowPercentage is missing a reference to a Health component.", this);
            return false;
        }

        if (healthComponent.maxHealth <= 0) return false;

        return (healthComponent.currentHealth / healthComponent.maxHealth) <= healthThreshold;
    }
} 