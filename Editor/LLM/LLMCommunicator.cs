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
        NLAISettings settings)
    {
        if (settings == null || string.IsNullOrEmpty(settings.apiKey))
        {
            Debug.LogError("API key is not set. Please set it in the NLAI Settings.");
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
You are an expert in Behavior Trees. Your task is to convert a user's description of NPC behavior into a valid JSON structure.

First, analyze the user's request against the provided list of Senses and Actions.

1.  If all parts of the request can be fully implemented with the available components, your output must ONLY be a single JSON object representing the Behavior Tree.
2.  If ANY part of the request requires logic for which no component exists (e.g., 'jump', 'open a door', 'check for a key'), you MUST NOT generate a tree. Instead, your output must ONLY be a single JSON object with a 'feedback' field. This field should contain a friendly, high-level explanation of what's missing and guide the user on how they could create it. Do NOT provide code templates. For example, if the user asks for the character to 'run away' from an enemy:
    {{
      ""feedback"": ""It looks like you're asking the character to 'run away', but there isn't an Action for that. To implement this, you could create a new C# script that implements the IAction interface. Inside its Execute() method, you would need to get the enemy's position, calculate a destination away from them, and then use the NavMeshAgent to move there. Alternatively, you could use an existing movement action if it can be given a destination.""
    }}

{promptHeader}
The user will describe the branches of the main {rootNodeType}. You must assemble these into a valid tree.

- Available Senses: {string.Join(", ", senses)}
- Available Actions: {string.Join(", ", actions)}

- 'PrioritySelector': A reactive selector. It executes children in order from left to right. It will always re-evaluate from the start.
- 'StatefulSequence': A sequence with memory. It executes children in order and resumes where it left off.
- 'Inverter': A decorator that inverts the result: SUCCESS becomes FAILURE, and FAILURE becomes SUCCESS.
- 'Root': A special decorator that re-runs its child indefinitely. Always make this the top-level node if the user wants looping behavior.
- Senses have a 'type' of 'Sense'.
- Actions have a 'type' of 'Action'.";

        var (success, jsonResponse) = await NLAIEdHttp.InvokeLLM(systemPrompt, description, settings);

        if (!success)
        {
            Debug.LogError("Error communicating with the LLM.");
            return (null, null);
        }

        try
        {
            JObject json = JObject.Parse(jsonResponse);
            
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

            if (node is InverterNode inverter)
            {
                if (childNodes.Count > 0) inverter.child = childNodes[0];
            }
            else if (node is RootNode root)
            {
                if (childNodes.Count > 0) root.child = childNodes[0];
            }
            else if (node is CompositeNode composite)
            {
                composite.SetChildren(childNodes);
            }
        }
        
        return node;
    }
} 