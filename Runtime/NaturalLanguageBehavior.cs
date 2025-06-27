using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The main component to attach to an NPC. It holds the natural language description,
/// a reference to the compiled BehaviorTree, and executes the tree at runtime.
/// </summary>
public class NaturalLanguageBehavior : MonoBehaviour
{
    [Tooltip("The compiled Behavior Tree asset generated from the description.")]
    public BehaviorTree behaviorTree;

    // These lists will be populated at runtime with the available actions and senses on this agent.
    [HideInInspector] public List<IAction> actions = new List<IAction>();
    [HideInInspector] public List<ISense> senses = new List<ISense>();

    private Dictionary<string, IAction> _actions = new Dictionary<string, IAction>();
    private Dictionary<string, ISense> _senses = new Dictionary<string, ISense>();

    [HideInInspector] public List<Node> allNodes = new List<Node>();

    void Awake()
    {
        actions = GetComponents<IAction>().ToList();
        senses = GetComponents<ISense>().ToList();

        foreach (var action in actions)
        {
            if (string.IsNullOrEmpty(action.Name))
            {
                Debug.LogError($"Found an IAction component ('{action.GetType().Name}') on GameObject '{this.name}' with a null or empty Name. Please assign a unique name in the inspector.", this);
                continue;
            }
            if (_actions.ContainsKey(action.Name))
            {
                Debug.LogError($"Found a duplicate IAction with the name '{action.Name}' on GameObject '{this.name}'. Action names must be unique. Component: {action.GetType().Name}", this);
                continue;
            }
            _actions.Add(action.Name, action);
        }

        foreach (var sense in senses)
        {
            if (string.IsNullOrEmpty(sense.Name))
            {
                Debug.LogError($"Found an ISense component ('{sense.GetType().Name}') on GameObject '{this.name}' with a null or empty Name. Please assign a unique name in the inspector.", this);
                continue;
            }
            if (_senses.ContainsKey(sense.Name))
            {
                Debug.LogError($"Found a duplicate ISense with the name '{sense.Name}' on GameObject '{this.name}'. Sense names must be unique. Component: {sense.GetType().Name}", this);
                continue;
            }
            _senses.Add(sense.Name, sense);
        }

        if (behaviorTree != null && behaviorTree.rootNode != null)
        {
            PopulateAllNodes(behaviorTree.rootNode);
        }
    }

    void Start()
    {
        if (behaviorTree != null && behaviorTree.rootNode != null)
        {
            behaviorTree.rootNode.Reset();
        }
    }

    private void PopulateAllNodes(Node node)
    {
        if (node == null) return;

        allNodes.Add(node);

        if (node is RootNode root) PopulateAllNodes(root.child);
        if (node is InverterNode inverter) PopulateAllNodes(inverter.child);
        if (node is CompositeNode composite)
        {
            foreach (var child in composite.children)
            {
                PopulateAllNodes(child);
            }
        }
    }

    public IAction GetAction(string actionName)
    {
        _actions.TryGetValue(actionName, out var action);
        return action;
    }

    public ISense GetSense(string senseName)
    {
        _senses.TryGetValue(senseName, out var sense);
        return sense;
    }

    void Update()
    {
        if (behaviorTree != null && behaviorTree.rootNode != null)
        {
            behaviorTree.rootNode.Execute(gameObject);
        }
    }
} 