using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Eraflo.UnityImportPackage.BehaviourTree;

namespace Eraflo.UnityImportPackage.Editor.BehaviourTree
{
    /// <summary>
    /// Search window for creating new nodes in the Behaviour Tree.
    /// Automatically detects all node types, including custom nodes with [BehaviourTreeNode] attribute.
    /// </summary>
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private BehaviourTreeView _graphView;
        private Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree _tree;
        private Texture2D _indentationIcon;
        
        // Cache of discovered node types
        private static List<NodeTypeInfo> _cachedNodeTypes;
        private static bool _cacheBuilt = false;
        
        private struct NodeTypeInfo
        {
            public Type Type;
            public string DisplayName;
            public string Category;
            public string Description;
        }
        
        public void Initialize(BehaviourTreeView graphView, Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree tree)
        {
            _graphView = graphView;
            _tree = tree;
            
            // Create a blank icon for indentation
            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, Color.clear);
            _indentationIcon.Apply();
            
            // Build cache if not already done
            if (!_cacheBuilt)
            {
                BuildNodeTypeCache();
            }
        }
        
        /// <summary>
        /// Forces a rebuild of the node type cache.
        /// Call this after adding new node types.
        /// </summary>
        public static void RefreshNodeTypes()
        {
            _cacheBuilt = false;
            BuildNodeTypeCache();
        }
        
        private static void BuildNodeTypeCache()
        {
            _cachedNodeTypes = new List<NodeTypeInfo>();
            
            // Find all node types in all assemblies
            var nodeBaseType = typeof(Node);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && nodeBaseType.IsAssignableFrom(t));
                    
                    foreach (var type in types)
                    {
                        var info = new NodeTypeInfo
                        {
                            Type = type,
                            DisplayName = type.Name,
                            Category = GetDefaultCategory(type),
                            Description = ""
                        };
                        
                        // Check for attribute
                        var attr = type.GetCustomAttribute<BehaviourTreeNodeAttribute>();
                        if (attr != null)
                        {
                            if (!string.IsNullOrEmpty(attr.DisplayName))
                                info.DisplayName = attr.DisplayName;
                            if (!string.IsNullOrEmpty(attr.Category))
                                info.Category = attr.Category;
                            if (!string.IsNullOrEmpty(attr.Description))
                                info.Description = attr.Description;
                        }
                        
                        // Also check CreateAssetMenu for display name
                        var menuAttr = type.GetCustomAttribute<CreateAssetMenuAttribute>();
                        if (menuAttr != null && !string.IsNullOrEmpty(menuAttr.fileName))
                        {
                            if (attr == null || string.IsNullOrEmpty(attr.DisplayName))
                                info.DisplayName = menuAttr.fileName;
                        }
                        
                        _cachedNodeTypes.Add(info);
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip assemblies that can't be loaded
                }
            }
            
            // Sort by category then name
            _cachedNodeTypes = _cachedNodeTypes
                .OrderBy(n => n.Category)
                .ThenBy(n => n.DisplayName)
                .ToList();
            
            _cacheBuilt = true;
        }
        
        private static string GetDefaultCategory(Type type)
        {
            if (typeof(CompositeNode).IsAssignableFrom(type)) return "Composites";
            if (typeof(DecoratorNode).IsAssignableFrom(type)) return "Decorators";
            if (typeof(ActionNode).IsAssignableFrom(type)) return "Actions";
            if (typeof(ConditionNode).IsAssignableFrom(type)) return "Conditions";
            return "Other";
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0)
            };
            
            // Group nodes by category
            var categories = _cachedNodeTypes
                .GroupBy(n => n.Category)
                .OrderBy(g => g.Key);
            
            foreach (var category in categories)
            {
                // Add category header
                tree.Add(new SearchTreeGroupEntry(new GUIContent(category.Key), 1));
                
                // Add nodes in category
                foreach (var nodeInfo in category)
                {
                    tree.Add(CreateEntry(nodeInfo.DisplayName, nodeInfo.Type, 2, nodeInfo.Description));
                }
            }
            
            return tree;
        }
        
        private SearchTreeEntry CreateEntry(string name, Type type, int level, string tooltip = "")
        {
            var entry = new SearchTreeEntry(new GUIContent(name, _indentationIcon, tooltip))
            {
                level = level,
                userData = type
            };
            return entry;
        }
        
        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (_tree == null || _graphView == null) return false;
            
            // Get window position
            var worldMousePosition = context.screenMousePosition;
            var windowRoot = _graphView.parent;
            var localMousePosition = windowRoot.WorldToLocal(worldMousePosition - _graphView.worldBound.position);
            
            // Convert to graph coordinates
            var graphMousePosition = _graphView.viewTransform.matrix.inverse.MultiplyPoint(localMousePosition);
            
            // Create the node
            var type = entry.userData as Type;
            if (type != null)
            {
                var node = _tree.CreateNode(type);
                node.Position = graphMousePosition;
                
                // Create view
                var nodeView = new NodeView(node, _tree);
                nodeView.OnNodeSelected = _graphView.OnNodeSelected;
                _graphView.AddElement(nodeView);
                
                return true;
            }
            
            return false;
        }
    }
}
