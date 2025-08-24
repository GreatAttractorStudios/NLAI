using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;

public class BehaviorTreeGraphWindow : EditorWindow
{
    private BehaviorTree behaviorTree;
    private BehaviorTreeGraphView graphView;
    private Toolbar toolbar;

    [MenuItem("Window/NLAI/Behavior Tree Visualizer")]
    public static void OpenWindow()
    {
        var window = GetWindow<BehaviorTreeGraphWindow>();
        window.titleContent = new GUIContent("Behavior Tree Visualizer");
        window.Show();
    }

    public static void OpenWindow(BehaviorTree tree)
    {
        var window = GetWindow<BehaviorTreeGraphWindow>();
        window.titleContent = new GUIContent($"Visualizer - {tree.name}");
        window.behaviorTree = tree;
        window.Show();
        window.CreateGraph();
    }

    private void CreateGUI()
    {
        // Create toolbar
        toolbar = new Toolbar();
        rootVisualElement.Add(toolbar);

        // Add behavior tree field
        var treeField = new ObjectField("Behavior Tree")
        {
            objectType = typeof(BehaviorTree),
            value = behaviorTree
        };
        treeField.RegisterValueChangedCallback(evt =>
        {
            behaviorTree = evt.newValue as BehaviorTree;
            CreateGraph();
        });
        toolbar.Add(treeField);

        // Add refresh button
        var refreshButton = new Button(() => CreateGraph()) { text = "Refresh" };
        toolbar.Add(refreshButton);

        // Create graph view
        graphView = new BehaviorTreeGraphView
        {
            name = "Behavior Tree Graph"
        };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);

        if (behaviorTree != null)
        {
            CreateGraph();
        }
    }

    private void CreateGraph()
    {
        if (graphView == null) return;
        
        graphView.ClearGraph();

        if (behaviorTree == null)
        {
            titleContent = new GUIContent("Behavior Tree Visualizer");
            return;
        }

        titleContent = new GUIContent($"Visualizer - {behaviorTree.name}");

        if (behaviorTree.rootNode == null) return;

        // Start the root node on the left side with some padding
        CreateNodeRecursive(behaviorTree.rootNode, new Vector2(-400, 0));
        graphView.FrameAll();
    }

    private BehaviorTreeNode CreateNodeRecursive(Node node, Vector2 position)
    {
        if (node == null) return null;

        var graphNode = new BehaviorTreeNode(node, position);
        graphView.AddElement(graphNode);

        // Create child nodes with left-to-right layout
        if (node is RootNode rootNode)
        {
            var childNode = CreateNodeRecursive(rootNode.child, position + new Vector2(200, 0));
            if (childNode != null && graphNode.outputPort != null)
            {
                var edge = graphNode.outputPort.ConnectTo(childNode.inputPort);
                graphView.AddElement(edge);
            }
        }
        else if (node is InverterNode inverterNode)
        {
            // Add an even larger vertical offset for inverter children to prevent overlapping
            var childNode = CreateNodeRecursive(inverterNode.child, position + new Vector2(200, 100));
            if (childNode != null && graphNode.outputPort != null)
            {
                var edge = graphNode.outputPort.ConnectTo(childNode.inputPort);
                graphView.AddElement(edge);
            }
        }
        else if (node is StatefulSequenceNode sequenceNode)
        {
            // For sequences, create a horizontal chain to show execution flow (Sense → Action → Sense → Action)
            Vector2 childPosition = position + new Vector2(200, 0); // Restored horizontal spacing
            BehaviorTreeNode previousNode = graphNode; // Start chain from the sequence node

            foreach (var child in sequenceNode.children)
            {
                var childGraphNode = CreateNodeRecursive(child, childPosition);
                if (childGraphNode == null) continue;

                // Connect to the previous node in the chain to show execution flow
                if (previousNode.outputPort != null)
                {
                    var edge = previousNode.outputPort.ConnectTo(childGraphNode.inputPort);
                    graphView.AddElement(edge);
                }
                
                previousNode = childGraphNode;
                childPosition.x += 200; // Restored horizontal spacing for better readability
            }
        }
        else if (node is CompositeNode compositeNode) // Handles Priority Selectors
        {
            // For priority selectors, spread children vertically with reduced spacing
            float childSpacing = 200f; // Reduced vertical spacing for more compact layout
            float totalHeight = (compositeNode.children.Count - 1) * childSpacing;
            float startY = position.y - totalHeight / 2f;

            for (int i = 0; i < compositeNode.children.Count; i++)
            {
                var childNode = CreateNodeRecursive(compositeNode.children[i], 
                    new Vector2(position.x + 200, startY + i * childSpacing));
                if (childNode != null && graphNode.outputPort != null)
                {
                    var edge = graphNode.outputPort.ConnectTo(childNode.inputPort);
                    graphView.AddElement(edge);
                }
            }
        }

        return graphNode;
    }
}

public class BehaviorTreeGraphView : GraphView
{
    public BehaviorTreeGraphView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        // Removed SelectionDragger and RectangleSelector to prevent editing
        
        // Make the view read-only
        this.viewDataKey = "BehaviorTreeGraphView";

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
    }

    // Disable port connections by returning empty list
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return new List<Port>(); // Return empty list to disable connections
    }
    
    // Override to prevent deletion and editing
    protected override bool canDeleteSelection => false;
    
    // Override key events to disable editing shortcuts
    protected override void ExecuteDefaultActionAtTarget(EventBase evt)
    {
        // Disable default actions like delete, copy, paste
        if (evt is KeyDownEvent keyEvent)
        {
            if (keyEvent.keyCode == KeyCode.Delete || 
                keyEvent.keyCode == KeyCode.Backspace ||
                (keyEvent.ctrlKey && (keyEvent.keyCode == KeyCode.C || 
                                     keyEvent.keyCode == KeyCode.V || 
                                     keyEvent.keyCode == KeyCode.X)))
            {
                evt.StopPropagation();
                return;
            }
        }
        base.ExecuteDefaultActionAtTarget(evt);
    }

    public void ClearGraph()
    {
        DeleteElements(nodes.ToList());
        DeleteElements(edges.ToList());
    }
}

public class BehaviorTreeNode : UnityEditor.Experimental.GraphView.Node
{
    public Port inputPort;
    public Port outputPort;
    private Node behaviorNode;

    public BehaviorTreeNode(Node node, Vector2 position)
    {
        behaviorNode = node;
        SetPosition(new Rect(position, Vector2.zero));

        // Set title and style first
        title = GetNodeTitle(node);
        SetNodeStyle(node);
        
        // Force dark grey text color on the title label using schedule
        Color darkGrey = new Color(0.2f, 0.2f, 0.2f, 1f);
        this.schedule.Execute(() => {
            var titleLabel = this.Q<Label>();
            if (titleLabel != null)
            {
                titleLabel.style.color = darkGrey;
            }
            titleContainer.style.color = darkGrey;
        }).ExecuteLater(1);
        
        // Hide the collapse button as it's not used
        titleButtonContainer.style.display = DisplayStyle.None;

        // Make the node non-movable for read-only visualization
        capabilities &= ~Capabilities.Movable;
        capabilities &= ~Capabilities.Deletable;
        capabilities &= ~Capabilities.Selectable;

        // Create input port (each node has one parent) - disabled for interaction
        inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
        inputPort.portName = "";
        inputPort.SetEnabled(false); // Disable port interaction
        inputContainer.Add(inputPort);

        // Create output port based on node type - disabled for interaction
        if (node is CompositeNode)
        {
            outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        }
        else if (node is RootNode || node is InverterNode || node is SenseNode || node is ActionNode)
        {
            outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
        }

        // All nodes can have output ports for sequence chaining
        if (outputPort != null)
        {
            outputPort.portName = "";
            outputPort.SetEnabled(false); // Disable port interaction
            outputContainer.Add(outputPort);
        }
    }

    private string GetNodeTitle(Node node)
    {
        if (node is ActionNode actionNode)
            return $"Action: {actionNode.actionName}";
        if (node is SenseNode senseNode)
            return $"Sense: {senseNode.senseName}";
        if (node is RootNode)
            return "Root";
        if (node is PrioritySelectorNode)
            return "Priority Selector";
        if (node is StatefulSequenceNode)
            return "Stateful Sequence";
        if (node is InverterNode)
            return "Inverter";
        
        return node.GetType().Name;
    }

    private void SetNodeStyle(Node node)
    {
        // Set black text color for all nodes - try multiple approaches
        titleContainer.style.color = Color.black;
        titleContainer.style.unityTextAlign = TextAnchor.MiddleCenter;
        
        // Set different colors for different node types
        if (node is ActionNode)
        {
            titleContainer.style.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        }
        else if (node is SenseNode)
        {
            titleContainer.style.backgroundColor = new Color(0.9f, 0.5f, 0.5f, 0.8f); // Lighter red
        }
        else if (node is PrioritySelectorNode)
        {
            titleContainer.style.backgroundColor = new Color(0.5f, 0.5f, 0.9f, 0.8f); // Lighter blue
        }
        else if (node is StatefulSequenceNode)
        {
            titleContainer.style.backgroundColor = new Color(0.8f, 0.8f, 0.2f, 0.8f);
        }
        else if (node is RootNode)
        {
            titleContainer.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        }
        else if (node is InverterNode)
        {
            titleContainer.style.backgroundColor = new Color(0.9f, 0.7f, 0.9f, 0.8f); // Lighter magenta/pink
        }
    }
} 