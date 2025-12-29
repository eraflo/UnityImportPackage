using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Eraflo.UnityImportPackage.BehaviourTree;

namespace Eraflo.UnityImportPackage.Editor.BehaviourTree
{
    /// <summary>
    /// Main editor window for the Behaviour Tree visual editor.
    /// </summary>
    public class BehaviourTreeEditorWindow : EditorWindow
    {
        private BehaviourTreeView _treeView;
        private InspectorView _inspectorView;
        private BlackboardView _blackboardView;
        private ToolbarMenu _assetMenu;
        private Label _treeNameLabel;
        
        private Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree _tree;
        
        [MenuItem("Tools/Unity Import Package/Behaviour Tree Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<BehaviourTreeEditorWindow>();
            window.titleContent = new GUIContent("Behaviour Tree", EditorGUIUtility.IconContent("d_Animation Icon").image);
            window.minSize = new Vector2(800, 600);
        }
        
        public static void OpenWindow(Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree tree)
        {
            var window = GetWindow<BehaviourTreeEditorWindow>();
            window.titleContent = new GUIContent("Behaviour Tree", EditorGUIUtility.IconContent("d_Animation Icon").image);
            window.minSize = new Vector2(800, 600);
            window.SelectTree(tree);
        }
        
        private void CreateGUI()
        {
            // Root container
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            
            // Toolbar
            var toolbar = CreateToolbar();
            root.Add(toolbar);
            
            // Main content area (horizontal split)
            var mainContainer = new VisualElement();
            mainContainer.style.flexDirection = FlexDirection.Row;
            mainContainer.style.flexGrow = 1;
            root.Add(mainContainer);
            
            // Left panel (Blackboard + Inspector)
            var leftPanel = CreateLeftPanel();
            mainContainer.Add(leftPanel);
            
            // Splitter
            var splitter = new VisualElement();
            splitter.style.width = 2;
            splitter.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            mainContainer.Add(splitter);
            
            // Graph view (center)
            _treeView = new BehaviourTreeView(this);
            _treeView.style.flexGrow = 1;
            _treeView.OnNodeSelected += OnNodeSelected;
            mainContainer.Add(_treeView);
            
            // Handle selection changes
            OnSelectionChange();
        }
        
        private Toolbar CreateToolbar()
        {
            var toolbar = new Toolbar();
            
            // Asset menu
            _assetMenu = new ToolbarMenu();
            _assetMenu.text = "Select Tree";
            RefreshAssetMenu();
            toolbar.Add(_assetMenu);
            
            // Tree name label
            _treeNameLabel = new Label("No tree selected");
            _treeNameLabel.style.marginLeft = 10;
            _treeNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            toolbar.Add(_treeNameLabel);
            
            // Spacer
            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);
            
            // Create new tree button
            var createButton = new ToolbarButton(() => CreateNewTree()) { text = "New Tree" };
            toolbar.Add(createButton);
            
            // Save button
            var saveButton = new ToolbarButton(() => SaveTree()) { text = "Save" };
            toolbar.Add(saveButton);
            
            return toolbar;
        }
        
        private VisualElement CreateLeftPanel()
        {
            var leftPanel = new VisualElement();
            leftPanel.style.width = 280;
            leftPanel.style.minWidth = 200;
            leftPanel.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            
            // Blackboard section
            var blackboardHeader = new Label("Blackboard");
            blackboardHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            blackboardHeader.style.marginLeft = 5;
            blackboardHeader.style.marginTop = 5;
            blackboardHeader.style.fontSize = 14;
            leftPanel.Add(blackboardHeader);
            
            _blackboardView = new BlackboardView();
            _blackboardView.style.height = 200;
            _blackboardView.style.marginBottom = 10;
            leftPanel.Add(_blackboardView);
            
            // Divider
            var divider = new VisualElement();
            divider.style.height = 2;
            divider.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            leftPanel.Add(divider);
            
            // Inspector section
            var inspectorHeader = new Label("Node Inspector");
            inspectorHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            inspectorHeader.style.marginLeft = 5;
            inspectorHeader.style.marginTop = 10;
            inspectorHeader.style.fontSize = 14;
            leftPanel.Add(inspectorHeader);
            
            _inspectorView = new InspectorView();
            _inspectorView.style.flexGrow = 1;
            leftPanel.Add(_inspectorView);
            
            return leftPanel;
        }
        
        private void RefreshAssetMenu()
        {
            _assetMenu.menu.ClearItems();
            
            var guids = AssetDatabase.FindAssets("t:BehaviourTree");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var tree = AssetDatabase.LoadAssetAtPath<Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree>(path);
                if (tree != null)
                {
                    _assetMenu.menu.AppendAction(tree.name, _ => SelectTree(tree));
                }
            }
            
            if (guids.Length == 0)
            {
                _assetMenu.menu.AppendAction("(No trees found)", null, DropdownMenuAction.Status.Disabled);
            }
        }
        
        public void SelectTree(Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree tree)
        {
            _tree = tree;
            _treeNameLabel.text = tree != null ? tree.name : "No tree selected";
            
            _treeView?.PopulateView(tree);
            _blackboardView?.UpdateView(tree);
            _inspectorView?.ClearSelection();
        }
        
        private void OnSelectionChange()
        {
            var tree = Selection.activeObject as Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree;
            if (tree != null && tree != _tree)
            {
                SelectTree(tree);
            }
        }
        
        private void OnNodeSelected(NodeView nodeView)
        {
            _inspectorView?.UpdateSelection(nodeView?.Node);
        }
        
        private void CreateNewTree()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Behaviour Tree",
                "NewBehaviourTree",
                "asset",
                "Choose a location to save the new Behaviour Tree"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                var tree = ScriptableObject.CreateInstance<Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree>();
                AssetDatabase.CreateAsset(tree, path);
                AssetDatabase.SaveAssets();
                
                RefreshAssetMenu();
                SelectTree(tree);
            }
        }
        
        private void SaveTree()
        {
            if (_tree != null)
            {
                EditorUtility.SetDirty(_tree);
                AssetDatabase.SaveAssets();
            }
        }
        
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }
        
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }
        
        private void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode)
            {
                OnSelectionChange();
            }
        }
        
        private void OnInspectorUpdate()
        {
            // Refresh blackboard during play mode
            if (Application.isPlaying && _tree != null)
            {
                _blackboardView?.UpdateView(_tree);
            }
        }
    }
}
