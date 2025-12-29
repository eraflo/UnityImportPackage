using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Eraflo.UnityImportPackage.BehaviourTree;

namespace Eraflo.UnityImportPackage.Editor.BehaviourTree
{
    /// <summary>
    /// Panel that displays the inspector for the selected node.
    /// </summary>
    public class InspectorView : VisualElement
    {
        private UnityEditor.Editor _editor;
        private IMGUIContainer _container;
        private Node _currentNode;
        
        public InspectorView()
        {
            style.flexGrow = 1;
            style.paddingLeft = 5;
            style.paddingRight = 5;
            style.paddingTop = 5;
            style.paddingBottom = 5;
            
            // Placeholder message
            var placeholder = new Label("Select a node to view its properties");
            placeholder.name = "placeholder";
            placeholder.style.color = new Color(0.5f, 0.5f, 0.5f);
            placeholder.style.unityTextAlign = TextAnchor.MiddleCenter;
            placeholder.style.marginTop = 20;
            Add(placeholder);
        }
        
        public void UpdateSelection(Node node)
        {
            _currentNode = node;
            
            // Clear previous editor
            if (_container != null)
            {
                Remove(_container);
                _container = null;
            }
            
            if (_editor != null)
            {
                Object.DestroyImmediate(_editor);
                _editor = null;
            }
            
            // Show placeholder if no selection
            var placeholder = this.Q<Label>("placeholder");
            if (node == null)
            {
                if (placeholder == null)
                {
                    placeholder = new Label("Select a node to view its properties");
                    placeholder.name = "placeholder";
                    placeholder.style.color = new Color(0.5f, 0.5f, 0.5f);
                    placeholder.style.unityTextAlign = TextAnchor.MiddleCenter;
                    placeholder.style.marginTop = 20;
                    Add(placeholder);
                }
                return;
            }
            
            // Remove placeholder
            placeholder?.RemoveFromHierarchy();
            
            // Create editor for selected node
            _editor = UnityEditor.Editor.CreateEditor(node);
            
            _container = new IMGUIContainer(() =>
            {
                if (_editor != null && _editor.target != null)
                {
                    // Draw header
                    EditorGUILayout.BeginVertical("box");
                    
                    string nodeType = GetNodeTypeName(node);
                    EditorGUILayout.LabelField($"{nodeType}: {node.name}", EditorStyles.boldLabel);
                    
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space();
                    
                    // Name field
                    EditorGUI.BeginChangeCheck();
                    string newName = EditorGUILayout.TextField("Name", node.name);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(node, "Rename Node");
                        node.name = newName;
                        EditorUtility.SetDirty(node);
                    }
                    
                    EditorGUILayout.Space();
                    
                    // Draw properties (excluding base class fields)
                    DrawNodeProperties(_editor);
                }
            });
            
            Add(_container);
        }
        
        public void ClearSelection()
        {
            UpdateSelection(null);
        }
        
        private void DrawNodeProperties(UnityEditor.Editor editor)
        {
            var serializedObject = editor.serializedObject;
            serializedObject.Update();
            
            var iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                
                // Skip base class fields
                if (iterator.name == "m_Script") continue;
                if (iterator.name == "State") continue;
                if (iterator.name == "Started") continue;
                if (iterator.name == "Guid") continue;
                if (iterator.name == "Position") continue;
                if (iterator.name == "Children") continue;
                if (iterator.name == "Child") continue;
                
                EditorGUILayout.PropertyField(iterator, true);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private string GetNodeTypeName(Node node)
        {
            if (node is CompositeNode) return "Composite";
            if (node is DecoratorNode) return "Decorator";
            if (node is ActionNode) return "Action";
            if (node is ConditionNode) return "Condition";
            return "Node";
        }
    }
}
