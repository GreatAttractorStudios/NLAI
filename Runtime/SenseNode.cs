using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Sense", menuName = "NLNPC/Behavior Tree/Sense", order = 6)]
public class SenseNode : Node
{
    public string senseName;

    public override NodeStatus Execute(GameObject agent)
    {
        var behaviorManager = agent.GetComponent<NaturalLanguageBehavior>();
        if (behaviorManager == null)
        {
            Debug.LogError("SenseNode requires a NaturalLanguageBehavior component on the agent.", agent);
            return NodeStatus.FAILURE;
        }

        var sense = behaviorManager.senses.FirstOrDefault(s => s.Name == senseName);

        if (sense == null)
        {
            Debug.LogWarning($"SenseNode: Could not find a registered ISense with the name '{senseName}' on the agent's NaturalLanguageBehavior component. Check if the component exists and the name is correct.", agent);
            return NodeStatus.FAILURE;
        }
        
        status = sense.Evaluate() ? NodeStatus.SUCCESS : NodeStatus.FAILURE;
        return status;
    }
} 