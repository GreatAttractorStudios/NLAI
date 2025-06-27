using System.Collections.Generic;
using UnityEngine;

public abstract class CompositeNode : Node
{
    public List<Node> children = new List<Node>();

    public override void Reset()
    {
        foreach (var child in children)
        {
            child.Reset();
        }
    }

    public void SetChildren(List<Node> newChildren)
    {
        children = newChildren;
    }

    public List<Node> GetChildren()
    {
        return children;
    }
} 