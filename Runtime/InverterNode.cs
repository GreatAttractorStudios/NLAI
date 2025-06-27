using UnityEngine;

public class InverterNode : Node
{
    public Node child;

    public override NodeStatus Execute(GameObject agent)
    {
        if (child == null) return status = NodeStatus.FAILURE;

        var childStatus = child.Execute(agent);

        switch (childStatus)
        {
            case NodeStatus.SUCCESS:
                return status = NodeStatus.FAILURE;
            case NodeStatus.FAILURE:
                return status = NodeStatus.SUCCESS;
            default:
                return status = childStatus; // Running
        }
    }
} 