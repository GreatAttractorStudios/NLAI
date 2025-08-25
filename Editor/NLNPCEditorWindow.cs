using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NLNPCEditorWindow : EditorWindow
{
    private string _userInput = "Patrol between two points, but chase the player if they are visible.";
    private Vector2 _scrollPosition;
    private NLNPCSettings _settings;
    private GameObject _contextPrefab;

    private string _llmFeedback = "";
    private Vector2 _feedbackScrollPosition;
    private bool _isWaitingForLLM = false;



    // New options for behavior structure
    private bool _loopBehavior = true;
    private enum BehaviorType { Reactive, Static }
    private BehaviorType _behaviorType = BehaviorType.Reactive;

    [MenuItem("Window/NLNPC Editor")]
    public static void ShowWindow()
    {
        GetWindow<NLNPCEditorWindow>("NLNPC Editor");
    }

    private void OnEnable()
    {
        // Find existing settings or prompt to create one
        _settings = AssetDatabase.FindAssets("t:NLNPCSettings")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<NLNPCSettings>(path))
            .FirstOrDefault();
        

    }



    private void OnGUI()
    {
        DrawGenerationPanel();
    }

    private void DrawGenerationPanel()
    {
        EditorGUILayout.LabelField("NLNPC Settings", EditorStyles.boldLabel);

        if (_settings == null)
        {
            EditorGUILayout.HelpBox("NLNPC Settings asset not found. Please create one.", MessageType.Warning);
            if (GUILayout.Button("Create NLNPC Settings"))
            {
                CreateSettingsAsset();
            }
            return;
        }

        _settings = (NLNPCSettings)EditorGUILayout.ObjectField("Settings Asset", _settings, typeof(NLNPCSettings), false);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Target NPC Prefab", EditorStyles.boldLabel);
        _contextPrefab = (GameObject)EditorGUILayout.ObjectField("NPC Prefab", _contextPrefab, typeof(GameObject), false);
        
        if (_contextPrefab == null)
        {
            EditorGUILayout.HelpBox("Please assign the NPC prefab that will use this behavior tree. NLNPC needs to know what Actions and Senses are available on your specific NPC.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("NLNPC will generate a behavior tree using only the Actions and Senses found on this prefab.", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Behavior Structure", EditorStyles.boldLabel);
        
        // Loop Behavior with tooltip
        GUIContent loopContent = new GUIContent("Loop Behavior", 
            "When enabled, the behavior tree will continuously loop and restart from the beginning after completion. " +
            "Disable this for one-time behaviors that should stop after finishing.");
        _loopBehavior = EditorGUILayout.Toggle(loopContent, _loopBehavior);
        
        // Priority Type with tooltip
        GUIContent priorityContent = new GUIContent("Priority Type", 
            "Reactive: Continuously re-evaluates all conditions every frame, allowing instant priority changes. " +
            "Static: Evaluates conditions once and sticks with the chosen behavior until it completes.");
        _behaviorType = (BehaviorType)EditorGUILayout.EnumPopup(priorityContent, _behaviorType);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Describe Behaviors (in order of priority)", EditorStyles.boldLabel);
        
        // Create a text area with proper word wrapping and fixed width
        GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.wordWrap = true;
        
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(100));
        _userInput = EditorGUILayout.TextArea(_userInput, textAreaStyle, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        EditorGUILayout.EndScrollView();

        GUI.enabled = !_isWaitingForLLM && _contextPrefab != null;
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





    private void CreateSettingsAsset()
    {
        NLNPCSettings newSettings = CreateInstance<NLNPCSettings>();
        
        if (!AssetDatabase.IsValidFolder("Assets/NLNPC"))
        {
            AssetDatabase.CreateFolder("Assets", "NLNPC");
        }
        if (!AssetDatabase.IsValidFolder("Assets/NLNPC/GeneratedTrees"))
        {
            AssetDatabase.CreateFolder("Assets/NLNPC", "GeneratedTrees");
        }

        AssetDatabase.CreateAsset(newSettings, "Assets/NLNPC/New NLNPC Settings.asset");
        AssetDatabase.SaveAssets();
        _settings = newSettings;
        Selection.activeObject = newSettings;
        EditorUtility.FocusProjectWindow();
    }

    private async void GenerateTree()
    {
        // Scan the assigned NPC prefab for available actions and senses
        List<string> senses = _contextPrefab.GetComponentsInChildren<ISense>(true)
            .Select(s => s.Name)
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct().ToList();
            
        List<string> actions = _contextPrefab.GetComponentsInChildren<IAction>(true)
            .Select(a => a.Name)
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct().ToList();
        
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
                    "Assets/NLNPC/GeneratedTrees");

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

} 