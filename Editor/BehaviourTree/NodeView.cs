using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Eraflo.UnityImportPackage.BehaviourTree;

namespace Eraflo.UnityImportPackage.Editor.BehaviourTree
{
    /// <summary>
    /// Visual representation of a Node in the GraphView.
    /// Top-to-bottom layout with input port on top and output port on bottom.
    /// </summary>
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Action<NodeView> OnNodeSelected;
        
        public Node Node { get; private set; }
        public Port Input { get; private set; }
        public Port Output { get; private set; }
        
        private Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree _tree;
        
        public NodeView(Node node, Eraflo.UnityImportPackage.BehaviourTree.BehaviourTree tree) : base()
        {
            Node = node;
            _tree = tree;
            
            title = node.name;
            viewDataKey = node.Guid;
            
            // Set position from saved data
            style.left = node.Position.x;
            style.top = node.Position.y;
            
            // Create ports (top-to-bottom layout)
            CreateInputPorts();
            CreateOutputPorts();
            
            // Set visual style based on node type
            SetupStyles();
            
            // Add description label
            if (!string.IsNullOrEmpty(node.Description))
            {
                var descLabel = new Label(node.Description);
                descLabel.style.fontSize = 10;
                descLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                descLabel.style.whiteSpace = WhiteSpace.Normal;
                descLabel.style.maxWidth = 150;
                mainContainer.Add(descLabel);
            }
        }
        
        private void CreateInputPorts()
        {
            // All nodes except root can have an input
            Input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
            
            if (Input != null)
            {
                Input.portName = "";
                Input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(Input);
            }
        }
        
        private void CreateOutputPorts()
        {
            // Output port depends on node type
            if (Node is CompositeNode)
            {
                // Composites can have multiple children
                Output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            }
            else if (Node is DecoratorNode)
            {
                // Decorators have exactly one child
                Output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            }
            // Action and Condition nodes have no output (leaf nodes)
            
            if (Output != null)
            {
                Output.portName = "";
                Output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(Output);
            }
        }
        
        private void SetupStyles()
        {
            // Color coding based on node type
            Color headerColor;
            string typeLabel;
            
            if (Node is CompositeNode)
            {
                headerColor = new Color(0.2f, 0.4f, 0.6f); // Blue
                typeLabel = "Composite";
            }
            else if (Node is DecoratorNode)
            {
                headerColor = new Color(0.5f, 0.3f, 0.5f); // Purple
                typeLabel = "Decorator";
            }
            else if (Node is ActionNode)
            {
                headerColor = new Color(0.3f, 0.5f, 0.3f); // Green
                typeLabel = "Action";
            }
            else if (Node is ConditionNode)
            {
                headerColor = new Color(0.6f, 0.5f, 0.2f); // Yellow/Orange
                typeLabel = "Condition";
            }
            else
            {
                headerColor = new Color(0.3f, 0.3f, 0.3f);
                typeLabel = "Node";
            }
            
            // Apply header color
            var titleContainer = this.Q("title");
            if (titleContainer != null)
            {
                titleContainer.style.backgroundColor = headerColor;
            }
            
            // Add type badge
            var typeBadge = new Label(typeLabel);
            typeBadge.style.fontSize = 9;
            typeBadge.style.color = new Color(0.8f, 0.8f, 0.8f);
            typeBadge.style.marginLeft = 5;
            titleContainer?.Add(typeBadge);
            
            // Check if this is the root node
            if (_tree != null && _tree.RootNode == Node)
            {
                var rootBadge = new Label("ROOT");
                rootBadge.style.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
                rootBadge.style.color = Color.white;
                rootBadge.style.fontSize = 9;
                rootBadge.style.paddingLeft = 4;
                rootBadge.style.paddingRight = 4;
                rootBadge.style.borderTopLeftRadius = 3;
                rootBadge.style.borderTopRightRadius = 3;
                rootBadge.style.borderBottomLeftRadius = 3;
                rootBadge.style.borderBottomRightRadius = 3;
                rootBadge.style.marginLeft = 5;
                titleContainer?.Add(rootBadge);
            }
        }
        
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            
            Undo.RecordObject(Node, "Move Node");
            Node.Position = new Vector2(newPos.x, newPos.y);
            EditorUtility.SetDirty(Node);
        }
        
        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }
        
        public override void OnUnselected()
        {
            base.OnUnselected();
            OnNodeSelected?.Invoke(null);
        }
        
        /// <summary>
        /// Sorts children based on their visual X position (left to right).
        /// </summary>
        public void SortChildren()
        {
            if (Node is CompositeNode composite)
            {
                Undo.RecordObject(composite, "Sort Children");
                composite.Children.Sort((a, b) =>
                {
                    return a.Position.x.CompareTo(b.Position.x);
                });
                EditorUtility.SetDirty(composite);
            }
        }
        
        /// <summary>
        /// Updates the visual state based on runtime node state.
        /// </summary>
        public void UpdateState()
        {
            RemoveFromClassList("running");
            RemoveFromClassList("success");
            RemoveFromClassList("failure");
            
            if (Application.isPlaying)
            {
                switch (Node.State)
                {
                    case NodeState.Running:
                        if (Node.Started)
                        {
                            AddToClassList("running");
                        }
                        break;
                    case NodeState.Success:
                        AddToClassList("success");
                        break;
                    case NodeState.Failure:
                        AddToClassList("failure");
                        break;
                }
            }
        }
    }
}
