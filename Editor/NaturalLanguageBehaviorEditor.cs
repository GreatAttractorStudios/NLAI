using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NaturalLanguageBehavior))]
public class NaturalLanguageBehaviorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields (like the behaviorTree reference)
        base.OnInspectorGUI();

        // Get the target component
        var behavior = (NaturalLanguageBehavior)target;

        // If a behavior tree is assigned, display its description
        if (behavior.behaviorTree != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Behavior Tree Description", EditorStyles.boldLabel);
            
            // Use a disabled text area for a read-only, word-wrapped display
            GUI.enabled = false;
            EditorGUILayout.TextArea(behavior.behaviorTree.description, GUI.skin.textArea);
            GUI.enabled = true;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Behavior Tree Visualizer", EditorStyles.boldLabel);
            
            if (behavior.behaviorTree.rootNode != null)
            {
                DrawNode(behavior.behaviorTree.rootNode, 0);
            }
        }
        
        // Repaint the inspector if the application is playing to show real-time status updates.
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    private void DrawNode(Node node, int depth)
    {
        if (node == null) return;

        GUIStyle style = new GUIStyle(EditorStyles.helpBox);
        style.padding = new RectOffset(10 + depth * 20, 10, 10, 10);
        
        // Set background color based on node status
        Color originalColor = GUI.backgroundColor;
        switch (node.status)
        {
            case NodeStatus.SUCCESS:
                GUI.backgroundColor = Color.green;
                break;
            case NodeStatus.RUNNING:
                GUI.backgroundColor = Color.yellow;
                break;
            case NodeStatus.FAILURE:
                GUI.backgroundColor = Color.red;
                break;
        }

        EditorGUILayout.BeginVertical(style);
        GUI.backgroundColor = originalColor;

        string nodeLabel = $"<b>{node.GetType().Name}</b>";
        if (node is ActionNode actionNode)
        {
            nodeLabel += $": <i>{actionNode.actionName}</i>";
        }
        else if (node is SenseNode senseNode)
        {
            nodeLabel += $": <i>{senseNode.senseName}</i>";
        }
        EditorGUILayout.LabelField(new GUIContent(nodeLabel), new GUIStyle(EditorStyles.label) { richText = true });

        // Recursively draw children
        if (node is CompositeNode compositeNode)
        {
            foreach (var child in compositeNode.GetChildren())
            {
                DrawNode(child, depth + 1);
            }
        }
        else if (node is RootNode rootNode)
        {
            DrawNode(rootNode.child, depth + 1);
        }
        else if (node is InverterNode inverterNode)
        {
            DrawNode(inverterNode.child, depth + 1);
        }

        EditorGUILayout.EndVertical();
    }
} 