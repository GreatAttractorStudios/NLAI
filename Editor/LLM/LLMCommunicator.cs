using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;

public static class LLMCommunicator
{
    public static async System.Threading.Tasks.Task<(Node tree, string feedback)> ConvertDescriptionToTree(
        string description,
        bool loop,
        bool isReactive,
        List<string> senses,
        List<string> actions,
        NLNPCSettings settings)
    {
        if (settings == null || string.IsNullOrEmpty(settings.apiKey))
        {
            Debug.LogError("API key is not set. Please set it in the NLNPC Settings.");
            return (null, null);
        }

        // Programmatically construct the boilerplate part of the prompt
        string rootNodeType = isReactive ? "PrioritySelector" : "StatefulSequence";
        string promptHeader = $"The user wants a behavior tree. The main logic should be a {rootNodeType}.";
        if (loop)
        {
            promptHeader += $" This entire logic should be wrapped in a 'Root' node to make it run continuously.";
        }

        var systemPrompt = $@"
You are an expert in Behavior Trees and AI behavior design. Your task is to convert a user's description of NPC behavior into a valid JSON structure.

CRITICAL: Understand these common AI behavior patterns when interpreting user requests:

1. **Chase Behaviors**: When a user says 'chase an enemy' or 'pursue a target', this typically means:
   - Continuously move toward the target WHILE checking if it's still visible/detectable
   - If the target becomes undetectable during the chase, the chase should end
   - Use StatefulSequence with the detection sense FIRST, then the chase action

2. **State Transitions**: Phrases like 'if target is lost', 'when enemy disappears', 'lose sight' refer to:
   - Conditions that should be checked DURING other actions (like chasing)
   - NOT separate, independent branches that run in parallel
   - The transition FROM one state (chasing) TO another state (patrolling)

3. **Continuous Behaviors**: Actions like 'patrol', 'wander', 'guard' should:
   - Be implemented as single, intelligent Actions that handle their own looping
   - NOT be broken down into sequences of individual waypoint movements
   - Return RUNNING while active, SUCCESS when complete (if ever)

4. **Priority-Based Logic**: When users describe priorities ('first do X, but if Y, then Z'):
   - Use PrioritySelector where higher priority conditions are checked first
   - Each priority branch should be a complete behavior (sense + action sequence)

EXAMPLE INTERPRETATIONS:
- 'Chase enemy, if target lost, return to patrol' = PrioritySelector with:
  Branch 1: StatefulSequence[CanSeeEnemy → Chase] 
  Branch 2: Patrol (default behavior)
- 'If health low, flee. Otherwise patrol' = PrioritySelector with:
  Branch 1: StatefulSequence[IsHealthLow → Flee]
  Branch 2: Patrol

IMPORTANT: For chase behaviors, NEVER create separate 'HasLostTarget' or 'TargetLost' branches. The chase should automatically stop when the detection sense fails.

First, analyze the user's request against the provided list of Senses and Actions.

- If all parts of the request can be fully implemented with the available components, your output must ONLY be a single JSON object representing the Behavior Tree.
- If ANY part of the request requires logic for which no component exists, you MUST NOT generate a tree. Instead, your output must ONLY be a single JSON object with a 'feedback' field.

The feedback must be a clear, user-friendly guide on how to create the missing components. It should follow these rules:
1.  Start by acknowledging the user's goal (e.g., 'It looks like you want to create an AI that can patrol, chase enemies, and flee when hurt.').
2.  List the specific Senses and Actions that are required but not available.
3.  For EACH missing component, provide a conceptual recipe. Explain its purpose, what data it might need, and its core logic. Do NOT provide code, only high-level descriptions.
4.  If the user asks for a complex behavior like 'patrolling', explain how they could build a dedicated, intelligent Action for it.

For example, if the user asks for the character to 'flee' and 'patrol':
    {{
""feedback"": ""It looks like you want an AI that can patrol and flee. To do that, you'll need a few new components:\n\n1.  **A Sense for Low Health:** This would be a Sense component that needs a reference to a 'Health' script. Its job is to check if the current health is below a certain percentage and return SUCCESS if it is.\n\n2.  **A 'Flee' Action:** This would be an Action component. It would need a reference to the enemy's location and a list of safe points. Its logic would find the safest point farthest from the enemy and tell the NavMeshAgent to move there.\n\n3.  **A 'Patrol' Action:** For a continuous patrol, it's best to create a dedicated Action. This component would hold a list of waypoint transforms. Its logic would be to move to the next waypoint in the list each time it arrives at one, creating a continuous loop.""
    }}

{promptHeader}
The user will describe the branches of the main {rootNodeType}. You must assemble these into a valid tree.

- Available Senses: {string.Join(", ", senses)}
- Available Actions: {string.Join(", ", actions)}

JSON Structure Rules:
- The top-level element MUST be a single JSON object representing the root node of the tree.
- Every node MUST have a 'type' property (e.g., 'Root', 'PrioritySelector', 'StatefulSequence', 'Inverter', 'Action', 'Sense').
- Action nodes MUST have a 'name' property with the action's name. Example: {{ ""type"": ""Action"", ""name"": ""MyAction"" }}
- Sense nodes MUST have a 'name' property with the sense's name. Example: {{ ""type"": ""Sense"", ""name"": ""MySense"" }}
- Root and Inverter nodes MUST have a single 'child' property containing their child node.
- PrioritySelector and StatefulSequence nodes MUST have a 'children' property, which is an array of their child nodes.";

        var (success, jsonResponse) = await NLNPCEdHttp.InvokeLLM(systemPrompt, description, settings);

        if (!success)
        {
            Debug.LogError("Error communicating with the LLM.");
            return (null, null);
        }

        try
        {
            JObject json = JObject.Parse(jsonResponse);
            Debug.Log($"LLM Raw Response:\n{jsonResponse}");
            
            // Check if the LLM provided feedback instead of a tree
            if (json["feedback"] != null)
            {
                string feedback = (string)json["feedback"];
                return (null, feedback);
            }
            
            return (CreateNode(json, actions, senses), null);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse LLM response as JSON: {e.Message}");
            Debug.LogError($"LLM Response was: {jsonResponse}");
            return (null, null);
        }
    }

    private static Node CreateNode(JObject jsonNode, List<string> availableActions, List<string> availableSenses)
    {
        string type = (string)jsonNode["type"];
        Node node = null;

        switch (type)
        {
            case "PrioritySelector":
                node = ScriptableObject.CreateInstance<PrioritySelectorNode>();
                break;
            case "StatefulSequence":
                node = ScriptableObject.CreateInstance<StatefulSequenceNode>();
                break;
            case "Inverter":
                node = ScriptableObject.CreateInstance<InverterNode>();
                break;
            case "Root":
                node = ScriptableObject.CreateInstance<RootNode>();
                break;
            case "Action":
                string actionName = (string)jsonNode["name"];
                if (!availableActions.Contains(actionName))
                {
                    Debug.LogError($"LLM tried to create an Action node with an unavailable action: '{actionName}'. Check if the action exists and its name matches exactly.");
                    return null;
                }
                ActionNode actionNode = ScriptableObject.CreateInstance<ActionNode>();
                actionNode.actionName = actionName;
                node = actionNode;
                break;
            case "Sense":
                string senseName = (string)jsonNode["name"];
                if (!availableSenses.Contains(senseName))
                {
                    Debug.LogError($"LLM tried to create a Sense node with an unavailable sense: '{senseName}'. Check if the sense exists and its name matches exactly.");
                    return null;
                }
                SenseNode senseNode = ScriptableObject.CreateInstance<SenseNode>();
                senseNode.senseName = senseName;
                node = senseNode;
                break;
            default:
                Debug.LogError($"Unknown node type received from LLM: {type}");
                return null;
        }

        // Handle children for composite nodes
        if (jsonNode["children"] is JArray children)
        {
            var childNodes = new List<Node>();
            foreach (var childJson in children)
            {
                Node childNode = CreateNode((JObject)childJson, availableActions, availableSenses);
                if (childNode != null)
                {
                    childNodes.Add(childNode);
                }
            }
            if (node is CompositeNode composite)
            {
                composite.SetChildren(childNodes);
            }
        }
        // Handle child for decorator nodes
        else if (jsonNode["child"] is JObject child)
        {
            Node childNode = CreateNode(child, availableActions, availableSenses);
            if (childNode != null)
            {
                if (node is RootNode root)
                {
                    root.child = childNode;
                }
                else if (node is InverterNode inverter)
                {
                    inverter.child = childNode;
                }
            }
        }
        
        return node;
    }
} 