using UnityEngine;

[CreateAssetMenu(fileName = "Root", menuName = "NLAI/Behavior Tree/Root", order = 0)]
public class RootNode : Node
{
    public Node child;

    public override NodeStatus Execute(GameObject agent)
    {
        if (child == null)
        {
            return status = NodeStatus.FAILURE;
        }

        return status = child.Execute(agent);
    }
} 