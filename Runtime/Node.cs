using UnityEngine;

public abstract class Node : ScriptableObject
{
    [HideInInspector] public NodeStatus status = NodeStatus.SUCCESS;
    [HideInInspector] public string guid;

    public virtual NodeStatus Execute(GameObject agent)
    {
        // Default implementation does nothing, can be overridden.
        return NodeStatus.SUCCESS;
    }
    
    public virtual void Reset() { }

    private void OnEnable()
    {
        // Ensure every node has a unique ID when it's created or loaded.
        if (string.IsNullOrEmpty(guid))
        {
            guid = System.Guid.NewGuid().ToString();
        }
    }
} 