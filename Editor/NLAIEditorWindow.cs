using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NLAIEditorWindow : EditorWindow
{
    private string _userInput = "Patrol between two points, but chase the player if they are visible.";
    private Vector2 _scrollPosition;
    private NLAISettings _settings;
    private GameObject _contextPrefab;
    private BehaviorTree _selectedTree;
    private NaturalLanguageBehavior _selectedAgent;
    private Vector2 _viewerScrollPosition;
    private bool _resetScrollView;
    private Dictionary<Node, Rect> _boundsCache;
    private string _llmFeedback = "";
    private Vector2 _feedbackScrollPosition;
    private bool _isWaitingForLLM = false;

    // Constants for tree visualization
    private const float NodeWidth = 150;
    private const float NodeHeight = 50;
    private const float HorizontalGap = 20;
    private const float VerticalGap = 50;
    private const float Padding = 20;

    // New options for behavior structure
    private bool _loopBehavior = true;
    private enum BehaviorType { Reactive, Static }
    private BehaviorType _behaviorType = BehaviorType.Reactive;

    [MenuItem("Window/NLAI Editor")]
    public static void ShowWindow()
    {
        GetWindow<NLAIEditorWindow>("NLAI Editor");
    }

    private void OnEnable()
    {
        // Find existing settings or prompt to create one
        _settings = AssetDatabase.FindAssets("t:NLAISettings")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<NLAISettings>(path))
            .FirstOrDefault();
        
        Selection.selectionChanged += OnSelectionChanged;
        OnSelectionChanged(); // Initial check
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnGUI()
    {
        DrawGenerationPanel();
        
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        var separatorRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.horizontalSlider, GUILayout.Height(2));
        EditorGUI.DrawRect(separatorRect, new Color(0.15f, 0.15f, 0.15f, 1));
        EditorGUILayout.Space();

        DrawViewerPanel();
        
        if (Application.isPlaying) Repaint();
    }

    private void DrawGenerationPanel()
    {
        EditorGUILayout.LabelField("NLAI Settings", EditorStyles.boldLabel);

        if (_settings == null)
        {
            EditorGUILayout.HelpBox("NLAI Settings asset not found. Please create one.", MessageType.Warning);
            if (GUILayout.Button("Create NLAI Settings"))
            {
                CreateSettingsAsset();
            }
            return;
        }

        _settings = (NLAISettings)EditorGUILayout.ObjectField("Settings Asset", _settings, typeof(NLAISettings), false);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Behavior Generation Context (Optional)", EditorStyles.boldLabel);
        _contextPrefab = (GameObject)EditorGUILayout.ObjectField("Context Prefab", _contextPrefab, typeof(GameObject), false);
        EditorGUILayout.HelpBox("If you assign a prefab, NLAI will only use Actions and Senses from that prefab. If left empty, it will scan all prefabs in the project.", MessageType.Info);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Behavior Structure", EditorStyles.boldLabel);
        _loopBehavior = EditorGUILayout.Toggle("Loop Behavior", _loopBehavior);
        _behaviorType = (BehaviorType)EditorGUILayout.EnumPopup("Priority Type", _behaviorType);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Describe Behaviors (in order of priority)", EditorStyles.boldLabel);
        
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(100));
        _userInput = EditorGUILayout.TextArea(_userInput, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        GUI.enabled = !_isWaitingForLLM;
        if (GUILayout.Button("Generate Behavior Tree"))
        {
            GenerateTree();
        }
        GUI.enabled = true;

        if (_isWaitingForLLM)
        {
            EditorGUILayout.HelpBox("Waiting for LLM response...", MessageType.Info);
        }
        
        if (!string.IsNullOrEmpty(_llmFeedback))
        {
            EditorGUILayout.HelpBox("The LLM has provided feedback on your request. See below for details.", MessageType.Info);
            _feedbackScrollPosition = EditorGUILayout.BeginScrollView(_feedbackScrollPosition, GUILayout.Height(150));
            EditorGUILayout.TextArea(_llmFeedback, new GUIStyle(EditorStyles.textArea) { wordWrap = true });
            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawViewerPanel()
    {
        string viewerTitle = "Behavior Tree Viewer";
        if (_selectedTree != null)
        {
            viewerTitle += $" ({_selectedTree.name})";
        }
        EditorGUILayout.LabelField(viewerTitle, EditorStyles.boldLabel);

        // Use GUILayoutUtility to get a flexible rect for the scroll view.
        Rect viewArea = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.MinHeight(400));

        if (_selectedTree == null)
        {
            GUI.Label(viewArea, "Select a BehaviorTree asset or an agent in the scene to view.", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        if (_selectedTree.rootNode == null)
        {
            GUI.Label(viewArea, "This BehaviorTree has no root node.", EditorStyles.centeredGreyMiniLabel);
            return;
        }
        
        if (_boundsCache == null) _boundsCache = new Dictionary<Node, Rect>();

        // Define a dynamic content area for the tree based on its calculated size.
        Rect treeBounds = CalculateTreeBounds(_selectedTree.rootNode);
        Rect contentRect = new Rect(0, 0, treeBounds.width + Padding * 2, treeBounds.height + Padding * 2);

        // If a new tree has been selected, reset the scroll position to center the root node.
        if (_resetScrollView && Event.current.type == EventType.Layout)
        {
            _viewerScrollPosition = new Vector2((contentRect.width - viewArea.width) / 2, 0);
            _resetScrollView = false;
        }

        _viewerScrollPosition = GUI.BeginScrollView(viewArea, _viewerScrollPosition, contentRect);

        // Draw the nodes recursively, starting at the top-center of our content area.
        float rootDrawX = -treeBounds.x + Padding;
        DrawNode(_selectedTree.rootNode, new Vector2(rootDrawX, Padding));
        
        GUI.EndScrollView();
    }
    
    private void DrawNode(Node node, Vector2 position)
    {
        // Define the size and style of the node box
        Rect nodeRect = new Rect(position.x - NodeWidth / 2, position.y, NodeWidth, NodeHeight);

        // Set color based on the node's status if an agent is selected
        GUIStyle style = new GUIStyle(GUI.skin.box);
        if (_selectedAgent != null && Application.isPlaying)
        {
            Node runtimeNode = _selectedAgent.allNodes.Find(n => n.guid == node.guid);
            if (runtimeNode != null)
            {
                switch (runtimeNode.status)
                {
                    case NodeStatus.SUCCESS:
                        style.normal.background = MakeTex(2, 2, new Color(0.2f, 0.8f, 0.2f, 1f)); // Green
                        break;
                    case NodeStatus.RUNNING:
                        style.normal.background = MakeTex(2, 2, new Color(0.9f, 0.9f, 0.2f, 1f)); // Yellow
                        break;
                    case NodeStatus.FAILURE:
                        style.normal.background = MakeTex(2, 2, new Color(0.8f, 0.2f, 0.2f, 1f)); // Red
                        break;
                }
            }
        }

        GUI.Box(nodeRect, GetNodeTitle(node), style);

        var children = GetChildren(node);
        if (children == null || children.Count == 0) return;

        var childPositions = GetChildPositions(node, position);
        for (int i = 0; i < children.Count; i++)
        {
            Vector2 childPosition = childPositions[i];
            DrawConnection(nodeRect, new Rect(childPosition.x, childPosition.y, 0, 0));
            DrawNode(children[i], childPosition);
        }
    }
    
    private void DrawConnection(Rect start, Rect end)
    {
        Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height, 0);
        Vector3 endPos = new Vector3(end.x, end.y, 0);
        Vector3 startTan = startPos + Vector3.up * (VerticalGap * 0.75f);
        Vector3 endTan = endPos + Vector3.down * (VerticalGap * 0.75f);
        Handles.DrawBezier(startPos, endPos, endTan, startTan, Color.gray, null, 2f);
    }

    private List<Node> GetChildren(Node parent)
    {
        if (parent is CompositeNode composite) return composite.children;
        if (parent is RootNode root && root.child != null) return new List<Node> { root.child };
        if (parent is InverterNode inverter && inverter.child != null) return new List<Node> { inverter.child };
        return null;
    }

    private string GetNodeTitle(Node node)
    {
        if (node is ActionNode action) return $"Action:\n{action.actionName}";
        if (node is SenseNode sense) return $"Sense:\n{sense.senseName}";
        return node.GetType().Name;
    }
    
    private Rect CalculateTreeBounds(Node rootNode)
    {
        if (rootNode == null) return Rect.zero;
        if (_boundsCache.ContainsKey(rootNode)) return _boundsCache[rootNode];

        Rect bounds = new Rect(0,0,0,0);
        MeasureNode(rootNode, Vector2.zero, ref bounds);
        
        _boundsCache[rootNode] = bounds;
        return bounds;
    }

    private void MeasureNode(Node node, Vector2 position, ref Rect bounds)
    {
        if (node == null) return;
        
        var nodeRect = new Rect(position.x - NodeWidth / 2, position.y, NodeWidth, NodeHeight);

        if (bounds.width == 0) // First node
        {
            bounds = nodeRect;
        }
        else
        {
            bounds = Rect.MinMaxRect(
                Mathf.Min(bounds.xMin, nodeRect.xMin),
                Mathf.Min(bounds.yMin, nodeRect.yMin),
                Mathf.Max(bounds.xMax, nodeRect.xMax),
                Mathf.Max(bounds.yMax, nodeRect.yMax));
        }
        
        var children = GetChildren(node);
        if (children == null || children.Count == 0) return;

        var childPositions = GetChildPositions(node, position);
        for (int i = 0; i < children.Count; i++)
        {
            MeasureNode(children[i], childPositions[i], ref bounds);
        }
    }
    
    private List<Vector2> GetChildPositions(Node parentNode, Vector2 parentPosition)
    {
        var positions = new List<Vector2>();
        var children = GetChildren(parentNode);
        if (children == null || children.Count == 0) return positions;

        List<Rect> childBounds = children.Select(c => CalculateTreeBounds(c)).ToList();
        float totalWidth = childBounds.Sum(b => b.width) + Mathf.Max(0, children.Count - 1) * HorizontalGap;
        
        float currentX = parentPosition.x - totalWidth / 2;

        for (int i = 0; i < children.Count; i++)
        {
            Rect childBound = childBounds[i];
            
            // Position the child's origin so its subtree is correctly placed.
            Vector2 childPosition = new Vector2(currentX - childBound.x + childBound.width / 2, parentPosition.y + NodeHeight + VerticalGap);
            positions.Add(childPosition);

            currentX += childBound.width + HorizontalGap;
        }
        return positions;
    }
    
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void OnSelectionChanged()
    {
        bool changed = false;
        if (Selection.activeObject is BehaviorTree tree)
        {
            if (_selectedTree != tree)
            {
                _selectedTree = tree;
                _selectedAgent = null; // Clear agent if we select a new tree asset
                changed = true;
            }
        }
        else if (Selection.activeGameObject != null)
        {
            var agent = Selection.activeGameObject.GetComponent<NaturalLanguageBehavior>();
            if (agent != null && agent.behaviorTree != _selectedTree)
            {
                _selectedTree = agent.behaviorTree;
                _selectedAgent = agent;
                changed = true;
            }
        }
        
        if (changed)
        {
            if (_boundsCache != null) _boundsCache.Clear();
            _resetScrollView = true;
            Repaint();
        }
    }

    private void CreateSettingsAsset()
    {
        NLAISettings newSettings = CreateInstance<NLAISettings>();
        
        if (!AssetDatabase.IsValidFolder("Assets/NLAI"))
        {
            AssetDatabase.CreateFolder("Assets", "NLAI");
        }
        if (!AssetDatabase.IsValidFolder("Assets/NLAI/GeneratedTrees"))
        {
            AssetDatabase.CreateFolder("Assets/NLAI", "GeneratedTrees");
        }

        AssetDatabase.CreateAsset(newSettings, "Assets/NLAI/New NLAI Settings.asset");
        AssetDatabase.SaveAssets();
        _settings = newSettings;
        Selection.activeObject = newSettings;
        EditorUtility.FocusProjectWindow();
    }

    private async void GenerateTree()
    {
        List<string> senses;
        List<string> actions;

        if (_contextPrefab != null)
        {
            // Context-specific scan
            senses = _contextPrefab.GetComponentsInChildren<ISense>(true)
                .Select(s => s.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct().ToList();
            actions = _contextPrefab.GetComponentsInChildren<IAction>(true)
                .Select(a => a.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct().ToList();
        }
        else
        {
            // Global scan
            senses = FindAllMonoBehavioursInPrefabs()
                .OfType<ISense>()
                .Select(s => s.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToList();
            
            actions = FindAllMonoBehavioursInPrefabs()
                .OfType<IAction>()
                .Select(a => a.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToList();
        }
        
        Debug.Log($"Using Senses: {string.Join(", ", senses)}");
        Debug.Log($"Using Actions: {string.Join(", ", actions)}");
        
        _llmFeedback = ""; // Clear previous feedback
        _isWaitingForLLM = true;
        Repaint();

        try
        {
            var (generatedTree, feedback) = await LLMCommunicator.ConvertDescriptionToTree(
                _userInput,
                _loopBehavior,
                _behaviorType == BehaviorType.Reactive,
                senses,
                actions,
                _settings);

            if (!string.IsNullOrEmpty(feedback))
            {
                _llmFeedback = feedback;
                Repaint();
                return;
            }

            // Safeguard against an invalid or empty tree being generated
            bool isTreeValid = generatedTree != null && !(generatedTree is RootNode rootNode && rootNode.child == null);

            if (!isTreeValid)
            {
                _llmFeedback = "The LLM failed to generate a valid tree. This can happen if the prompt is ambiguous or requires components that are not available. Please review your prompt and the list of available components in the console log, then try again.";
                Repaint();
                return;
            }

            if (generatedTree != null)
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "Save Behavior Tree",
                    "NewBehaviorTree",
                    "asset",
                    "Please enter a file name to save the behavior tree to.",
                    "Assets/NLAI/GeneratedTrees");

                if (!string.IsNullOrEmpty(path))
                {
                    var tree = CreateInstance<BehaviorTree>();
                    tree.description = _userInput;
                    AssetDatabase.CreateAsset(tree, path);
                    tree.rootNode = generatedTree;
                    SaveNodeRecursive(tree, generatedTree);

                    AssetDatabase.SaveAssets();
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = tree;
                }
            }
        }
        finally
        {
            _isWaitingForLLM = false;
            Repaint();
        }
    }

    private void SaveNodeRecursive(Object mainAsset, Node node)
    {
        if (node == null) return;
        
        AssetDatabase.AddObjectToAsset(node, mainAsset);

        List<Node> children = null;
        if (node is CompositeNode composite) children = composite.GetChildren();
        else if (node is InverterNode inverter && inverter.child != null) children = new List<Node> { inverter.child };
        else if (node is RootNode root && root.child != null) children = new List<Node> { root.child };

        if (children != null)
        {
            foreach (var child in children)
            {
                SaveNodeRecursive(mainAsset, child);
            }
        }
    }
    
    public static List<MonoBehaviour> FindAllMonoBehavioursInPrefabs()
    {
        return AssetDatabase.FindAssets("t:Prefab")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path))
            .Where(prefab => prefab != null)
            .SelectMany(prefab => prefab.GetComponentsInChildren<MonoBehaviour>(true))
            .ToList();
    }
} 