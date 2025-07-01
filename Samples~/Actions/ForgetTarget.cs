using UnityEngine;

/// <summary>
/// Resets the EnemyDetector's memory, making the AI "forget" about the target.
/// Use this to cleanly transition from chase back to patrol.
/// </summary>
public class ForgetTarget : MonoBehaviour, IAction
{
    [Tooltip("The unique name for this action")]
    [SerializeField] private string _name = "ForgetTarget";
    public string Name => _name;

    [Header("Settings")]
    [Tooltip("Reference to the EnemyDetector component to reset")]
    public EnemyDetector enemyDetector;

    void Start()
    {
        if (enemyDetector == null)
            enemyDetector = GetComponent<EnemyDetector>();
    }

    public NodeStatus Execute()
    {
        if (enemyDetector == null)
        {
            Debug.LogError("ForgetTarget needs a reference to an EnemyDetector component!", this);
            return NodeStatus.FAILURE;
        }

        // Reset the detector's memory
        enemyDetector.ForgetTarget();
        
        Debug.Log("Target forgotten - returning to patrol");
        return NodeStatus.SUCCESS;
    }
} 