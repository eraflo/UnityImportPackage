using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Eraflo.UnityImportPackage.BehaviourTree;

namespace Eraflo.UnityImportPackage.Editor.BehaviourTree
{
    /// <summary>
    /// GraphView implementation for the Behaviour Tree visual editor.
    /// Displays nodes in a top-to-bottom hierarchy.
    /// </summary>
    public class BehaviourTreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits> { }
        
        public Action<NodeView> OnNodeSelected;
        
        private Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree _tree;
        private BehaviourTreeEditorWindow _editorWindow;
        private NodeSearchWindow _searchWindow;
        
        public BehaviourTreeView(BehaviourTreeEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;
            
            // Add manipulators for zoom, drag, selection
            Insert(0, new GridBackground());
            
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            
            // Add stylesheet
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Packages/com.eraflo.unityimportpackage/Editor/BehaviourTree/BehaviourTreeEditor.uss"
            );
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }
            
            // Set default styles
            style.flexGrow = 1;
            style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            
            // Undo handling
            Undo.undoRedoPerformed += OnUndoRedo;
        }
        
        /// <summary>
        /// Initializes the search window with the current tree.
        /// Must be called after tree is set.
        /// </summary>
        public void InitializeSearchWindow()
        {
            if (_tree == null) return;
            
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Initialize(this, _tree);
            
            // Add node creation shortcut (Space key)
            this.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Space && _tree != null)
                {
                    SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(evt.localMousePosition)), _searchWindow);
                }
            });
        }
        
        ~BehaviourTreeView()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }
        
        private void OnUndoRedo()
        {
            PopulateView(_tree);
            AssetDatabase.SaveAssets();
        }
        
        public void PopulateView(Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree tree)
        {
            _tree = tree;
            
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;
            
            if (tree == null) return;
            
            // Create node views
            foreach (var node in tree.Nodes)
            {
                if (node != null)
                {
                    CreateNodeView(node);
                }
            }
            
            // Create edges
            foreach (var node in tree.Nodes)
            {
                if (node == null) continue;
                
                var children = GetChildren(node);
                var parentView = FindNodeView(node);
                
                foreach (var child in children)
                {
                    var childView = FindNodeView(child);
                    if (parentView != null && childView != null)
                    {
                        var edge = parentView.Output.ConnectTo(childView.Input);
                        AddElement(edge);
                    }
                }
            }
            
            // Initialize search window for this tree
            InitializeSearchWindow();
        }
        
        private List<Node> GetChildren(Node node)
        {
            var children = new List<Node>();
            
            if (node is CompositeNode composite)
            {
                children.AddRange(composite.Children.Where(c => c != null));
            }
            else if (node is DecoratorNode decorator && decorator.Child != null)
            {
                children.Add(decorator.Child);
            }
            
            return children;
        }
        
        private NodeView FindNodeView(Node node)
        {
            return GetNodeByGuid(node.Guid) as NodeView;
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node
            ).ToList();
        }
        
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            // Handle removed elements
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is NodeView nodeView)
                    {
                        DeleteNode(nodeView.Node);
                    }
                    
                    if (element is Edge edge)
                    {
                        var parentView = edge.output.node as NodeView;
                        var childView = edge.input.node as NodeView;
                        
                        if (parentView != null && childView != null)
                        {
                            RemoveChild(parentView.Node, childView.Node);
                        }
                    }
                }
            }
            
            // Handle new edges
            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    var parentView = edge.output.node as NodeView;
                    var childView = edge.input.node as NodeView;
                    
                    if (parentView != null && childView != null)
                    {
                        AddChild(parentView.Node, childView.Node);
                    }
                }
            }
            
            // Handle moved nodes
            if (graphViewChange.movedElements != null)
            {
                foreach (var element in graphViewChange.movedElements)
                {
                    if (element is NodeView nodeView)
                    {
                        nodeView.SortChildren();
                    }
                }
            }
            
            return graphViewChange;
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (_tree == null) return;
            
            // Open node search window
            evt.menu.AppendAction("Create Node...", _ =>
            {
                if (_searchWindow != null)
                {
                    SearchWindow.Open(new SearchWindowContext(evt.mousePosition), _searchWindow);
                }
            });
            
            evt.menu.AppendSeparator();
            
            // Set root
            if (selection.Count == 1 && selection[0] is NodeView selectedNode)
            {
                evt.menu.AppendAction("Set as Root", _ =>
                {
                    Undo.RecordObject(_tree, "Set Root");
                    _tree.RootNode = selectedNode.Node;
                    EditorUtility.SetDirty(_tree);
                    PopulateView(_tree); // Refresh to show root badge
                });
            }
        }
        
        private void CreateNode(Type type)
        {
            var node = _tree.CreateNode(type);
            CreateNodeView(node);
        }
        
        private void CreateNodeView(Node node)
        {
            var nodeView = new NodeView(node, _tree);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }
        
        private void DeleteNode(Node node)
        {
            if (_tree.RootNode == node)
            {
                _tree.RootNode = null;
            }
            
            _tree.DeleteNode(node);
        }
        
        private void AddChild(Node parent, Node child)
        {
            Undo.RecordObject(parent, "Add Child");
            
            if (parent is CompositeNode composite)
            {
                composite.Children.Add(child);
            }
            else if (parent is DecoratorNode decorator)
            {
                decorator.Child = child;
            }
            
            EditorUtility.SetDirty(parent);
        }
        
        private void RemoveChild(Node parent, Node child)
        {
            Undo.RecordObject(parent, "Remove Child");
            
            if (parent is CompositeNode composite)
            {
                composite.Children.Remove(child);
            }
            else if (parent is DecoratorNode decorator)
            {
                decorator.Child = null;
            }
            
            EditorUtility.SetDirty(parent);
        }
    }
}
