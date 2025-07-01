using UnityEngine;

/// <summary>
/// BOILERPLATE for creating a new Action.
/// Actions are the "verbs" of your AI - what it can DO.
/// Examples: MoveToPoint, PlayAnimation, AttackTarget, Patrol, Chase.
/// 
/// CRITICAL BEHAVIOR TREE CONCEPTS:
/// 
/// 1. LOOPING: Your action is called EVERY FRAME while active!
///    - The behavior tree calls Execute() repeatedly until you return SUCCESS or FAILURE
///    - NEVER use loops inside Execute() - the behavior tree IS your loop
/// 
/// 2. WHEN TO RETURN WHAT:
///    - RUNNING: "I'm still working, call me again next frame"
///    - SUCCESS: "I'm completely done, move to next behavior"  
///    - FAILURE: "I can't do this, try a different behavior"
/// 
/// 3. SMART ACTIONS: Check your own conditions inside Execute()!
///    - Don't rely on separate sense nodes for ongoing conditions
///    - Example: Chase action should check "can I still see target?" every frame
/// </summary>
public class NewAction : MonoBehaviour, IAction
{
    [Tooltip("A unique name for this action. The LLM uses this to identify the action in the behavior tree.")]
    [SerializeField] private string _name;
    public string Name => _name;

    /// <summary>
    /// This method is called by the Behavior Tree EVERY FRAME while this action is active.
    /// It should contain the core logic for the action.
    /// 
    /// EXAMPLES OF WHEN TO RETURN EACH STATUS:
    /// 
    /// RUNNING Examples:
    /// - Moving to a destination: return RUNNING until you arrive
    /// - Playing animation: return RUNNING until animation completes
    /// - Patrolling: return RUNNING always (continuous behavior)
    /// - Chasing: return RUNNING while target visible, FAILURE when lost
    /// 
    /// SUCCESS Examples:
    /// - Reached destination: return SUCCESS when close enough
    /// - Animation finished: return SUCCESS when animation ends
    /// - Pickup item: return SUCCESS when item collected
    /// - One-shot actions: return SUCCESS immediately after execution
    /// 
    /// FAILURE Examples:
    /// - Can't reach destination: return FAILURE if path blocked
    /// - Lost chase target: return FAILURE so behavior tree tries next priority
    /// - Missing required component: return FAILURE with error log
    /// - Invalid parameters: return FAILURE with warning
    /// </summary>
    /// <returns>
    /// NodeStatus.RUNNING: Action is in progress, call Execute() again next frame
    /// NodeStatus.SUCCESS: Action completed successfully, behavior tree moves on
    /// NodeStatus.FAILURE: Action failed/impossible, behavior tree tries next option
    /// </returns>
    public NodeStatus Execute()
    {
        // TODO: Implement your action logic here.
        Debug.Log($"Executing {Name}");

        // EXAMPLE PATTERNS:

        // PATTERN 1: One-shot action (instant completion)
        // DoSomethingInstant();
        // return NodeStatus.SUCCESS;

        // PATTERN 2: Time-based action (check completion each frame)
        // if (IsActionComplete())
        //     return NodeStatus.SUCCESS;
        // else
        //     return NodeStatus.RUNNING;

        // PATTERN 3: Continuous action (runs forever until interrupted)
        // DoContinuousAction();
        // return NodeStatus.RUNNING;

        // PATTERN 4: Smart action (checks its own conditions)
        // if (!AreConditionsStillMet())
        //     return NodeStatus.FAILURE;  // Let behavior tree try next priority
        // 
        // DoContinuousAction();
        // return NodeStatus.RUNNING;

        // PATTERN 5: Conditional failure
        // if (CantDoAction())
        //     return NodeStatus.FAILURE;
        // 
        // DoAction();
        // return IsActionComplete() ? NodeStatus.SUCCESS : NodeStatus.RUNNING;

        return NodeStatus.SUCCESS;
    }
} 