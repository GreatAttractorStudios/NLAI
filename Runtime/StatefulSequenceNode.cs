using UnityEngine;

/// <summary>
/// A sequence node that remembers its last running child and resumes from it.
/// If a child returns FAILURE, it resets and starts from the beginning on the next evaluation.
/// </summary>
[CreateAssetMenu(fileName = "StatefulSequence", menuName = "NLAI/Behavior Tree/Stateful Sequence", order = 3)]
public class StatefulSequenceNode : CompositeNode
{
    private int _lastRunningChildIndex = 0;

    public override void Reset()
    {
        _lastRunningChildIndex = 0;
        foreach (var child in children)
        {
            child.Reset();
        }
    }

    public override NodeStatus Execute(GameObject agent)
    {
        for (int i = _lastRunningChildIndex; i < children.Count; i++)
        {
            var childStatus = children[i].Execute(agent);

            switch (childStatus)
            {
                case NodeStatus.FAILURE:
                    _lastRunningChildIndex = 0; // Reset on failure
                    return NodeStatus.FAILURE;
                
                case NodeStatus.RUNNING:
                    _lastRunningChildIndex = i; // Remember running child
                    return NodeStatus.RUNNING;
                
                case NodeStatus.SUCCESS:
                    continue; // Continue to the next child
            }
        }
        
        // If the loop completes, the entire sequence was successful
        _lastRunningChildIndex = 0; // Reset for the next run
        return NodeStatus.SUCCESS;
    }
} 