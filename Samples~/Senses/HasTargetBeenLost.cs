using UnityEngine;

/// <summary>
/// An ISense that checks a corresponding IsTargetInLineOfSight component to see
/// if the target has been out of sight for longer than a specified duration.
/// </summary>
public class HasTargetBeenLost : MonoBehaviour, ISense
{
    [Tooltip("A unique name for this sense. This is used by the LLM to identify the sense.")]
    [SerializeField] private string _name;
    public string Name => _name;

    [Tooltip("A reference to the Line of Sight component to check against.")]
    public IsTargetInLineOfSight lineOfSightComponent;

    [Tooltip("The duration in seconds after which the target is considered 'lost'.")]
    public float memoryDuration = 5.0f;

    public bool Evaluate()
    {
        if (lineOfSightComponent == null)
        {
            Debug.LogWarning("HasTargetBeenLost is missing a reference to a Line of Sight component.", this);
            return true; // If there's no sight component, the target is effectively lost.
        }

        // If we currently see the target, it's definitely not lost.
        if (lineOfSightComponent.Evaluate())
        {
            return false;
        }

        // Check if enough time has passed since we last saw the target.
        return Time.time > lineOfSightComponent.lastTimeSeen + memoryDuration;
    }
} 