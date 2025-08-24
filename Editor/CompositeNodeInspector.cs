using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor(typeof(CompositeNode), true)]
public class CompositeNodeInspector : Editor
{
    private ReorderableList childList;

    private void OnEnable()
    {
        var compositeNode = target as CompositeNode;
        if (compositeNode != null)
        {
            childList = new ReorderableList(serializedObject, serializedObject.FindProperty("children"), true, true, true, true);
            
            childList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Child Nodes");
            };
            
            childList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = childList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                
                // Node type and name
                var node = element.objectReferenceValue as Node;
                string displayName = node != null ? GetNodeDisplayName(node) : "None";
                
                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight), 
                    displayName, node, typeof(Node), false);
                if (EditorGUI.EndChangeCheck())
                {
                    element.objectReferenceValue = newValue;
                }
                
                // Remove button
                if (GUI.Button(new Rect(rect.x + rect.width - 50, rect.y, 50, EditorGUIUtility.singleLineHeight), "Remove"))
                {
                    childList.serializedProperty.DeleteArrayElementAtIndex(index);
                }
            };
            
            childList.onAddCallback = (ReorderableList list) =>
            {
                var index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = index;
                
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                element.objectReferenceValue = null;
            };
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        
        // Draw child list
        if (childList != null)
        {
            childList.DoLayoutList();
        }
        
        // Add quick creation buttons
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Add", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Action Node"))
        {
            AddChildNode<ActionNode>("New Action");
        }
        if (GUILayout.Button("Add Sense Node"))
        {
            AddChildNode<SenseNode>("New Sense");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Priority Selector"))
        {
            AddChildNode<PrioritySelectorNode>("New Priority Selector");
        }
        if (GUILayout.Button("Add Stateful Sequence"))
        {
            AddChildNode<StatefulSequenceNode>("New Stateful Sequence");
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Add Inverter"))
        {
            AddChildNode<InverterNode>("New Inverter");
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void AddChildNode<T>(string defaultName) where T : Node
    {
        var compositeNode = target as CompositeNode;
        if (compositeNode != null)
        {
            var newNode = CreateInstance<T>();
            newNode.name = defaultName;
            
            // Save the asset
            string path = AssetDatabase.GetAssetPath(target);
            string directory = System.IO.Path.GetDirectoryName(path);
            string fileName = $"{defaultName}_{System.Guid.NewGuid().ToString().Substring(0, 8)}.asset";
            string fullPath = $"{directory}/{fileName}";
            
            AssetDatabase.CreateAsset(newNode, fullPath);
            AssetDatabase.SaveAssets();
            
            // Add to children list
            var childrenProperty = serializedObject.FindProperty("children");
            childrenProperty.arraySize++;
            childrenProperty.GetArrayElementAtIndex(childrenProperty.arraySize - 1).objectReferenceValue = newNode;
            
            serializedObject.ApplyModifiedProperties();
        }
    }
    
    private string GetNodeDisplayName(Node node)
    {
        if (node == null) return "None";
        
        if (node is ActionNode actionNode)
            return $"Action: {actionNode.actionName}";
        if (node is SenseNode senseNode)
            return $"Sense: {senseNode.senseName}";
        if (node is PrioritySelectorNode)
            return "Priority Selector";
        if (node is StatefulSequenceNode)
            return "Stateful Sequence";
        if (node is InverterNode)
            return "Inverter";
        if (node is RootNode)
            return "Root";
        
        return node.GetType().Name;
    }
} 