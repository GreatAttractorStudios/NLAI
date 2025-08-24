using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(BehaviorTree))]
public class BehaviorTreeInspector : Editor
{
    private bool showVisualTree = true;
    private Vector2 scrollPosition;

    public override void OnInspectorGUI()
    {
        BehaviorTree behaviorTree = (BehaviorTree)target;
        
        // Draw only the relevant fields, not all sub-assets
        EditorGUILayout.LabelField("Behavior Tree", EditorStyles.boldLabel);
        
        // Show the description field
        EditorGUILayout.LabelField("Description", EditorStyles.miniBoldLabel);
        EditorGUILayout.TextArea(behaviorTree.description, EditorStyles.wordWrappedLabel);
        
        EditorGUILayout.Space();
        
        // Visual tree toggle
        showVisualTree = EditorGUILayout.Toggle("Show Visual Tree", showVisualTree);
        
        // Open visual graph window button
        if (GUILayout.Button("Open Visual Graph Window"))
        {
            BehaviorTreeGraphWindow.OpenWindow(behaviorTree);
        }
        
        if (showVisualTree && behaviorTree.rootNode != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tree Structure", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
            
            DrawNode(behaviorTree.rootNode, 0);
            
            EditorGUILayout.EndScrollView();
        }
        else if (showVisualTree)
        {
            EditorGUILayout.HelpBox("No root node assigned to this behavior tree.", MessageType.Info);
        }
    }
    
    private void DrawNode(Node node, int depth)
    {
        if (node == null) return;
        
        // Indent based on depth
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(depth * 20);
        
        // Node icon and type
        string nodeType = GetNodeTypeName(node);
        Texture2D icon = GetNodeIcon(node);
        
        if (icon != null)
        {
            GUILayout.Label(icon, GUILayout.Width(16), GUILayout.Height(16));
        }
        
        // Node name
        string nodeName = GetNodeDisplayName(node);
        EditorGUILayout.LabelField(nodeName);
        
        EditorGUILayout.EndHorizontal();
        
        // Draw children if they exist
        if (HasChildren(node))
        {
            DrawNodeChildren(node, depth + 1);
        }
    }
    
    private void DrawNodeChildren(Node node, int depth)
    {
        if (node is RootNode rootNode)
        {
            DrawNode(rootNode.child, depth);
        }
        else if (node is InverterNode inverterNode)
        {
            DrawNode(inverterNode.child, depth);
        }
        else if (node is CompositeNode compositeNode)
        {
            foreach (var child in compositeNode.children)
            {
                DrawNode(child, depth);
            }
        }
    }
    
    private string GetNodeTypeName(Node node)
    {
        if (node is RootNode) return "Root";
        if (node is PrioritySelectorNode) return "PrioritySelector";
        if (node is StatefulSequenceNode) return "StatefulSequence";
        if (node is InverterNode) return "Inverter";
        if (node is ActionNode) return "Action";
        if (node is SenseNode) return "Sense";
        return node.GetType().Name;
    }
    
    private string GetNodeDisplayName(Node node)
    {
        string typeName = GetNodeTypeName(node);
        
        if (node is ActionNode actionNode)
        {
            return $"{typeName}: {actionNode.actionName}";
        }
        else if (node is SenseNode senseNode)
        {
            return $"{typeName}: {senseNode.senseName}";
        }
        
        return typeName;
    }
    
    private Texture2D GetNodeIcon(Node node)
    {
        // Helper method to safely get icons with fallback
        Texture2D GetIconSafely(string iconName, string fallbackName = "DefaultAsset Icon")
        {
            try
            {
                var content = EditorGUIUtility.IconContent(iconName);
                return content?.image as Texture2D ?? EditorGUIUtility.IconContent(fallbackName).image as Texture2D;
            }
            catch
            {
                return EditorGUIUtility.IconContent(fallbackName).image as Texture2D;
            }
        }

        // Assign icons for different node types using valid Unity icon names
        if (node is ActionNode) return GetIconSafely("PlayButton");
        if (node is SenseNode) return GetIconSafely("ViewToolOrbit", "Search Icon");
        if (node is PrioritySelectorNode) return GetIconSafely("UnityEditor.ConsoleWindow", "Folder Icon");
        if (node is StatefulSequenceNode) return GetIconSafely("UnityEditor.AnimationWindow", "Animation.Record");
        if (node is RootNode) return GetIconSafely("Transform Icon");
        if (node is InverterNode) return GetIconSafely("Refresh", "DefaultAsset Icon");
        
        return GetIconSafely("DefaultAsset Icon");
    }
    
    private bool HasChildren(Node node)
    {
        if (node is RootNode rootNode) return rootNode.child != null;
        if (node is InverterNode inverterNode) return inverterNode.child != null;
        if (node is CompositeNode compositeNode) return compositeNode.children != null && compositeNode.children.Count > 0;
        return false;
    }
} 