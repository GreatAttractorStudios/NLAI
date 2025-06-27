using UnityEngine;

[CreateAssetMenu(fileName = "New Behavior Tree", menuName = "NLAI/Behavior Tree/Tree", order = -1)]
public class BehaviorTree : ScriptableObject
{
    public Node rootNode;
    
    [TextArea(3, 10)]
    [Tooltip("The description used to generate this tree.")]
    public string description;
} 