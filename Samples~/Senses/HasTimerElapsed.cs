using UnityEngine;

/// <summary>
/// An ISense that acts as a simple timer or cooldown. It returns false until the
/// specified duration has passed, at which point it returns true and resets.
/// </summary>
public class HasTimerElapsed : MonoBehaviour, ISense
{
    [Tooltip("A unique name for this sense. This is used by the LLM to identify the sense.")]
    [SerializeField] private string _name;
    public string Name => _name;

    [Tooltip("The duration of the timer in seconds.")]
    public float timerDuration = 5f;

    private float _startTime = -1f;

    void Awake()
    {
        // Initialize the timer so it's ready on the first check.
        _startTime = -timerDuration;
    }
    
    public bool Evaluate()
    {
        if (Time.time >= _startTime + timerDuration)
        {
            // Timer has elapsed. Reset it for the next cycle and return true.
            _startTime = Time.time;
            return true;
        }

        // Timer is still running.
        return false;
    }
} 