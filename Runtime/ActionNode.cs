using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Action", menuName = "NLAI/Behavior Tree/Action", order = 5)]
public class ActionNode : Node
{
    public string actionName;

    public override NodeStatus Execute(GameObject agent)
    {
        var behaviorManager = agent.GetComponent<NaturalLanguageBehavior>();
        if (behaviorManager == null)
        {
            Debug.LogError("ActionNode requires a NaturalLanguageBehavior component on the agent.", agent);
            return NodeStatus.FAILURE;
        }

        var action = behaviorManager.actions.FirstOrDefault(a => a.Name == actionName);

        if (action == null)
        {
            Debug.LogWarning($"ActionNode: Could not find a registered IAction with the name '{actionName}' on the agent's NaturalLanguageBehavior component. Check if the component exists and the name is correct.", agent);
            return NodeStatus.FAILURE;
        }
        
        status = action.Execute();
        return status;
    }
} 