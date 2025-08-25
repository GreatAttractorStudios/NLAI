using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A "reactive" selector that re-evaluates its children from the beginning on every tick.
/// This allows a high-priority task to interrupt a running low-priority task.
/// It will execute children in order until one of them returns SUCCESS or RUNNING.
/// </summary>
[CreateAssetMenu(fileName = "PrioritySelector", menuName = "NLNPC/Behavior Tree/Priority Selector", order = 1)]
public class PrioritySelectorNode : CompositeNode
{
    public override NodeStatus Execute(GameObject agent)
    {
        foreach (var child in children)
        {
            var childStatus = child.Execute(agent);
            if (childStatus != NodeStatus.FAILURE)
            {
                return status = childStatus;
            }
        }
        
        return status = NodeStatus.FAILURE;
    }
} 